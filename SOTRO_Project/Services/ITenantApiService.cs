using SOTRO_Project.Models.Auth;
using SOTRO_Project.Models.Tenant;

namespace SOTRO_Project.Services
{
    public interface ITenantApiService
    {
        Task<ApiResponse<List<TenantResponse>>> GetTenantsAsync();
        Task<ApiResponse<TenantStatsResponse>> GetTenantStatsAsync();
        Task<ApiResponse<TenantResponse>> GetTenantByIdAsync(int tenantId);
        Task<ApiResponse<TenantResponse>> CreateTenantAsync(CreateTenantRequest request);
        Task<ApiResponse<TenantResponse>> UpdateTenantAsync(int tenantId, UpdateTenantRequest request);
        Task<ApiResponse<bool>> DeleteTenantAsync(int tenantId);
    }
}
