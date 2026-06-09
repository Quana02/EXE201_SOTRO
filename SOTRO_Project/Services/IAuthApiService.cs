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
        Task<ApiResponse<AuthResponse>> GoogleLoginAsync(GoogleLoginRequest request);
        Task<ApiResponse<AuthResponse>> CompleteProfileAsync(CompleteProfileRequest request);
        Task<ApiResponse<AuthResponse>> UpdateProfileAsync(UpdateProfileRequest request, Stream? avatarStream, string? avatarFileName);
        Task<ApiResponse<string>> ChangePasswordAsync(ChangePasswordRequest request);
        Task<ApiResponse<AuthResponse>> LinkGoogleAsync(LinkGoogleRequest request);
    }
}
