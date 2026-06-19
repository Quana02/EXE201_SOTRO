using SOTRO_Project.Models.Auth;
using SOTRO_Project.Models.Subscription;

namespace SOTRO_Project.Services
{
    public interface ISubscriptionApiService
    {
        Task<ApiResponse<SubscriptionStatusResponse>> GetCurrentSubscriptionAsync();
    }
}
