using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.JSInterop;
using SOTRO_Project.Models.Auth;
using SOTRO_Project.Models.Dashboard;

namespace SOTRO_Project.Services
{
    public class DashboardApiService : IDashboardApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        public DashboardApiService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public async Task<ApiResponse<DashboardSummaryResponse>> GetSummaryAsync()
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<DashboardSummaryResponse>>("api/dashboard/summary");
                return response ?? new ApiResponse<DashboardSummaryResponse> { Success = false, Message = "Không nhận được phản hồi từ máy chủ." };
            }
            catch (Exception ex)
            {
                return new ApiResponse<DashboardSummaryResponse> { Success = false, Message = $"Lỗi kết nối: {ex.Message}" };
            }
        }

        private async Task SetAuthorizationHeaderAsync()
        {
            try
            {
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "sotro_token");
                _httpClient.DefaultRequestHeaders.Authorization = !string.IsNullOrWhiteSpace(token)
                    ? new AuthenticationHeaderValue("Bearer", token)
                    : null;
            }
            catch
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }
    }
}
