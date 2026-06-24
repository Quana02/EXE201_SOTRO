using SoTro_BE.Data;
using SoTro_BE.DTOs.Auth;
using SoTro_BE.DTOs.Room;
using SoTro_BE.Models;
using SoTro_BE.Repositories;
using System.Text.Json;

namespace SoTro_BE.Services
{
    public interface IRoomService
    {
        Task<ApiResponse<List<RoomResponse>>> GetRoomsByBuildingAsync(int buildingId, int landlordId);
        Task<ApiResponse<RoomDetailResponse>> GetRoomByIdAsync(int roomId, int landlordId);
        Task<ApiResponse<RoomStatsResponse>> GetRoomStatsAsync(int buildingId, int landlordId);
        Task<ApiResponse<RoomResponse>> CreateRoomAsync(int buildingId, int landlordId, CreateRoomRequest request);
        Task<ApiResponse<RoomResponse>> UpdateRoomAsync(int roomId, int landlordId, UpdateRoomRequest request);
        Task<ApiResponse<bool>> DeleteRoomAsync(int roomId, int landlordId);
        Task<ApiResponse<RoomResponse>> ChangeRoomStatusAsync(int roomId, int landlordId, int userId, ChangeRoomStatusRequest request);
    }

    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _roomRepository;
        private readonly SoTroDbContext _context;

        public RoomService(IRoomRepository roomRepository, SoTroDbContext context)
        {
            _roomRepository = roomRepository;
            _context = context;
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET LIST
        // ─────────────────────────────────────────────────────────────────────
        public async Task<ApiResponse<List<RoomResponse>>> GetRoomsByBuildingAsync(int buildingId, int landlordId)
        {
            try
            {
                // Kiểm tra building thuộc landlord này
                var building = await _context.Buildings.FindAsync(buildingId);
                if (building == null || building.LandlordId != landlordId || building.IsDeleted == true)
                    return Fail<List<RoomResponse>>("Không tìm thấy nhà trọ.");

                var rooms = await _roomRepository.GetRoomsByBuildingAsync(buildingId);
                var responses = rooms.Select(MapToResponse).ToList();

                return new ApiResponse<List<RoomResponse>> { Success = true, Data = responses };
            }
            catch (Exception ex)
            {
                return Fail<List<RoomResponse>>($"Lỗi khi lấy danh sách phòng: {ex.Message}");
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET DETAIL
        // ─────────────────────────────────────────────────────────────────────
        public async Task<ApiResponse<RoomDetailResponse>> GetRoomByIdAsync(int roomId, int landlordId)
        {
            try
            {
                var room = await _roomRepository.GetRoomByIdWithOwnershipAsync(roomId, landlordId);
                if (room == null)
                    return Fail<RoomDetailResponse>("Không tìm thấy phòng.");

                return new ApiResponse<RoomDetailResponse> { Success = true, Data = MapToDetailResponse(room) };
            }
            catch (Exception ex)
            {
                return Fail<RoomDetailResponse>($"Lỗi: {ex.Message}");
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET STATS
        // ─────────────────────────────────────────────────────────────────────
        public async Task<ApiResponse<RoomStatsResponse>> GetRoomStatsAsync(int buildingId, int landlordId)
        {
            try
            {
                var building = await _context.Buildings.FindAsync(buildingId);
                if (building == null || building.LandlordId != landlordId || building.IsDeleted == true)
                    return Fail<RoomStatsResponse>("Không tìm thấy nhà trọ.");

                var rooms = await _roomRepository.GetRoomsByBuildingAsync(buildingId);
                var total = rooms.Count;
                var available = rooms.Count(r => r.Status == "Available");
                var occupied = rooms.Count(r => r.Status == "Occupied");
                var maintenance = rooms.Count(r => r.Status == "Maintenance");
                var rate = total > 0 ? Math.Round((decimal)occupied / total * 100, 1) : 0;

                return new ApiResponse<RoomStatsResponse>
                {
                    Success = true,
                    Data = new RoomStatsResponse
                    {
                        TotalRooms = total,
                        AvailableRooms = available,
                        OccupiedRooms = occupied,
                        MaintenanceRooms = maintenance,
                        OccupancyRate = rate
                    }
                };
            }
            catch (Exception ex)
            {
                return Fail<RoomStatsResponse>($"Lỗi: {ex.Message}");
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // CREATE
        // ─────────────────────────────────────────────────────────────────────
        public async Task<ApiResponse<RoomResponse>> CreateRoomAsync(int buildingId, int landlordId, CreateRoomRequest request)
        {
            try
            {
                var building = await _context.Buildings.FindAsync(buildingId);
                if (building == null || building.LandlordId != landlordId || building.IsDeleted == true)
                    return Fail<RoomResponse>("Không tìm thấy nhà trọ.");

                // Kiểm tra trùng mã phòng
                if (await _roomRepository.RoomCodeExistsAsync(buildingId, request.RoomCode))
                    return Fail<RoomResponse>($"Mã phòng '{request.RoomCode}' đã tồn tại trong nhà trọ này.");

                // Validate số người thuê khi phòng đang được thuê
                if (request.Status == "Occupied")
                {
                    if (!request.Capacity.HasValue || request.Capacity <= 0)
                        return Fail<RoomResponse>("Khi phòng đang thuê, vui lòng nhập sức chứa tối đa của phòng.");

                    if (!request.CurrentTenantCount.HasValue || request.CurrentTenantCount <= 0)
                        return Fail<RoomResponse>("Khi phòng đang thuê, bắt buộc phải nhập số người đang thuê (lớn hơn 0).");

                    if (request.CurrentTenantCount > request.Capacity)
                        return Fail<RoomResponse>($"Số người đang thuê ({request.CurrentTenantCount}) không được vượt quá sức chứa ({request.Capacity} người).");
                }
                else if (request.Status == "Maintenance")
                {
                    if (request.CurrentTenantCount.HasValue && request.CurrentTenantCount < 0)
                        return Fail<RoomResponse>("Số người đang ở không được nhỏ hơn 0.");
                }

                // Lưu extra fields (RoomName, Zone, ServiceFee, IncidentFee, BankInfo, BillingDay, PaymentDueDay)
                // vào field Note dạng JSON để không cần migration
                var extraData = new RoomExtraData
                {
                    RoomName = request.RoomName,
                    Zone = request.Zone,
                    ServiceFee = request.ServiceFee,
                    IncidentFee = request.IncidentFee,
                    BillingDay = request.BillingDay,
                    PaymentDueDay = request.PaymentDueDay,
                    BankName = request.BankName,
                    BankAccountNumber = request.BankAccountNumber,
                    BankQrUrl = request.BankQrUrl,
                    UserNote = request.Note
                };

                var room = new Room
                {
                    BuildingId = buildingId,
                    RoomTypeId = request.RoomTypeId,
                    RoomCode = request.RoomCode,
                    FloorNumber = request.FloorNumber,
                    BasePrice = request.BasePrice,
                    ElectricPrice = request.ElectricPrice,
                    WaterPrice = request.WaterPrice,
                    Capacity = request.Capacity,
                    CurrentTenantCount = request.Status == "Available" ? 0 : (request.CurrentTenantCount ?? 0),
                    Status = request.Status,
                    Note = JsonSerializer.Serialize(extraData),
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var created = await _roomRepository.CreateRoomAsync(room);

                // Cập nhật TotalRooms của building
                building.TotalRooms = (building.TotalRooms ?? 0) + 1;
                building.UpdatedAt = DateTime.UtcNow;
                _context.Buildings.Update(building);
                await _context.SaveChangesAsync();

                return new ApiResponse<RoomResponse>
                {
                    Success = true,
                    Message = "Tạo phòng thành công!",
                    Data = MapToResponse(created)
                };
            }
            catch (Exception ex)
            {
                var details = ex.InnerException != null ? $"{ex.Message} (Inner: {ex.InnerException.Message})" : ex.Message;
                return Fail<RoomResponse>($"Lỗi khi tạo phòng: {details}");
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // UPDATE
        // ─────────────────────────────────────────────────────────────────────
        public async Task<ApiResponse<RoomResponse>> UpdateRoomAsync(int roomId, int landlordId, UpdateRoomRequest request)
        {
            try
            {
                var room = await _roomRepository.GetRoomByIdWithOwnershipAsync(roomId, landlordId);
                if (room == null)
                    return Fail<RoomResponse>("Không tìm thấy phòng.");

                // Kiểm tra trùng mã phòng (exclude chính nó)
                if (room.RoomCode != request.RoomCode)
                {
                    if (await _roomRepository.RoomCodeExistsAsync(room.BuildingId!.Value, request.RoomCode, roomId))
                        return Fail<RoomResponse>($"Mã phòng '{request.RoomCode}' đã tồn tại trong nhà trọ này.");
                }

                // Validate số người thuê khi phòng đang được thuê
                if (request.Status == "Occupied")
                {
                    if (!request.Capacity.HasValue || request.Capacity <= 0)
                        return Fail<RoomResponse>("Khi phòng đang thuê, vui lòng nhập sức chứa tối đa của phòng.");

                    if (!request.CurrentTenantCount.HasValue || request.CurrentTenantCount <= 0)
                        return Fail<RoomResponse>("Khi phòng đang thuê, bắt buộc phải nhập số người đang thuê (lớn hơn 0).");

                    if (request.CurrentTenantCount > request.Capacity)
                        return Fail<RoomResponse>($"Số người đang thuê ({request.CurrentTenantCount}) không được vượt quá sức chứa ({request.Capacity} người).");
                }
                else if (request.Status == "Maintenance")
                {
                    if (request.CurrentTenantCount.HasValue && request.CurrentTenantCount < 0)
                        return Fail<RoomResponse>("Số người đang ở không được nhỏ hơn 0.");
                }

                var extraData = new RoomExtraData
                {
                    RoomName = request.RoomName,
                    Zone = request.Zone,
                    ServiceFee = request.ServiceFee,
                    IncidentFee = request.IncidentFee,
                    BillingDay = request.BillingDay,
                    PaymentDueDay = request.PaymentDueDay,
                    BankName = request.BankName,
                    BankAccountNumber = request.BankAccountNumber,
                    BankQrUrl = request.BankQrUrl,
                    UserNote = request.Note
                };

                room.RoomCode = request.RoomCode;
                room.FloorNumber = request.FloorNumber;
                room.BasePrice = request.BasePrice;
                room.ElectricPrice = request.ElectricPrice;
                room.WaterPrice = request.WaterPrice;
                room.Capacity = request.Capacity;
                room.RoomTypeId = request.RoomTypeId;
                room.Note = JsonSerializer.Serialize(extraData);
                room.UpdatedAt = DateTime.UtcNow;

                // Đổi trạng thái nếu có
                if (!string.IsNullOrEmpty(request.Status) && room.Status != request.Status)
                {
                    room.Status = request.Status;
                }

                // Cập nhật số người đang thuê
                if (request.Status == "Available")
                    room.CurrentTenantCount = 0;
                else if (request.CurrentTenantCount.HasValue)
                    room.CurrentTenantCount = request.CurrentTenantCount.Value;

                var updated = await _roomRepository.UpdateRoomAsync(room);
                return new ApiResponse<RoomResponse>
                {
                    Success = true,
                    Message = "Cập nhật phòng thành công!",
                    Data = MapToResponse(updated)
                };
            }
            catch (Exception ex)
            {
                return Fail<RoomResponse>($"Lỗi khi cập nhật phòng: {ex.Message}");
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // DELETE (soft delete)
        // ─────────────────────────────────────────────────────────────────────
        public async Task<ApiResponse<bool>> DeleteRoomAsync(int roomId, int landlordId)
        {
            try
            {
                var room = await _roomRepository.GetRoomByIdWithOwnershipAsync(roomId, landlordId);
                if (room == null)
                    return Fail<bool>("Không tìm thấy phòng.");

                // Không xóa nếu đang có người thuê
                if (room.Status == "Occupied")
                    return Fail<bool>("Không thể xóa phòng đang có người thuê.");

                await _roomRepository.DeleteRoomAsync(roomId);

                // Cập nhật TotalRooms của building
                var building = await _context.Buildings.FindAsync(room.BuildingId);
                if (building != null)
                {
                    building.TotalRooms = Math.Max(0, (building.TotalRooms ?? 1) - 1);
                    building.UpdatedAt = DateTime.UtcNow;
                    _context.Buildings.Update(building);
                    await _context.SaveChangesAsync();
                }

                return new ApiResponse<bool> { Success = true, Message = "Xóa phòng thành công!", Data = true };
            }
            catch (Exception ex)
            {
                return Fail<bool>($"Lỗi khi xóa phòng: {ex.Message}");
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // CHANGE STATUS
        // ─────────────────────────────────────────────────────────────────────
        public async Task<ApiResponse<RoomResponse>> ChangeRoomStatusAsync(int roomId, int landlordId, int userId, ChangeRoomStatusRequest request)
        {
            try
            {
                var room = await _roomRepository.GetRoomByIdWithOwnershipAsync(roomId, landlordId);
                if (room == null)
                    return Fail<RoomResponse>("Không tìm thấy phòng.");

                var validStatuses = new[] { "Available", "Occupied", "Maintenance" };
                if (!validStatuses.Contains(request.NewStatus))
                    return Fail<RoomResponse>("Trạng thái không hợp lệ. Phải là: Available, Occupied, Maintenance.");

                var oldStatus = room.Status;
                room.Status = request.NewStatus;
                room.UpdatedAt = DateTime.UtcNow;

                // Ghi lịch sử
                var history = new RoomStatusHistory
                {
                    RoomId = roomId,
                    OldStatus = oldStatus,
                    NewStatus = request.NewStatus,
                    Reason = request.Reason,
                    ChangedBy = userId,
                    ChangedAt = DateTime.UtcNow
                };
                _context.RoomStatusHistories.Add(history);
                _context.Rooms.Update(room);
                await _context.SaveChangesAsync();

                var updated = await _roomRepository.GetRoomByIdAsync(roomId);
                return new ApiResponse<RoomResponse>
                {
                    Success = true,
                    Message = $"Đổi trạng thái phòng thành '{request.NewStatus}' thành công!",
                    Data = MapToResponse(updated!)
                };
            }
            catch (Exception ex)
            {
                return Fail<RoomResponse>($"Lỗi khi đổi trạng thái: {ex.Message}");
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ─────────────────────────────────────────────────────────────────────
        private static RoomExtraData ParseExtraData(string? note)
        {
            if (string.IsNullOrWhiteSpace(note)) return new RoomExtraData();
            try { return JsonSerializer.Deserialize<RoomExtraData>(note) ?? new RoomExtraData(); }
            catch { return new RoomExtraData { UserNote = note }; }
        }

        private static RoomResponse MapToResponse(Room room)
        {
            var extra = ParseExtraData(room.Note);
            var activeRental = room.RentalRecords?.FirstOrDefault(rr => rr.Status == "Active" && rr.IsDeleted != true);
            var latestInvoice = room.Invoices?.OrderByDescending(i => i.Year).ThenByDescending(i => i.Month).FirstOrDefault();

            TenantSummaryDto? primaryTenant = null;
            if (activeRental?.Tenant != null)
            {
                primaryTenant = new TenantSummaryDto
                {
                    TenantId = activeRental.Tenant.TenantId,
                    FullName = activeRental.Tenant.FullName,
                    PhoneNumber = activeRental.Tenant.PhoneNumber,
                    Email = activeRental.Tenant.Email,
                    StartDate = activeRental.StartDate
                };
            }

            var occupants = room.RoomOccupants?
                .Where(o => o.Status == "Living" && o.Tenant != null)
                .Select(o => new OccupantDto
                {
                    OccupantId = o.OccupantId,
                    TenantId = o.TenantId,
                    FullName = o.Tenant?.FullName,
                    PhoneNumber = o.Tenant?.PhoneNumber,
                    IsPrimary = o.IsPrimaryTenant,
                    MoveInDate = o.MoveInDate
                }).ToList() ?? new List<OccupantDto>();
            var currentTenantCount = occupants.Count;

            LatestInvoiceDto? latestInvoiceDto = null;
            if (latestInvoice != null)
            {
                latestInvoiceDto = new LatestInvoiceDto
                {
                    InvoiceId = latestInvoice.InvoiceId,
                    InvoiceCode = latestInvoice.InvoiceCode,
                    Month = latestInvoice.Month ?? 0,
                    Year = latestInvoice.Year ?? 0,
                    TotalAmount = latestInvoice.TotalAmount,
                    RemainingAmount = latestInvoice.RemainingAmount,
                    Status = latestInvoice.Status,
                    CreatedAt = latestInvoice.CreatedAt
                };
            }

            return new RoomResponse
            {
                RoomId = room.RoomId,
                BuildingId = room.BuildingId,
                BuildingName = room.Building?.BuildingName,
                RoomTypeId = room.RoomTypeId,
                RoomTypeName = room.RoomType?.TypeName,
                RoomCode = room.RoomCode,
                RoomName = extra.RoomName,
                Zone = extra.Zone,
                FloorNumber = room.FloorNumber,
                Area = room.Area,
                Status = room.Status,
                BasePrice = room.BasePrice,
                ElectricPrice = room.ElectricPrice,
                WaterPrice = room.WaterPrice,
                ServiceFee = extra.ServiceFee,
                IncidentFee = extra.IncidentFee,
                Capacity = room.Capacity,
                CurrentTenantCount = currentTenantCount,
                BillingDay = extra.BillingDay,
                PaymentDueDay = extra.PaymentDueDay,
                PaymentStatus = latestInvoice?.Status,
                BankName = extra.BankName,
                BankAccountNumber = extra.BankAccountNumber,
                BankQrUrl = extra.BankQrUrl,
                Note = extra.UserNote,
                PrimaryTenant = primaryTenant,
                Occupants = occupants,
                LatestInvoice = latestInvoiceDto,
                CreatedAt = room.CreatedAt,
                UpdatedAt = room.UpdatedAt
            };
        }

        private static RoomDetailResponse MapToDetailResponse(Room room)
        {
            var baseResponse = MapToResponse(room);
            var detail = new RoomDetailResponse
            {
                RoomId = baseResponse.RoomId,
                BuildingId = baseResponse.BuildingId,
                BuildingName = baseResponse.BuildingName,
                RoomTypeId = baseResponse.RoomTypeId,
                RoomTypeName = baseResponse.RoomTypeName,
                RoomCode = baseResponse.RoomCode,
                RoomName = baseResponse.RoomName,
                Zone = baseResponse.Zone,
                FloorNumber = baseResponse.FloorNumber,
                Area = baseResponse.Area,
                Status = baseResponse.Status,
                BasePrice = baseResponse.BasePrice,
                ElectricPrice = baseResponse.ElectricPrice,
                WaterPrice = baseResponse.WaterPrice,
                ServiceFee = baseResponse.ServiceFee,
                IncidentFee = baseResponse.IncidentFee,
                Capacity = baseResponse.Capacity,
                CurrentTenantCount = baseResponse.CurrentTenantCount,
                BillingDay = baseResponse.BillingDay,
                PaymentDueDay = baseResponse.PaymentDueDay,
                PaymentStatus = baseResponse.PaymentStatus,
                BankName = baseResponse.BankName,
                BankAccountNumber = baseResponse.BankAccountNumber,
                BankQrUrl = baseResponse.BankQrUrl,
                Note = baseResponse.Note,
                PrimaryTenant = baseResponse.PrimaryTenant,
                Occupants = baseResponse.Occupants,
                LatestInvoice = baseResponse.LatestInvoice,
                CreatedAt = baseResponse.CreatedAt,
                UpdatedAt = baseResponse.UpdatedAt,

                StatusHistory = room.RoomStatusHistories?
                    .OrderByDescending(h => h.ChangedAt)
                    .Select(h => new RoomStatusHistoryDto
                    {
                        StatusHistoryId = h.StatusHistoryId,
                        OldStatus = h.OldStatus,
                        NewStatus = h.NewStatus,
                        Reason = h.Reason,
                        ChangedAt = h.ChangedAt
                    }).ToList() ?? new(),

                MaintenanceReports = room.MaintenanceReports?
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => new MaintenanceReportDto
                    {
                        ReportId = m.ReportId,
                        Title = m.Title,
                        Description = m.Description,
                        Priority = m.Priority,
                        Status = m.Status,
                        CreatedAt = m.CreatedAt,
                        ResolvedAt = m.ResolvedAt
                    }).ToList() ?? new()
            };

            return detail;
        }

        private static ApiResponse<T> Fail<T>(string message) =>
            new ApiResponse<T> { Success = false, Message = message };
    }

    /// <summary>
    /// Extra data lưu trong Note field của Room (JSON) để tránh migration
    /// </summary>
    public class RoomExtraData
    {
        public string? RoomName { get; set; }
        public string? Zone { get; set; }
        public decimal? ServiceFee { get; set; }
        public decimal? IncidentFee { get; set; }
        public int? BillingDay { get; set; }
        public int? PaymentDueDay { get; set; }
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankQrUrl { get; set; }
        public string? UserNote { get; set; }
    }
}
