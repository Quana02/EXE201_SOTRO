using SoTro_BE.Models;

namespace SoTro_BE.Repositories
{
    public interface IRoomRepository
    {
        Task<Room?> GetRoomByIdAsync(int roomId);
        Task<Room?> GetRoomByIdWithOwnershipAsync(int roomId, int landlordId);
        Task<List<Room>> GetRoomsByBuildingAsync(int buildingId);
        Task<List<Room>> GetRoomsByStatusAsync(int buildingId, string status);
        Task<bool> RoomCodeExistsAsync(int buildingId, string roomCode, int? excludeRoomId = null);
        Task<Room> CreateRoomAsync(Room room);
        Task<Room> UpdateRoomAsync(Room room);
        Task DeleteRoomAsync(int roomId);
    }
}
