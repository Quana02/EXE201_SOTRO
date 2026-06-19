using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.JSInterop;
using SOTRO_Project.Models.Auth;
using SOTRO_Project.Models.Subscription;

namespace SOTRO_Project.Services
{
    public class SubscriptionApiService : ISubscriptionApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        public SubscriptionApiService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public async Task<ApiResponse<SubscriptionStatusResponse>> GetCurrentSubscriptionAsync()
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                var response = await _httpClient.GetAsync("api/subscriptions/current");
                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                {
                    return Fail($"API gói dịch vụ trả về mã {(int)response.StatusCode} ({response.ReasonPhrase}) nhưng không có dữ liệu.");
                }

                var apiResponse = JsonSerializer.Deserialize<ApiResponse<SubscriptionStatusResponse>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return apiResponse ?? Fail("Không đọc được trạng thái gói dịch vụ.");
            }
            catch (Exception ex)
            {
                return Fail($"Lỗi kết nối API gói dịch vụ: {ex.Message}");
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

        private static ApiResponse<SubscriptionStatusResponse> Fail(string message) => new()
        {
            Success = false,
            Message = message,
            Data = new SubscriptionStatusResponse()
        };
    }
}
