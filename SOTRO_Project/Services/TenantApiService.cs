using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.JSInterop;
using SOTRO_Project.Models.Auth;
using SOTRO_Project.Models.Tenant;

namespace SOTRO_Project.Services
{
    public class TenantApiService : ITenantApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        public TenantApiService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public async Task<ApiResponse<List<TenantResponse>>> GetTenantsAsync()
        {
            await SetAuthorizationHeaderAsync();
            return await GetAsync<List<TenantResponse>>("api/tenants");
        }

        public async Task<ApiResponse<TenantStatsResponse>> GetTenantStatsAsync()
        {
            await SetAuthorizationHeaderAsync();
            return await GetAsync<TenantStatsResponse>("api/tenants/stats");
        }

        public async Task<ApiResponse<TenantResponse>> GetTenantByIdAsync(int tenantId)
        {
            await SetAuthorizationHeaderAsync();
            return await GetAsync<TenantResponse>($"api/tenants/{tenantId}");
        }

        public async Task<ApiResponse<TenantResponse>> CreateTenantAsync(CreateTenantRequest request)
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync("api/tenants", request);
            return await ReadResponseAsync<TenantResponse>(response);
        }

        public async Task<ApiResponse<TenantResponse>> UpdateTenantAsync(int tenantId, UpdateTenantRequest request)
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.PutAsJsonAsync($"api/tenants/{tenantId}", request);
            return await ReadResponseAsync<TenantResponse>(response);
        }

        public async Task<ApiResponse<bool>> DeleteTenantAsync(int tenantId)
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.DeleteAsync($"api/tenants/{tenantId}");
            return await ReadResponseAsync<bool>(response);
        }

        private async Task<ApiResponse<T>> GetAsync<T>(string url)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<T>>(url);
                return response ?? Fail<T>("Không nhận được phản hồi từ máy chủ.");
            }
            catch (Exception ex)
            {
                return Fail<T>($"Lỗi kết nối: {ex.Message}");
            }
        }

        private async Task<ApiResponse<T>> ReadResponseAsync<T>(HttpResponseMessage response)
        {
            try
            {
                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
                if (apiResponse != null)
                    return apiResponse;

                return Fail<T>($"API trả về mã {response.StatusCode} nhưng không có dữ liệu.");
            }
            catch (Exception ex)
            {
                return Fail<T>($"Lỗi đọc phản hồi API: {ex.Message}");
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
