using SoTro_BE.DTOs.Auth;
using SoTro_BE.DTOs.Tenant;

namespace SoTro_BE.Services
{
    public interface ITenantService
    {
        Task<ApiResponse<List<TenantResponse>>> GetTenantsAsync(int landlordId, int? buildingId = null);
        Task<ApiResponse<TenantResponse>> GetTenantByIdAsync(int tenantId, int landlordId);
        Task<ApiResponse<TenantStatsResponse>> GetTenantStatsAsync(int landlordId, int? buildingId = null);
        Task<ApiResponse<TenantResponse>> CreateTenantAsync(int landlordId, CreateTenantRequest request);
        Task<ApiResponse<TenantResponse>> UpdateTenantAsync(int tenantId, int landlordId, UpdateTenantRequest request);
        Task<ApiResponse<bool>> DeleteTenantAsync(int tenantId, int landlordId);
    }
}
