using SOTRO_Project.Models.Auth;

namespace SOTRO_Project.Services
{
    public interface IAuthApiService
    {
        Task<ApiResponse<string>> SendRegisterOtpAsync(RegisterSendOtpRequest request);
        Task<ApiResponse<string>> ResendRegisterOtpAsync(RegisterResendOtpRequest request);
        Task<ApiResponse<string>> VerifyRegisterOtpAsync(RegisterVerifyOtpRequest request);
        Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request);
        Task<ApiResponse<string>> SendForgotPasswordOtpAsync(ForgotPasswordSendOtpRequest request);
        Task<ApiResponse<string>> VerifyForgotPasswordOtpAsync(ForgotPasswordVerifyOtpRequest request);
        Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequest request);
    }
}
