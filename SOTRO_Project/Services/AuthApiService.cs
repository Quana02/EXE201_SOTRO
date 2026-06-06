using System.Net.Http.Json;
using SOTRO_Project.Models.Auth;

namespace SOTRO_Project.Services
{
    public class AuthApiService : IAuthApiService
    {
        private readonly HttpClient _httpClient;

        public AuthApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<ApiResponse<string>> SendRegisterOtpAsync(RegisterSendOtpRequest request)
        {
            return PostAsync<string>("api/auth/register/send-otp", request);
        }

        public Task<ApiResponse<string>> VerifyRegisterOtpAsync(RegisterVerifyOtpRequest request)
        {
            return PostAsync<string>("api/auth/register/verify-otp", request);
        }

        public Task<ApiResponse<string>> ResendRegisterOtpAsync(RegisterResendOtpRequest request)
        {
            return PostAsync<string>("api/auth/register/resend-otp", request);
        }

        public Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
        {
            return PostAsync<AuthResponse>("api/auth/login", request);
        }

        public Task<ApiResponse<string>> SendForgotPasswordOtpAsync(ForgotPasswordSendOtpRequest request)
        {
            return PostAsync<string>("api/auth/forgot-password/send-otp", request);
        }

        public Task<ApiResponse<string>> VerifyForgotPasswordOtpAsync(ForgotPasswordVerifyOtpRequest request)
        {
            return PostAsync<string>("api/auth/forgot-password/verify-otp", request);
        }

        public Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequest request)
        {
            return PostAsync<string>("api/auth/forgot-password/reset-password", request);
        }

        private async Task<ApiResponse<T>> PostAsync<T>(string url, object request)
        {
            try
            {
                using var response = await _httpClient.PostAsJsonAsync(url, request);
                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<T>>();

                if (apiResponse != null)
                {
                    return apiResponse;
                }

                return new ApiResponse<T>
                {
                    Success = false,
                    Message = "Khong doc duoc phan hoi tu API."
                };
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = $"Khong ket noi duoc SoTro_BE: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = $"Loi xu ly yeu cau: {ex.Message}"
                };
            }
        }
    }
}
