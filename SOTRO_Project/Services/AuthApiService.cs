using System.Net.Http.Json;
using SOTRO_Project.Models.Auth;
using Microsoft.JSInterop;
using System.IO;
using System.Net.Http.Headers;

namespace SOTRO_Project.Services
{
    public class AuthApiService : IAuthApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        public AuthApiService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
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

        public Task<ApiResponse<AuthResponse>> GoogleLoginAsync(GoogleLoginRequest request)
        {
            return PostAsync<AuthResponse>("api/auth/google-login", request);
        }

        public Task<ApiResponse<AuthResponse>> CompleteProfileAsync(CompleteProfileRequest request)
        {
            return PostAsync<AuthResponse>("api/auth/complete-profile", request);
        }

        private async Task SetAuthorizationHeaderAsync()
        {
            try
            {
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "sotro_token");
                if (!string.IsNullOrWhiteSpace(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                else
                {
                    _httpClient.DefaultRequestHeaders.Authorization = null;
                }
            }
            catch (Exception)
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<ApiResponse<AuthResponse>> UpdateProfileAsync(UpdateProfileRequest request, Stream? avatarStream, string? avatarFileName)
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                using var content = new MultipartFormDataContent();
                content.Add(new StringContent(request.Email), nameof(request.Email));
                content.Add(new StringContent(request.FullName), nameof(request.FullName));
                content.Add(new StringContent(request.PhoneNumber), nameof(request.PhoneNumber));

                if (avatarStream != null && !string.IsNullOrWhiteSpace(avatarFileName))
                {
                    var streamContent = new StreamContent(avatarStream);
                    var ext = Path.GetExtension(avatarFileName).ToLowerInvariant();
                    var mime = ext switch
                    {
                        ".jpg" or ".jpeg" => "image/jpeg",
                        ".png" => "image/png",
                        ".webp" => "image/webp",
                        ".gif" => "image/gif",
                        _ => "application/octet-stream"
                    };
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue(mime);
                    content.Add(streamContent, "avatar", avatarFileName);
                }

                var response = await _httpClient.PostAsync("api/auth/update-profile", content);
                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
                return apiResponse ?? new ApiResponse<AuthResponse> { Success = false, Message = "Không đọc được phản hồi từ API." };
            }
            catch (Exception ex)
            {
                return new ApiResponse<AuthResponse> { Success = false, Message = $"Lỗi cập nhật hồ sơ: {ex.Message}" };
            }
        }

        public async Task<ApiResponse<string>> ChangePasswordAsync(ChangePasswordRequest request)
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/change-password", request);
                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
                return apiResponse ?? new ApiResponse<string> { Success = false, Message = "Không đọc được phản hồi từ API." };
            }
            catch (Exception ex)
            {
                return new ApiResponse<string> { Success = false, Message = $"Lỗi đổi mật khẩu: {ex.Message}" };
            }
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
                    Message = "Không đọc được phản hồi từ API."
                };
            }
            catch (HttpRequestException ex)
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = $"Không kết nối được với SoTro_BE: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = $"Lỗi xử lý yêu cầu: {ex.Message}"
                };
            }
        }
    }
}
