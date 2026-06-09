using SoTro_BE.DTOs.Auth;
using SoTro_BE.DTOs.Dashboard;

namespace SoTro_BE.Services
{
    public interface IDashboardService
    {
        Task<ApiResponse<DashboardSummaryResponse>> GetSummaryAsync(int landlordId);
    }
}
