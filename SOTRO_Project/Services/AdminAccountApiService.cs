using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
using SOTRO_Project.Models.Admin;
using SOTRO_Project.Models.Auth;

namespace SOTRO_Project.Services
{
    public class AdminAccountApiService : IAdminAccountApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        public AdminAccountApiService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public async Task<ApiResponse<List<AdminAccountResponse>>> GetAccountsAsync()
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.GetAsync("api/admin/accounts");
            return await ReadResponseAsync<List<AdminAccountResponse>>(response);
        }

        public async Task<ApiResponse<List<AdminSubscriptionPlanResponse>>> GetPlansAsync()
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.GetAsync("api/admin/accounts/plans");
            return await ReadResponseAsync<List<AdminSubscriptionPlanResponse>>(response);
        }

        public async Task<ApiResponse<bool>> UpdateAccountStatusAsync(int userId, string status)
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.PutAsJsonAsync($"api/admin/accounts/{userId}/status", new UpdateAccountStatusRequest { Status = status });
            return await ReadResponseAsync<bool>(response);
        }

        public async Task<ApiResponse<bool>> UpdateAccountSubscriptionAsync(int userId, int? planId)
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.PutAsJsonAsync($"api/admin/accounts/{userId}/subscription", new UpdateAccountSubscriptionRequest { PlanId = planId });
            return await ReadResponseAsync<bool>(response);
        }

        private static async Task<ApiResponse<T>> ReadResponseAsync<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
            {
                return Fail<T>($"API admin trả về mã {(int)response.StatusCode} ({response.ReasonPhrase}) nhưng không có dữ liệu.");
            }

            try
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return apiResponse ?? Fail<T>($"API admin trả về dữ liệu không hợp lệ.");
            }
            catch
            {
                var preview = content.Length > 180 ? content[..180] : content;
                return Fail<T>($"API admin chưa trả về JSON hợp lệ. Mã {(int)response.StatusCode} ({response.ReasonPhrase}). Nội dung: {preview}");
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

        private static ApiResponse<T> Fail<T>(string message) => new() { Success = false, Message = message };
    }
}
