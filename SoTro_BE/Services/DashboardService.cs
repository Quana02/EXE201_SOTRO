using Microsoft.EntityFrameworkCore;
using SoTro_BE.Data;
using SoTro_BE.DTOs.Auth;
using SoTro_BE.DTOs.Dashboard;

namespace SoTro_BE.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly SoTroDbContext _context;

        public DashboardService(SoTroDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<DashboardSummaryResponse>> GetSummaryAsync(int landlordId, int? buildingId, int month, int year)
        {
            var selectedDate = new DateTime(year, month, 1);
            var previousMonth = selectedDate.AddMonths(-1);
            var chartStart = selectedDate.AddMonths(-5);

            if (buildingId is null or <= 0)
            {
                return ApiResponse<DashboardSummaryResponse>.Fail("Vui long chon nha tro truoc khi xem dashboard.");
            }

            if (buildingId.HasValue)
            {
                var ownsBuilding = await _context.Buildings.AnyAsync(b =>
                    b.BuildingId == buildingId.Value &&
                    b.LandlordId == landlordId &&
                    b.IsDeleted != true);

                if (!ownsBuilding)
                {
                    return ApiResponse<DashboardSummaryResponse>.Fail("Không tìm thấy nhà trọ hoặc bạn không có quyền truy cập.");
                }
            }

            var roomQuery = _context.Rooms
                .Include(r => r.Building)
                .Where(r => r.IsDeleted != true && r.Building != null && r.Building.LandlordId == landlordId && r.Building.IsDeleted != true);

            if (buildingId.HasValue)
            {
                roomQuery = roomQuery.Where(r => r.BuildingId == buildingId.Value);
            }

            var rooms = await roomQuery
                .AsNoTracking()
                .ToListAsync();

            var invoiceQuery = _context.Invoices
                .Include(i => i.Room)
                .Where(i => i.IsDeleted != true && i.LandlordId == landlordId)
                .AsNoTracking();

            if (buildingId.HasValue)
            {
                invoiceQuery = invoiceQuery.Where(i => i.Room != null && i.Room.BuildingId == buildingId.Value);
            }

            var selectedInvoices = await invoiceQuery
                .Where(i => i.Month == month && i.Year == year)
                .ToListAsync();

            var previousInvoices = await invoiceQuery
                .Where(i => i.Month == previousMonth.Month && i.Year == previousMonth.Year)
                .ToListAsync();

            var chartInvoices = await invoiceQuery
                .Where(i =>
                    (i.Year > chartStart.Year || (i.Year == chartStart.Year && i.Month >= chartStart.Month)) &&
                    (i.Year < year || (i.Year == year && i.Month <= month)))
                .ToListAsync();

            var monthlyRevenue = selectedInvoices.Sum(GetCollectedAmount);
            var previousRevenue = previousInvoices.Sum(GetCollectedAmount);

            var revenueGrowth = previousRevenue > 0
                ? Math.Round((monthlyRevenue - previousRevenue) / previousRevenue * 100, 1)
                : monthlyRevenue > 0 ? 100 : 0;

            var lastSixMonths = Enumerable.Range(0, 6)
                .Select(offset => selectedDate.AddMonths(-5 + offset))
                .Select(date => new DashboardRevenuePoint
                {
                    Month = date.Month,
                    Year = date.Year,
                    Revenue = chartInvoices
                        .Where(i => i.Month == date.Month && i.Year == date.Year)
                        .Sum(GetCollectedAmount)
                })
                .ToList();

            var totalRooms = rooms.Count;
            var occupiedRooms = rooms.Count(r => r.Status == "Occupied");

            var recentInvoices = await invoiceQuery
                .OrderByDescending(i => i.CreatedAt ?? i.IssueDate ?? DateTime.MinValue)
                .Take(5)
                .ToListAsync();

            var recentActivities = recentInvoices
                .Select(i => new DashboardActivityItem
                {
                    TimeLabel = FormatTimeLabel(i.CreatedAt ?? i.IssueDate),
                    Content = $"Hóa đơn tháng {i.Month}/{i.Year}",
                    RoomCode = i.Room?.RoomCode,
                    Status = i.Status,
                    Amount = i.TotalAmount
                })
                .ToList();

            var pendingInvoices = await invoiceQuery.CountAsync(i =>
                i.Month == month &&
                i.Year == year &&
                (i.Status == "Unpaid" || i.Status == "PartiallyPaid" || i.Status == "Overdue" || (i.RemainingAmount ?? 0) > 0));

            var summary = new DashboardSummaryResponse
            {
                MonthlyRevenue = monthlyRevenue,
                RevenueGrowthPercent = revenueGrowth,
                TotalRooms = totalRooms,
                OccupiedRooms = occupiedRooms,
                OccupancyRate = totalRooms > 0 ? Math.Round((decimal)occupiedRooms / totalRooms * 100, 1) : 0,
                PendingInvoices = pendingInvoices,
                RevenueLastSixMonths = lastSixMonths,
                RecentActivities = recentActivities
            };

            return ApiResponse<DashboardSummaryResponse>.Ok("Lấy tổng quan dashboard thành công.", summary);
        }

        private static decimal GetCollectedAmount(Models.Invoice invoice)
        {
            if (invoice.Status == "Paid")
            {
                return invoice.PaidAmount.GetValueOrDefault(invoice.TotalAmount.GetValueOrDefault());
            }

            if (invoice.Status == "PartiallyPaid")
            {
                return invoice.PaidAmount.GetValueOrDefault();
            }

            return 0;
        }

        private static string FormatTimeLabel(DateTime? value)
        {
            if (!value.HasValue)
                return string.Empty;

            var local = value.Value.ToLocalTime();
            var today = DateTime.Now.Date;
            if (local.Date == today)
                return local.ToString("HH:mm");
            if (local.Date == today.AddDays(-1))
                return "Hôm qua";
            return local.ToString("dd/MM/yyyy");
        }
    }
}
