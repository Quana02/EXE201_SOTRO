using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoTro_BE.Data;
using SoTro_BE.DTOs.Auth;
using SoTro_BE.DTOs.Invoice;
using SoTro_BE.Models;

namespace SoTro_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InvoicesController : ControllerBase
    {
        private readonly SoTroDbContext _context;

        public InvoicesController(SoTroDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<InvoiceResponse>>>> GetInvoices([FromQuery] int buildingId, [FromQuery] int month, [FromQuery] int year)
        {
            var landlordId = GetLandlordId();
            if (landlordId == null)
                return Unauthorized(ApiResponse<List<InvoiceResponse>>.Fail("Không tìm thấy thông tin chủ trọ."));

            if (!await OwnsBuildingAsync(buildingId, landlordId.Value))
                return NotFound(ApiResponse<List<InvoiceResponse>>.Fail("Không tìm thấy nhà trọ hoặc bạn không có quyền truy cập."));

            await RefreshOverdueStatusAsync(buildingId, landlordId.Value, month, year);
            var invoices = await QueryInvoices(buildingId, landlordId.Value, month, year).ToListAsync();
            return Ok(ApiResponse<List<InvoiceResponse>>.Ok("Lấy danh sách hóa đơn thành công.", invoices));
        }

        [HttpPost("generate-monthly")]
        public async Task<ActionResult<ApiResponse<List<InvoiceResponse>>>> GenerateMonthlyInvoices([FromBody] GenerateMonthlyInvoicesRequest request)
        {
            var landlordId = GetLandlordId();
            if (landlordId == null)
                return Unauthorized(ApiResponse<List<InvoiceResponse>>.Fail("Không tìm thấy thông tin chủ trọ."));

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<List<InvoiceResponse>>.Fail("Dữ liệu tạo hóa đơn không hợp lệ."));

            if (!await OwnsBuildingAsync(request.BuildingId, landlordId.Value))
                return NotFound(ApiResponse<List<InvoiceResponse>>.Fail("Không tìm thấy nhà trọ hoặc bạn không có quyền truy cập."));

            var rentals = (await _context.RentalRecords
                .Include(r => r.Room)
                .Include(r => r.Tenant)
                .Where(r =>
                    r.IsDeleted != true &&
                    r.Status == "Active" &&
                    r.Room != null &&
                    r.Room.BuildingId == request.BuildingId &&
                    r.Room.IsDeleted != true &&
                    r.Tenant != null &&
                    r.Tenant.LandlordId == landlordId.Value &&
                    r.Tenant.IsDeleted != true)
                .ToListAsync())
                .GroupBy(r => r.RoomId)
                .Select(g => g.OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt ?? DateTime.MinValue).First())
                .ToList();

            var reservedInvoiceCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var rental in rentals)
            {
                if (rental.RoomId == null)
                    continue;

                var exists = await _context.Invoices.AnyAsync(i =>
                    i.LandlordId == landlordId.Value &&
                    i.RoomId == rental.RoomId &&
                    i.Month == request.Month &&
                    i.Year == request.Year &&
                    i.IsDeleted != true);

                if (exists)
                    continue;

                var roomPrice = rental.MonthlyRent ?? rental.Room?.BasePrice ?? 0;
                var electricCost = (rental.Room?.ElectricPrice ?? 0) * 30;
                var waterCost = (rental.Room?.WaterPrice ?? 0) * 4;
                var serviceCost = request.TrashFee + request.InternetFee + request.CleaningFee + request.ManagementFee;
                var total = roomPrice + electricCost + waterCost + serviceCost;
                var dueDate = ResolveDueDate(rental, request.Month, request.Year);
                var invoice = new Invoice
                {
                    LandlordId = landlordId.Value,
                    RoomId = rental.RoomId,
                    RentalId = rental.RentalId,
                    InvoiceCode = await CreateInvoiceCodeAsync(request.Month, request.Year, rental.RoomId.Value, reservedInvoiceCodes),
                    Month = request.Month,
                    Year = request.Year,
                    IssueDate = DateTime.UtcNow,
                    DueDate = dueDate,
                    RoomPrice = roomPrice,
                    ElectricCost = electricCost,
                    WaterCost = waterCost,
                    ServiceCost = serviceCost,
                    OtherCost = 0,
                    DiscountAmount = 0,
                    TotalAmount = total,
                    PaidAmount = 0,
                    RemainingAmount = total,
                    Status = DateTime.UtcNow.Date > dueDate.Date ? "Overdue" : "Unpaid",
                    SentViaZalo = false,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                invoice.InvoiceItems.Add(new InvoiceItem { ItemType = "Room", ItemName = "Tiền phòng", Quantity = 1, UnitPrice = roomPrice, Amount = roomPrice });
                invoice.InvoiceItems.Add(new InvoiceItem { ItemType = "Electric", ItemName = "Điện", Quantity = 30, UnitPrice = rental.Room?.ElectricPrice ?? 0, Amount = electricCost });
                invoice.InvoiceItems.Add(new InvoiceItem { ItemType = "Water", ItemName = "Nước", Quantity = 4, UnitPrice = rental.Room?.WaterPrice ?? 0, Amount = waterCost });
                invoice.InvoiceItems.Add(new InvoiceItem { ItemType = "Service", ItemName = "Dịch vụ", Quantity = 1, UnitPrice = serviceCost, Amount = serviceCost });
                _context.Invoices.Add(invoice);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var detail = ex.InnerException?.Message ?? ex.Message;
                return BadRequest(ApiResponse<List<InvoiceResponse>>.Fail($"Không thể lưu hóa đơn xuống database: {detail}"));
            }
            await RefreshOverdueStatusAsync(request.BuildingId, landlordId.Value, request.Month, request.Year);

            var invoices = await QueryInvoices(request.BuildingId, landlordId.Value, request.Month, request.Year).ToListAsync();
            return Ok(ApiResponse<List<InvoiceResponse>>.Ok("Đã tạo/lấy hóa đơn tháng thành công.", invoices));
        }

        [HttpPost("{invoiceId:int}/mark-paid")]
        public async Task<ActionResult<ApiResponse<InvoiceResponse>>> MarkPaid(int invoiceId, [FromBody] MarkInvoicePaidRequest request)
        {
            var landlordId = GetLandlordId();
            if (landlordId == null)
                return Unauthorized(ApiResponse<InvoiceResponse>.Fail("Không tìm thấy thông tin chủ trọ."));

            var invoice = await _context.Invoices
                .Include(i => i.Room)
                .ThenInclude(r => r!.Building)
                .Include(i => i.RentalRecord)
                .ThenInclude(r => r!.Tenant)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId && i.LandlordId == landlordId.Value && i.IsDeleted != true);

            if (invoice == null)
                return NotFound(ApiResponse<InvoiceResponse>.Fail("Không tìm thấy hóa đơn."));

            var total = invoice.TotalAmount ?? 0;
            var amount = request.Amount.GetValueOrDefault(total);
            invoice.PaidAmount = amount;
            invoice.RemainingAmount = Math.Max(0, total - amount);
            invoice.Status = invoice.RemainingAmount <= 0 ? "Paid" : "PartiallyPaid";
            invoice.UpdatedAt = DateTime.UtcNow;

            _context.Payments.Add(new Payment
            {
                InvoiceId = invoice.InvoiceId,
                Amount = amount,
                PaymentMethod = request.PaymentMethod,
                PaidAt = DateTime.UtcNow,
                ReceivedBy = GetUserId(),
                Note = request.Note,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<InvoiceResponse>.Ok("Đã cập nhật hóa đơn thành đã thanh toán.", MapInvoice(invoice)));
        }

        [HttpPost("{invoiceId:int}/mark-unpaid")]
        public async Task<ActionResult<ApiResponse<InvoiceResponse>>> MarkUnpaid(int invoiceId)
        {
            var landlordId = GetLandlordId();
            if (landlordId == null)
                return Unauthorized(ApiResponse<InvoiceResponse>.Fail("Không tìm thấy thông tin chủ trọ."));

            var invoice = await _context.Invoices
                .Include(i => i.Room)
                .ThenInclude(r => r!.Building)
                .Include(i => i.RentalRecord)
                .ThenInclude(r => r!.Tenant)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId && i.LandlordId == landlordId.Value && i.IsDeleted != true);

            if (invoice == null)
                return NotFound(ApiResponse<InvoiceResponse>.Fail("Không tìm thấy hóa đơn."));

            var payments = await _context.Payments
                .Where(payment => payment.InvoiceId == invoice.InvoiceId)
                .ToListAsync();

            _context.Payments.RemoveRange(payments);

            var total = invoice.TotalAmount ?? 0;
            invoice.PaidAmount = 0;
            invoice.RemainingAmount = total;
            invoice.Status = invoice.DueDate.HasValue && invoice.DueDate.Value.Date < DateTime.Today
                ? "Overdue"
                : "Unpaid";
            invoice.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<InvoiceResponse>.Ok("Đã đặt lại hóa đơn về trạng thái chưa thanh toán.", MapInvoice(invoice)));
        }

        [HttpDelete("{invoiceId:int}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteInvoice(int invoiceId)
        {
            var landlordId = GetLandlordId();
            if (landlordId == null)
                return Unauthorized(ApiResponse<bool>.Fail("Không tìm thấy thông tin chủ trọ."));

            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId && i.LandlordId == landlordId.Value && i.IsDeleted != true);

            if (invoice == null)
                return NotFound(ApiResponse<bool>.Fail("Không tìm thấy hóa đơn."));

            invoice.IsDeleted = true;
            invoice.DeletedAt = DateTime.UtcNow;
            invoice.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<bool>.Ok("Đã xóa hóa đơn.", true));
        }

        private IQueryable<InvoiceResponse> QueryInvoices(int buildingId, int landlordId, int month, int year)
        {
            return _context.Invoices
                .AsNoTracking()
                .Include(i => i.Room)
                .ThenInclude(r => r!.Building)
                .Include(i => i.RentalRecord)
                .ThenInclude(r => r!.Tenant)
                .Where(i =>
                    i.LandlordId == landlordId &&
                    i.Month == month &&
                    i.Year == year &&
                    i.IsDeleted != true &&
                    i.Room != null &&
                    i.Room.BuildingId == buildingId)
                .OrderBy(i => i.Room!.RoomCode)
                .Select(i => new InvoiceResponse
                {
                    InvoiceId = i.InvoiceId,
                    InvoiceCode = i.InvoiceCode ?? string.Empty,
                    RoomId = i.RoomId,
                    RoomCode = i.Room != null ? i.Room.RoomCode : string.Empty,
                    TenantName = i.RentalRecord != null && i.RentalRecord.Tenant != null ? i.RentalRecord.Tenant.FullName : string.Empty,
                    TenantPhone = i.RentalRecord != null && i.RentalRecord.Tenant != null ? (i.RentalRecord.Tenant.PhoneNumber ?? string.Empty) : string.Empty,
                    Month = i.Month ?? 0,
                    Year = i.Year ?? 0,
                    IssueDate = i.IssueDate,
                    DueDate = i.DueDate,
                    RoomPrice = i.RoomPrice ?? 0,
                    ElectricCost = i.ElectricCost ?? 0,
                    WaterCost = i.WaterCost ?? 0,
                    ServiceCost = i.ServiceCost ?? 0,
                    OtherCost = i.OtherCost ?? 0,
                    DiscountAmount = i.DiscountAmount ?? 0,
                    TotalAmount = i.TotalAmount ?? 0,
                    PaidAmount = i.PaidAmount ?? 0,
                    RemainingAmount = i.RemainingAmount ?? 0,
                    Status = i.Status ?? "Unpaid"
                });
        }

        private InvoiceResponse MapInvoice(Invoice invoice)
        {
            return new InvoiceResponse
            {
                InvoiceId = invoice.InvoiceId,
                InvoiceCode = invoice.InvoiceCode ?? string.Empty,
                RoomId = invoice.RoomId,
                RoomCode = invoice.Room?.RoomCode ?? string.Empty,
                TenantName = invoice.RentalRecord?.Tenant?.FullName ?? string.Empty,
                TenantPhone = invoice.RentalRecord?.Tenant?.PhoneNumber ?? string.Empty,
                Month = invoice.Month ?? 0,
                Year = invoice.Year ?? 0,
                IssueDate = invoice.IssueDate,
                DueDate = invoice.DueDate,
                RoomPrice = invoice.RoomPrice ?? 0,
                ElectricCost = invoice.ElectricCost ?? 0,
                WaterCost = invoice.WaterCost ?? 0,
                ServiceCost = invoice.ServiceCost ?? 0,
                OtherCost = invoice.OtherCost ?? 0,
                DiscountAmount = invoice.DiscountAmount ?? 0,
                TotalAmount = invoice.TotalAmount ?? 0,
                PaidAmount = invoice.PaidAmount ?? 0,
                RemainingAmount = invoice.RemainingAmount ?? 0,
                Status = invoice.Status ?? "Unpaid"
            };
        }

        private async Task RefreshOverdueStatusAsync(int buildingId, int landlordId, int month, int year)
        {
            var invoices = await _context.Invoices
                .Include(i => i.Room)
                .Where(i =>
                    i.LandlordId == landlordId &&
                    i.Month == month &&
                    i.Year == year &&
                    i.IsDeleted != true &&
                    i.Status != "Paid" &&
                    i.Room != null &&
                    i.Room.BuildingId == buildingId)
                .ToListAsync();

            foreach (var invoice in invoices)
            {
                invoice.Status = invoice.DueDate?.Date < DateTime.UtcNow.Date ? "Overdue" : "Unpaid";
                invoice.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        private static DateTime ResolveDueDate(RentalRecord rental, int month, int year)
        {
            var dueDay = Math.Clamp(rental.PaymentDueDay ?? 5, 1, 28);
            var dueDate = new DateTime(year, month, dueDay, 0, 0, 0, DateTimeKind.Utc);
            var startDate = rental.StartDate?.ToDateTime(TimeOnly.MinValue);
            if (startDate.HasValue && startDate.Value.Year == year && startDate.Value.Month == month && startDate.Value.Date > dueDate.Date)
            {
                return DateTime.SpecifyKind(startDate.Value.Date.AddDays(7), DateTimeKind.Utc);
            }

            return dueDate;
        }

        private async Task<string> CreateInvoiceCodeAsync(int month, int year, int roomId, HashSet<string>? reservedCodes = null)
        {
            var baseCode = $"INV-{year % 100:00}{month:00}-{roomId:000}";
            var code = baseCode;
            var suffix = 1;
            while ((reservedCodes?.Contains(code) ?? false) || await _context.Invoices.AnyAsync(i => i.InvoiceCode == code))
            {
                code = $"{baseCode}-{suffix++}";
            }

            reservedCodes?.Add(code);
            return code;
        }

        private async Task<bool> OwnsBuildingAsync(int buildingId, int landlordId)
        {
            return await _context.Buildings.AnyAsync(b => b.BuildingId == buildingId && b.LandlordId == landlordId && b.IsDeleted != true);
        }

        private int? GetLandlordId()
        {
            var landlordIdClaim = User.FindFirst("LandlordId")?.Value;
            return int.TryParse(landlordIdClaim, out var landlordId) ? landlordId : null;
        }

        private int? GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
