using SOTRO_Project.Models.Auth;
using SOTRO_Project.Models.Dashboard;

namespace SOTRO_Project.Services
{
    public interface IDashboardApiService
    {
        Task<ApiResponse<DashboardSummaryResponse>> GetSummaryAsync(int? buildingId = null, int? month = null, int? year = null);
    }
}
