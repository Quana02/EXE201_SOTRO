using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SOTRO_Project.Models.Auth;
using SOTRO_Project.Models.Building;

namespace SOTRO_Project.Services
{
    public interface IBuildingApiService
    {
        Task<ApiResponse<List<BuildingResponse>>> GetBuildingsAsync();
        Task<ApiResponse<BuildingResponse>> GetBuildingByIdAsync(int id);
        Task<ApiResponse<BuildingResponse>> CreateBuildingAsync(CreateBuildingRequest request, List<(Stream Stream, string FileName)> imageFiles);
        Task<ApiResponse<BuildingResponse>> UpdateBuildingAsync(int id, CreateBuildingRequest request, List<(Stream Stream, string FileName)>? imageFiles);
        Task<ApiResponse<bool>> DeleteBuildingAsync(int id);
        Task<ApiResponse<List<BuildingTypeResponse>>> GetBuildingTypesAsync();
        Task<ApiResponse<List<string>>> SuggestAddressAsync(string query);
    }
}
