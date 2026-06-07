using Microsoft.EntityFrameworkCore;
using SoTro_BE.Data;
using SoTro_BE.Models;

namespace SoTro_BE.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly SoTroDbContext _context;

        public RoomRepository(SoTroDbContext context)
        {
            _context = context;
        }

        public async Task<Room?> GetRoomByIdAsync(int roomId)
        {
            return await _context.Rooms
                .Include(r => r.RoomType)
                .Include(r => r.Building)
                .Include(r => r.RoomOccupants.Where(o => o.Status == "Living"))
                    .ThenInclude(o => o.Tenant)
                .Include(r => r.RentalRecords.Where(rr => rr.Status == "Active" && rr.IsDeleted != true))
                    .ThenInclude(rr => rr.Tenant)
                .Include(r => r.RentalRecords)
                    .ThenInclude(rr => rr.TenantMembers)
                .Include(r => r.Invoices.OrderByDescending(i => i.Year).ThenByDescending(i => i.Month).Take(1))
                    .ThenInclude(i => i.Payments)
                .Include(r => r.RoomStatusHistories.OrderByDescending(h => h.ChangedAt).Take(10))
                .Include(r => r.MaintenanceReports.Where(m => m.Status != "Cancelled").OrderByDescending(m => m.CreatedAt).Take(10))
                .FirstOrDefaultAsync(r => r.RoomId == roomId && r.IsDeleted != true);
        }

        public async Task<Room?> GetRoomByIdWithOwnershipAsync(int roomId, int landlordId)
        {
            return await _context.Rooms
                .Include(r => r.RoomType)
                .Include(r => r.Building)
                .Include(r => r.RoomOccupants.Where(o => o.Status == "Living"))
                    .ThenInclude(o => o.Tenant)
                .Include(r => r.RentalRecords.Where(rr => rr.Status == "Active" && rr.IsDeleted != true))
                    .ThenInclude(rr => rr.Tenant)
                .Include(r => r.RentalRecords.Where(rr => rr.Status == "Active" && rr.IsDeleted != true))
                    .ThenInclude(rr => rr.TenantMembers)
                .Include(r => r.Invoices.OrderByDescending(i => i.Year).ThenByDescending(i => i.Month).Take(1))
                    .ThenInclude(i => i.Payments)
                .Include(r => r.RoomStatusHistories.OrderByDescending(h => h.ChangedAt).Take(20))
                .Include(r => r.MaintenanceReports.OrderByDescending(m => m.CreatedAt).Take(10))
                .FirstOrDefaultAsync(r =>
                    r.RoomId == roomId &&
                    r.IsDeleted != true &&
                    r.Building != null &&
                    r.Building.LandlordId == landlordId &&
                    r.Building.IsDeleted != true);
        }

        public async Task<List<Room>> GetRoomsByBuildingAsync(int buildingId)
        {
            return await _context.Rooms
                .Include(r => r.RoomType)
                .Include(r => r.RoomOccupants.Where(o => o.Status == "Living"))
                    .ThenInclude(o => o.Tenant)
                .Include(r => r.RentalRecords.Where(rr => rr.Status == "Active" && rr.IsDeleted != true))
                    .ThenInclude(rr => rr.Tenant)
                .Include(r => r.Invoices.OrderByDescending(i => i.Year).ThenByDescending(i => i.Month).Take(1))
                .Where(r => r.BuildingId == buildingId && r.IsDeleted != true)
                .OrderBy(r => r.FloorNumber)
                .ThenBy(r => r.RoomCode)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Room>> GetRoomsByStatusAsync(int buildingId, string status)
        {
            return await _context.Rooms
                .Include(r => r.RoomType)
                .Include(r => r.RentalRecords.Where(rr => rr.Status == "Active" && rr.IsDeleted != true))
                    .ThenInclude(rr => rr.Tenant)
                .Where(r => r.BuildingId == buildingId && r.Status == status && r.IsDeleted != true)
                .OrderBy(r => r.FloorNumber)
                .ThenBy(r => r.RoomCode)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> RoomCodeExistsAsync(int buildingId, string roomCode, int? excludeRoomId = null)
        {
            var query = _context.Rooms.Where(r =>
                r.BuildingId == buildingId &&
                r.RoomCode == roomCode &&
                r.IsDeleted != true);

            if (excludeRoomId.HasValue)
                query = query.Where(r => r.RoomId != excludeRoomId.Value);

            return await query.AnyAsync();
        }

        public async Task<Room> CreateRoomAsync(Room room)
        {
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            return (await GetRoomByIdAsync(room.RoomId))!;
        }

        public async Task<Room> UpdateRoomAsync(Room room)
        {
            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();
            return (await GetRoomByIdAsync(room.RoomId))!;
        }

        public async Task DeleteRoomAsync(int roomId)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room != null)
            {
                room.IsDeleted = true;
                room.DeletedAt = DateTime.UtcNow;
                room.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
