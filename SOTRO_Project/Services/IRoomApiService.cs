using SOTRO_Project.Models.Auth;
using SOTRO_Project.Models.Room;

namespace SOTRO_Project.Services
{
    public interface IRoomApiService
    {
        Task<ApiResponse<List<RoomResponse>>> GetRoomsByBuildingAsync(int buildingId);
        Task<ApiResponse<RoomDetailResponse>> GetRoomByIdAsync(int roomId);
        Task<ApiResponse<RoomStatsModel>> GetRoomStatsAsync(int buildingId);
        Task<ApiResponse<RoomResponse>> CreateRoomAsync(int buildingId, CreateRoomRequest request);
        Task<ApiResponse<RoomResponse>> UpdateRoomAsync(int roomId, UpdateRoomRequest request);
        Task<ApiResponse<bool>> DeleteRoomAsync(int roomId);
        Task<ApiResponse<RoomResponse>> ChangeRoomStatusAsync(int roomId, ChangeRoomStatusRequest request);
    }
}
