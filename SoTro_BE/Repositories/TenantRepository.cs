using Microsoft.EntityFrameworkCore;
using SoTro_BE.Data;
using SoTro_BE.Models;

namespace SoTro_BE.Repositories
{
    public class TenantRepository : ITenantRepository
    {
        private readonly SoTroDbContext _context;

        public TenantRepository(SoTroDbContext context)
        {
            _context = context;
        }

        public async Task<List<Tenant>> GetTenantsAsync(int landlordId)
        {
            return await _context.Tenants
                .Include(t => t.RentalRecords.Where(r => r.IsDeleted != true))
                    .ThenInclude(r => r.Room)
                .Where(t => t.LandlordId == landlordId && t.IsDeleted != true)
                .OrderByDescending(t => t.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Tenant?> GetTenantByIdAsync(int tenantId, int landlordId)
        {
            return await _context.Tenants
                .Include(t => t.RentalRecords.Where(r => r.IsDeleted != true))
                    .ThenInclude(r => r.Room)
                .Include(t => t.RoomOccupants)
                .FirstOrDefaultAsync(t =>
                    t.TenantId == tenantId &&
                    t.LandlordId == landlordId &&
                    t.IsDeleted != true);
        }

        public async Task<Room?> GetOwnedRoomAsync(int roomId, int landlordId)
        {
            return await _context.Rooms
                .Include(r => r.Building)
                .FirstOrDefaultAsync(r =>
                    r.RoomId == roomId &&
                    r.IsDeleted != true &&
                    r.Building != null &&
                    r.Building.LandlordId == landlordId &&
                    r.Building.IsDeleted != true);
        }

        public async Task<Tenant> CreateTenantAsync(Tenant tenant, RentalRecord rentalRecord, RoomOccupant occupant, Room room)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            rentalRecord.TenantId = tenant.TenantId;
            _context.RentalRecords.Add(rentalRecord);
            await _context.SaveChangesAsync();

            occupant.TenantId = tenant.TenantId;
            occupant.RentalId = rentalRecord.RentalId;
            _context.RoomOccupants.Add(occupant);

            room.Status = "Occupied";
            room.CurrentTenantCount = (room.CurrentTenantCount ?? 0) + 1;
            room.UpdatedAt = DateTime.UtcNow;
            _context.Rooms.Update(room);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (await GetTenantByIdAsync(tenant.TenantId, tenant.LandlordId!.Value))!;
        }

        public async Task<Tenant> UpdateTenantAsync(Tenant tenant, RentalRecord rentalRecord, RoomOccupant occupant, Room currentRoom, Room? previousRoom)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            _context.Tenants.Update(tenant);
            _context.RentalRecords.Update(rentalRecord);
            _context.RoomOccupants.Update(occupant);

            if (previousRoom != null && previousRoom.RoomId != currentRoom.RoomId)
            {
                previousRoom.CurrentTenantCount = Math.Max(0, (previousRoom.CurrentTenantCount ?? 1) - 1);
                previousRoom.Status = previousRoom.CurrentTenantCount > 0 ? "Occupied" : "Available";
                previousRoom.UpdatedAt = DateTime.UtcNow;
                _context.Rooms.Update(previousRoom);

                currentRoom.CurrentTenantCount = (currentRoom.CurrentTenantCount ?? 0) + 1;
                currentRoom.Status = "Occupied";
                currentRoom.UpdatedAt = DateTime.UtcNow;
                _context.Rooms.Update(currentRoom);
            }
            else if (occupant.Status == "MovedOut")
            {
                currentRoom.CurrentTenantCount = Math.Max(0, (currentRoom.CurrentTenantCount ?? 1) - 1);
                currentRoom.Status = currentRoom.CurrentTenantCount > 0 ? "Occupied" : "Available";
                currentRoom.UpdatedAt = DateTime.UtcNow;
                _context.Rooms.Update(currentRoom);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (await GetTenantByIdAsync(tenant.TenantId, tenant.LandlordId!.Value))!;
        }

        public async Task DeleteTenantAsync(Tenant tenant)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            tenant.IsDeleted = true;
            tenant.DeletedAt = DateTime.UtcNow;
            tenant.UpdatedAt = DateTime.UtcNow;

            foreach (var rental in tenant.RentalRecords.Where(r => r.IsDeleted != true))
            {
                rental.Status = "Ended";
                rental.EndDate ??= DateOnly.FromDateTime(DateTime.UtcNow);
                rental.IsDeleted = true;
                rental.DeletedAt = DateTime.UtcNow;
                rental.UpdatedAt = DateTime.UtcNow;
            }

            foreach (var occupant in tenant.RoomOccupants.Where(o => o.Status == "Living"))
            {
                occupant.Status = "MovedOut";
                occupant.MoveOutDate = DateOnly.FromDateTime(DateTime.UtcNow);
                occupant.UpdatedAt = DateTime.UtcNow;
            }

            var activeRoomIds = tenant.RoomOccupants
                .Where(o => o.RoomId.HasValue)
                .Select(o => o.RoomId!.Value)
                .Distinct()
                .ToList();

            foreach (var roomId in activeRoomIds)
            {
                var room = await _context.Rooms.FindAsync(roomId);
                if (room == null)
                    continue;

                room.CurrentTenantCount = Math.Max(0, (room.CurrentTenantCount ?? 1) - 1);
                room.Status = room.CurrentTenantCount > 0 ? "Occupied" : "Available";
                room.UpdatedAt = DateTime.UtcNow;
                _context.Rooms.Update(room);
            }

            _context.Tenants.Update(tenant);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
    }
}
