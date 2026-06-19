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

        public async Task<ApiResponse<DashboardSummaryResponse>> GetSummaryAsync(int? buildingId = null, int? month = null, int? year = null)
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                var query = new List<string>();
                if (buildingId.HasValue && buildingId.Value > 0)
                    query.Add($"buildingId={buildingId.Value}");
                if (month.HasValue && year.HasValue)
                {
                    query.Add($"month={month.Value}");
                    query.Add($"year={year.Value}");
                }

                var url = query.Count > 0
                    ? $"api/dashboard/summary?{string.Join("&", query)}"
                    : "api/dashboard/summary";
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<DashboardSummaryResponse>>(url);
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
