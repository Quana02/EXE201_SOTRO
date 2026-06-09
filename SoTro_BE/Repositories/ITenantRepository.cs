using SoTro_BE.Models;

namespace SoTro_BE.Repositories
{
    public interface ITenantRepository
    {
        Task<List<Tenant>> GetTenantsAsync(int landlordId);
        Task<Tenant?> GetTenantByIdAsync(int tenantId, int landlordId);
        Task<Room?> GetOwnedRoomAsync(int roomId, int landlordId);
        Task<Tenant> CreateTenantAsync(Tenant tenant, RentalRecord rentalRecord, RoomOccupant occupant, Room room);
        Task<Tenant> UpdateTenantAsync(Tenant tenant, RentalRecord rentalRecord, RoomOccupant occupant, Room currentRoom, Room? previousRoom);
        Task DeleteTenantAsync(Tenant tenant);
    }
}
