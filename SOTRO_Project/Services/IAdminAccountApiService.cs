using SOTRO_Project.Models.Admin;
using SOTRO_Project.Models.Auth;

namespace SOTRO_Project.Services
{
    public interface IAdminAccountApiService
    {
        Task<ApiResponse<List<AdminAccountResponse>>> GetAccountsAsync();
        Task<ApiResponse<List<AdminSubscriptionPlanResponse>>> GetPlansAsync();
        Task<ApiResponse<bool>> UpdateAccountStatusAsync(int userId, string status);
        Task<ApiResponse<bool>> UpdateAccountSubscriptionAsync(int userId, int? planId);
    }
}
