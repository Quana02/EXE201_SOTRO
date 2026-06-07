using Microsoft.AspNetCore.Mvc;
using SoTro_BE.DTOs.Auth;
using SoTro_BE.Services;

namespace SoTro_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register/send-otp")]
        public async Task<IActionResult> SendRegisterOtp(RegisterSendOtpRequest request)
        {
            var response = await _authService.SendRegisterOtpAsync(request);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPost("register/verify-otp")]
        public async Task<IActionResult> VerifyRegisterOtp(RegisterVerifyOtpRequest request)
        {
            var response = await _authService.VerifyRegisterOtpAsync(request);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPost("register/resend-otp")]
        public async Task<IActionResult> ResendRegisterOtp(RegisterResendOtpRequest request)
        {
            var response = await _authService.ResendRegisterOtpAsync(request);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var response = await _authService.LoginAsync(request);
            return response.Success ? Ok(response) : Unauthorized(response);
        }

        [HttpPost("forgot-password/send-otp")]
        public async Task<IActionResult> SendForgotPasswordOtp(ForgotPasswordSendOtpRequest request)
        {
            var response = await _authService.SendForgotPasswordOtpAsync(request);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPost("forgot-password/verify-otp")]
        public async Task<IActionResult> VerifyForgotPasswordOtp(ForgotPasswordVerifyOtpRequest request)
        {
            var response = await _authService.VerifyForgotPasswordOtpAsync(request);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPost("forgot-password/reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            var response = await _authService.ResetPasswordAsync(request);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin(GoogleLoginRequest request)
        {
            var response = await _authService.GoogleLoginAsync(request);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPost("complete-profile")]
        public async Task<IActionResult> CompleteProfile(CompleteProfileRequest request)
        {
            var response = await _authService.CompleteProfileAsync(request);
            return response.Success ? Ok(response) : BadRequest(response);
        }
    }
}
