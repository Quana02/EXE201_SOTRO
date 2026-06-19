using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
using SOTRO_Project.Models.Auth;
using SOTRO_Project.Models.Invoice;

namespace SOTRO_Project.Services
{
    public class InvoiceApiService : IInvoiceApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        public InvoiceApiService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public async Task<ApiResponse<List<InvoiceResponse>>> GetInvoicesAsync(int buildingId, int month, int year)
        {
            await SetAuthorizationHeaderAsync();
            return await GetAsync<List<InvoiceResponse>>($"api/invoices?buildingId={buildingId}&month={month}&year={year}");
        }

        public async Task<ApiResponse<List<InvoiceResponse>>> GenerateMonthlyInvoicesAsync(GenerateMonthlyInvoicesRequest request)
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync("api/invoices/generate-monthly", request);
            return await ReadResponseAsync<List<InvoiceResponse>>(response);
        }

        public async Task<ApiResponse<InvoiceResponse>> MarkPaidAsync(int invoiceId, MarkInvoicePaidRequest request)
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync($"api/invoices/{invoiceId}/mark-paid", request);
            return await ReadResponseAsync<InvoiceResponse>(response);
        }

        public async Task<ApiResponse<InvoiceResponse>> MarkUnpaidAsync(int invoiceId)
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.PostAsync($"api/invoices/{invoiceId}/mark-unpaid", null);
            return await ReadResponseAsync<InvoiceResponse>(response);
        }

        public async Task<ApiResponse<bool>> DeleteInvoiceAsync(int invoiceId)
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.DeleteAsync($"api/invoices/{invoiceId}");
            return await ReadResponseAsync<bool>(response);
        }

        private async Task<ApiResponse<T>> GetAsync<T>(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                return await ReadResponseAsync<T>(response);
            }
            catch (Exception ex)
            {
                return Fail<T>($"Lỗi kết nối API hóa đơn: {ex.Message}");
            }
        }

        private static async Task<ApiResponse<T>> ReadResponseAsync<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
            {
                return Fail<T>($"API hóa đơn trả về mã {(int)response.StatusCode} ({response.ReasonPhrase}) nhưng không có dữ liệu.");
            }

            try
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return apiResponse ?? Fail<T>($"API hóa đơn trả về mã {(int)response.StatusCode} ({response.ReasonPhrase}) nhưng không có dữ liệu hợp lệ.");
            }
            catch
            {
                var preview = content.Length > 160 ? content[..160] : content;
                return Fail<T>($"API hóa đơn chưa trả về JSON hợp lệ. Mã {(int)response.StatusCode} ({response.ReasonPhrase}). Nội dung: {preview}");
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
