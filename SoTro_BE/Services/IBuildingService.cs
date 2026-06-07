using SoTro_BE.DTOs.Auth;
using SoTro_BE.DTOs.Building;

namespace SoTro_BE.Services
{
    public interface IBuildingService
    {
        Task<ApiResponse<BuildingResponse>> CreateBuildingAsync(int landlordId, CreateBuildingRequest request, List<(Stream Stream, string FileName)> imageFiles);
        Task<ApiResponse<List<BuildingResponse>>> GetBuildingsByLandlordAsync(int landlordId);
        Task<ApiResponse<BuildingResponse>> GetBuildingByIdAsync(int buildingId, int landlordId);
        Task<ApiResponse<BuildingResponse>> UpdateBuildingAsync(int buildingId, int landlordId, CreateBuildingRequest request, List<(Stream Stream, string FileName)>? imageFiles);
        Task<ApiResponse<bool>> DeleteBuildingAsync(int buildingId, int landlordId);
        Task<ApiResponse<List<BuildingTypeResponse>>> GetBuildingTypesAsync();
        Task<ApiResponse<List<string>>> SuggestAddressAsync(string query);
    }
}
