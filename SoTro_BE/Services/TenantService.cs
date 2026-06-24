using SoTro_BE.DTOs.Auth;
using SoTro_BE.DTOs.Tenant;
using SoTro_BE.Models;
using SoTro_BE.Repositories;

namespace SoTro_BE.Services
{
    public class TenantService : ITenantService
    {
        private readonly ITenantRepository _tenantRepository;

        public TenantService(ITenantRepository tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }

        public async Task<ApiResponse<List<TenantResponse>>> GetTenantsAsync(int landlordId, int? buildingId = null)
        {
            if (buildingId is null or <= 0)
            {
                return ApiResponse<List<TenantResponse>>.Fail("Vui lòng chọn nhà trọ trước khi xem danh sách người thuê.");
            }

            var tenants = await _tenantRepository.GetTenantsAsync(landlordId, buildingId);
            return ApiResponse<List<TenantResponse>>.Ok("Lấy danh sách người thuê thành công.", tenants.Select(MapToResponse).ToList());
        }

        public async Task<ApiResponse<TenantResponse>> GetTenantByIdAsync(int tenantId, int landlordId)
        {
            var tenant = await _tenantRepository.GetTenantByIdAsync(tenantId, landlordId);
            return tenant == null
                ? ApiResponse<TenantResponse>.Fail("Không tìm thấy người thuê.")
                : ApiResponse<TenantResponse>.Ok("Lấy thông tin người thuê thành công.", MapToResponse(tenant));
        }

        public async Task<ApiResponse<TenantStatsResponse>> GetTenantStatsAsync(int landlordId, int? buildingId = null)
        {
            if (buildingId is null or <= 0)
            {
                return ApiResponse<TenantStatsResponse>.Fail("Vui lòng chọn nhà trọ trước khi xem thống kê người thuê.");
            }

            var tenants = await _tenantRepository.GetTenantsAsync(landlordId, buildingId);
            var activeRentals = tenants
                .SelectMany(t => t.RentalRecords)
                .Where(r => r.Status == "Active" && r.IsDeleted != true)
                .ToList();
            var activeTenantCount = activeRentals
                .Select(r => r.TenantId)
                .Where(id => id.HasValue)
                .Distinct()
                .Count();

            var now = DateOnly.FromDateTime(DateTime.UtcNow);
            var next30Days = now.AddDays(30);
            var totalRooms = activeRentals.Select(r => r.RoomId).Where(id => id.HasValue).Distinct().Count();

            var stats = new TenantStatsResponse
            {
                TotalTenants = tenants.Count,
                ActiveTenants = activeTenantCount,
                TotalDepositAmount = activeRentals.Sum(r => r.DepositAmount ?? 0),
                ExpiringContracts = activeRentals.Count(r => r.EndDate.HasValue && r.EndDate.Value >= now && r.EndDate.Value <= next30Days),
                NewThisMonth = tenants.Count(t => t.CreatedAt.HasValue && t.CreatedAt.Value.Year == DateTime.UtcNow.Year && t.CreatedAt.Value.Month == DateTime.UtcNow.Month),
                OccupancyRate = totalRooms > 0 ? Math.Round((decimal)activeTenantCount / totalRooms * 100, 1) : 0
            };

            return ApiResponse<TenantStatsResponse>.Ok("Lấy thống kê người thuê thành công.", stats);
        }

        public async Task<ApiResponse<TenantResponse>> CreateTenantAsync(int landlordId, CreateTenantRequest request)
        {
            var room = await _tenantRepository.GetOwnedRoomAsync(request.RoomId, landlordId);
            if (room == null)
                return ApiResponse<TenantResponse>.Fail("Không tìm thấy phòng.");

            var roomTenantCount = GetLivingTenantCount(room);
            if (room.Capacity.HasValue && room.Capacity.Value > 0 && roomTenantCount >= room.Capacity.Value)
                return ApiResponse<TenantResponse>.Fail("Phòng đã đủ số người ở.");

            var now = DateTime.UtcNow;
            var tenant = new Tenant
            {
                LandlordId = landlordId,
                FullName = request.FullName.Trim(),
                PhoneNumber = request.PhoneNumber.Trim(),
                Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
                IdentityNumber = request.IdentityNumber.Trim(),
                DateOfBirth = request.DateOfBirth,
                PermanentAddress = request.PermanentAddress,
                Status = "Active",
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            };

            var rental = new RentalRecord
            {
                RoomId = request.RoomId,
                StartDate = request.StartDate,
                DepositAmount = request.DepositAmount ?? 0,
                MonthlyRent = request.MonthlyRent ?? room.BasePrice,
                NumberOfPeople = 1,
                Status = "Active",
                IsDeleted = false,
                CreatedAt = now,
                UpdatedAt = now
            };

            var occupant = new RoomOccupant
            {
                RoomId = request.RoomId,
                IsPrimaryTenant = true,
                MoveInDate = request.StartDate,
                Status = "Living",
                CreatedAt = now,
                UpdatedAt = now
            };

            var created = await _tenantRepository.CreateTenantAsync(tenant, rental, occupant, room);
            return ApiResponse<TenantResponse>.Ok("Thêm người thuê thành công.", MapToResponse(created));
        }

        public async Task<ApiResponse<TenantResponse>> UpdateTenantAsync(int tenantId, int landlordId, UpdateTenantRequest request)
        {
            var tenant = await _tenantRepository.GetTenantByIdAsync(tenantId, landlordId);
            if (tenant == null)
                return ApiResponse<TenantResponse>.Fail("Không tìm thấy người thuê.");

            var currentRoom = await _tenantRepository.GetOwnedRoomAsync(request.RoomId, landlordId);
            if (currentRoom == null)
                return ApiResponse<TenantResponse>.Fail("Không tìm thấy phòng.");

            var activeRental = tenant.RentalRecords.FirstOrDefault(r => r.Status == "Active" && r.IsDeleted != true)
                ?? tenant.RentalRecords.OrderByDescending(r => r.CreatedAt).FirstOrDefault();
            if (activeRental == null)
                return ApiResponse<TenantResponse>.Fail("Không tìm thấy hợp đồng thuê của người thuê.");

            var occupant = tenant.RoomOccupants.FirstOrDefault(o => o.Status == "Living")
                ?? tenant.RoomOccupants.OrderByDescending(o => o.CreatedAt).FirstOrDefault();
            if (occupant == null)
                return ApiResponse<TenantResponse>.Fail("Không tìm thấy thông tin lưu trú của người thuê.");

            Room? previousRoom = null;
            var isMovingToAnotherRoom = activeRental.RoomId.HasValue && activeRental.RoomId.Value != request.RoomId;
            if (isMovingToAnotherRoom)
                previousRoom = await _tenantRepository.GetOwnedRoomAsync(activeRental.RoomId.GetValueOrDefault(), landlordId);

            if (request.Status == "Active" && isMovingToAnotherRoom && currentRoom.Capacity.HasValue && currentRoom.Capacity.Value > 0)
            {
                var currentTenantCount = GetLivingTenantCount(currentRoom);
                if (currentTenantCount >= currentRoom.Capacity.Value)
                    return ApiResponse<TenantResponse>.Fail("Phòng đã đủ số người ở, không thể chuyển thêm người thuê vào phòng này.");
            }

            tenant.FullName = request.FullName.Trim();
            tenant.PhoneNumber = request.PhoneNumber.Trim();
            tenant.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
            tenant.IdentityNumber = request.IdentityNumber.Trim();
            tenant.DateOfBirth = request.DateOfBirth;
            tenant.PermanentAddress = request.PermanentAddress;
            tenant.Status = request.Status;
            tenant.UpdatedAt = DateTime.UtcNow;

            activeRental.RoomId = request.RoomId;
            activeRental.StartDate = request.StartDate;
            activeRental.EndDate = request.EndDate;
            activeRental.DepositAmount = request.DepositAmount ?? 0;
            activeRental.MonthlyRent = request.MonthlyRent ?? currentRoom.BasePrice;
            activeRental.Status = request.Status == "Active" ? "Active" : "Ended";
            activeRental.UpdatedAt = DateTime.UtcNow;

            occupant.RoomId = request.RoomId;
            occupant.MoveInDate = request.StartDate;
            occupant.MoveOutDate = request.Status == "Active" ? null : request.EndDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
            occupant.Status = request.Status == "Active" ? "Living" : "MovedOut";
            occupant.UpdatedAt = DateTime.UtcNow;

            var updated = await _tenantRepository.UpdateTenantAsync(tenant, activeRental, occupant, currentRoom, previousRoom);
            return ApiResponse<TenantResponse>.Ok("Cập nhật người thuê thành công.", MapToResponse(updated));
        }

        public async Task<ApiResponse<bool>> DeleteTenantAsync(int tenantId, int landlordId)
        {
            var tenant = await _tenantRepository.GetTenantByIdAsync(tenantId, landlordId);
            if (tenant == null)
                return ApiResponse<bool>.Fail("Không tìm thấy người thuê.");

            await _tenantRepository.DeleteTenantAsync(tenant);
            return ApiResponse<bool>.Ok("Xóa người thuê thành công.", true);
        }

        private static int GetLivingTenantCount(Room room)
        {
            var occupantCount = room.RoomOccupants?.Count(o => o.Status == "Living") ?? 0;
            return occupantCount;
        }

        private static TenantResponse MapToResponse(Tenant tenant)
        {
            var rental = tenant.RentalRecords
                .Where(r => r.IsDeleted != true)
                .OrderByDescending(r => r.Status == "Active")
                .ThenByDescending(r => r.CreatedAt)
                .FirstOrDefault();

            return new TenantResponse
            {
                TenantId = tenant.TenantId,
                TenantCode = $"T{tenant.TenantId:D5}",
                FullName = tenant.FullName,
                PhoneNumber = tenant.PhoneNumber,
                Email = tenant.Email,
                IdentityNumber = tenant.IdentityNumber,
                DateOfBirth = tenant.DateOfBirth,
                PermanentAddress = tenant.PermanentAddress,
                Status = tenant.Status,
                RoomId = rental?.RoomId,
                RoomCode = rental?.Room?.RoomCode,
                StartDate = rental?.StartDate,
                EndDate = rental?.EndDate,
                DepositAmount = rental?.DepositAmount,
                MonthlyRent = rental?.MonthlyRent,
                CreatedAt = tenant.CreatedAt,
                UpdatedAt = tenant.UpdatedAt
            };
        }
    }
}
