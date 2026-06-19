using System.Net.Http.Json;
using SOTRO_Project.Models.Auth;
using SOTRO_Project.Models.Contact;

namespace SOTRO_Project.Services
{
    public class ContactApiService : IContactApiService
    {
        private readonly HttpClient _httpClient;

        public ContactApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResponse<bool>> SendConsultationAsync(ConsultationRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/contact/consultation", request);
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();

                return result ?? new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Không đọc được phản hồi từ máy chủ."
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Lỗi gửi yêu cầu tư vấn: {ex.Message}"
                };
            }
        }
    }
}
