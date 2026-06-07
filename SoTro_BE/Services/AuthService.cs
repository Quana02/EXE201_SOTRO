using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SoTro_BE.DTOs.Auth;
using SoTro_BE.Models;
using SoTro_BE.Repositories;

namespace SoTro_BE.Services
{
    public class AuthService : IAuthService
    {
        private const string RegisterPurpose = "Register";
        private const string ForgotPasswordPurpose = "ForgotPassword";

        private readonly IAuthRepository _authRepository;
        private readonly IOtpRepository _otpRepository;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<User> _passwordHasher;

        public AuthService(
            IAuthRepository authRepository,
            IOtpRepository otpRepository,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _authRepository = authRepository;
            _otpRepository = otpRepository;
            _emailService = emailService;
            _configuration = configuration;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<ApiResponse<string>> SendRegisterOtpAsync(RegisterSendOtpRequest request)
        {
            try
            {
                var email = NormalizeEmail(request.Email);

                var fullName = request.FullName?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(fullName))
                {
                    return ApiResponse<string>.Fail("Họ tên không được để trống và chỉ được chứa chữ cái, khoảng trắng.");
                }

                if (fullName.Length < 2)
                {
                    return ApiResponse<string>.Fail("Họ tên phải có ít nhất 2 ký tự.");
                }

                if (!Regex.IsMatch(fullName, @"^[a-zA-ZÀ-ỹ\s]{2,100}$"))
                {
                    return ApiResponse<string>.Fail("Họ tên không được để trống và chỉ được chứa chữ cái, khoảng trắng.");
                }

                if (string.IsNullOrWhiteSpace(request.PhoneNumber) ||
                    !Regex.IsMatch(request.PhoneNumber.Trim(), @"^0\d{9}$"))
                {
                    return ApiResponse<string>.Fail("Số điện thoại không hợp lệ. Số điện thoại phải bắt đầu bằng 0 và gồm 10 chữ số.");
                }

                if (!IsStrongPassword(request.Password))
                {
                    return ApiResponse<string>.Fail("Mật khẩu phải có ít nhất 8 ký tự, gồm 1 chữ in hoa, 1 chữ thường, 1 chữ số và 1 ký tự đặc biệt.");
                }

                if (request.Password != request.ConfirmPassword)
                {
                    return ApiResponse<string>.Fail("Mật khẩu xác nhận không khớp.");
                }

                if (await _authRepository.EmailExistsAsync(email))
                {
                    return ApiResponse<string>.Fail("Email already exists.");
                }

                var now = DateTime.UtcNow;
                var passwordSeedUser = new User { Email = email };
                var pendingRegistration = await _otpRepository.GetPendingRegistrationByEmailAsync(email);

                if (pendingRegistration == null)
                {
                    pendingRegistration = new PendingRegistration
                    {
                        Email = email,
                        CreatedAt = now
                    };

                    await FillAndSavePendingRegistrationAsync(pendingRegistration, request, passwordSeedUser, isNew: true);
                }
                else
                {
                    await FillAndSavePendingRegistrationAsync(pendingRegistration, request, passwordSeedUser, isNew: false);
                }

                var otpCode = GenerateOtpCode();
                await _otpRepository.RemoveOldOtpsAsync(email, RegisterPurpose);
                await _otpRepository.SaveOtpAsync(CreateOtp(email, otpCode, RegisterPurpose));

                await _emailService.SendEmailAsync(
                    email,
                    "Xac thuc dang ky tai khoan So Tro",
                    BuildOtpEmailBody("Xac thuc dang ky tai khoan So Tro", otpCode));

                return ApiResponse<string>.Ok("OTP register has been sent to your email.");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail($"Send register OTP failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> VerifyRegisterOtpAsync(RegisterVerifyOtpRequest request)
        {
            try
            {
                var email = NormalizeEmail(request.Email);

                if (await _authRepository.EmailExistsAsync(email))
                {
                    return ApiResponse<string>.Fail("Email already exists.");
                }

                var otp = await _otpRepository.GetValidOtpAsync(email, request.Otp, RegisterPurpose);
                if (otp == null)
                {
                    return ApiResponse<string>.Fail("OTP is invalid or expired.");
                }

                var pendingRegistration = await _otpRepository.GetPendingRegistrationByEmailAsync(email);
                if (pendingRegistration == null)
                {
                    return ApiResponse<string>.Fail("Pending registration was not found.");
                }

                var now = DateTime.UtcNow;
                var user = new User
                {
                    Email = pendingRegistration.Email,
                    FullName = pendingRegistration.FullName,
                    PhoneNumber = pendingRegistration.PhoneNumber,
                    PasswordHash = pendingRegistration.PasswordHash,
                    Provider = "Local",
                    IsExternalLogin = false,
                    IsProfileCompleted = !string.IsNullOrWhiteSpace(pendingRegistration.PhoneNumber),
                    Status = "Active",
                    EmailConfirmed = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                await _authRepository.CreateUserAsync(user);

                pendingRegistration.IsVerified = true;
                pendingRegistration.UpdatedAt = now;
                await _otpRepository.UpdatePendingRegistrationAsync(pendingRegistration);
                await _otpRepository.MarkOtpAsUsedAsync(otp);

                return ApiResponse<string>.Ok("Register OTP verified successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail($"Verify register OTP failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> ResendRegisterOtpAsync(RegisterResendOtpRequest request)
        {
            try
            {
                var email = NormalizeEmail(request.Email);

                if (await _authRepository.EmailExistsAsync(email))
                {
                    return ApiResponse<string>.Fail("Email already exists.");
                }

                var pendingRegistration = await _otpRepository.GetPendingRegistrationByEmailAsync(email);
                if (pendingRegistration == null || pendingRegistration.IsVerified)
                {
                    return ApiResponse<string>.Fail("Pending registration was not found.");
                }

                var otpCode = GenerateOtpCode();
                await _otpRepository.RemoveOldOtpsAsync(email, RegisterPurpose);
                await _otpRepository.SaveOtpAsync(CreateOtp(email, otpCode, RegisterPurpose));

                await _emailService.SendEmailAsync(
                    email,
                    "Xac thuc dang ky tai khoan So Tro",
                    BuildOtpEmailBody("Xac thuc dang ky tai khoan So Tro", otpCode));

                return ApiResponse<string>.Ok("OTP register has been resent to your email.");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail($"Resend register OTP failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                var user = await _authRepository.GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    return ApiResponse<AuthResponse>.Fail("Email or password is incorrect.");
                }

                if (string.Equals(user.Status, "Locked", StringComparison.OrdinalIgnoreCase))
                {
                    return ApiResponse<AuthResponse>.Fail("Account is locked.");
                }

                var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
                if (passwordResult == PasswordVerificationResult.Failed)
                {
                    return ApiResponse<AuthResponse>.Fail("Email or password is incorrect.");
                }

                return ApiResponse<AuthResponse>.Ok("Login successfully.", CreateAuthResponse(user));
            }
            catch (Exception ex)
            {
                return ApiResponse<AuthResponse>.Fail($"Login failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> SendForgotPasswordOtpAsync(ForgotPasswordSendOtpRequest request)
        {
            try
            {
                var email = NormalizeEmail(request.Email);
                var user = await _authRepository.GetUserByEmailAsync(email);

                if (user == null)
                {
                    return ApiResponse<string>.Ok("If the email exists, an OTP has been sent.");
                }

                var otpCode = GenerateOtpCode();
                await _otpRepository.RemoveOldOtpsAsync(email, ForgotPasswordPurpose);
                await _otpRepository.SaveOtpAsync(CreateOtp(email, otpCode, ForgotPasswordPurpose));

                await _emailService.SendEmailAsync(
                    email,
                    "Xac thuc quen mat khau So Tro",
                    BuildOtpEmailBody("Xac thuc quen mat khau So Tro", otpCode));

                return ApiResponse<string>.Ok("If the email exists, an OTP has been sent.");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail($"Send forgot password OTP failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> VerifyForgotPasswordOtpAsync(ForgotPasswordVerifyOtpRequest request)
        {
            try
            {
                var email = NormalizeEmail(request.Email);
                var otp = await _otpRepository.GetValidOtpAsync(email, request.Otp, ForgotPasswordPurpose);

                if (otp == null)
                {
                    return ApiResponse<string>.Fail("OTP is invalid or expired.");
                }

                otp.ResetPasswordToken = GenerateSecureToken();
                otp.ResetPasswordTokenExpiry = DateTime.UtcNow.AddMinutes(_configuration.GetValue("Otp:ResetTokenExpiryMinutes", 10));
                await _otpRepository.UpdateOtpAsync(otp);

                return ApiResponse<string>.Ok("Forgot password OTP verified successfully.", otp.ResetPasswordToken);
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail($"Verify forgot password OTP failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequest request)
        {
            try
            {
                if (!IsStrongPassword(request.NewPassword))
                {
                    return ApiResponse<string>.Fail("Mật khẩu phải có ít nhất 8 ký tự, gồm 1 chữ in hoa, 1 chữ thường, 1 chữ số và 1 ký tự đặc biệt.");
                }

                if (request.NewPassword != request.ConfirmPassword)
                {
                    return ApiResponse<string>.Fail("Mật khẩu xác nhận không khớp.");
                }

                var user = await _authRepository.GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    return ApiResponse<string>.Fail("Invalid reset password request.");
                }

                var otp = await _otpRepository.GetValidResetTokenAsync(request.Email, request.ResetPasswordToken);
                if (otp == null)
                {
                    return ApiResponse<string>.Fail("Reset password token is invalid or expired.");
                }

                user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _authRepository.UpdateUserAsync(user);
                await _otpRepository.MarkOtpAsUsedAsync(otp);

                return ApiResponse<string>.Ok("Password has been reset successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail($"Reset password failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<AuthResponse>> GoogleLoginAsync(GoogleLoginRequest request)
        {
            try
            {
                var email = NormalizeEmail(request.Email);
                var now = DateTime.UtcNow;
                var user = await _authRepository.GetUserByEmailAsync(email);

                if (user == null)
                {
                    user = new User
                    {
                        Email = email,
                        FullName = request.FullName.Trim(),
                        GoogleId = request.GoogleId,
                        AvatarUrl = request.AvatarUrl,
                        Provider = "Google",
                        IsExternalLogin = true,
                        EmailConfirmed = true,
                        PasswordHash = string.Empty,
                        PhoneNumber = null,
                        IsProfileCompleted = false,
                        Status = "Active",
                        CreatedAt = now,
                        UpdatedAt = now
                    };

                    await _authRepository.CreateUserAsync(user);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(user.GoogleId))
                    {
                        user.GoogleId = request.GoogleId;
                    }

                    user.Provider = string.IsNullOrWhiteSpace(user.Provider) ? "Google" : user.Provider;
                    user.IsExternalLogin = true;
                    user.EmailConfirmed = true;
                    user.AvatarUrl = string.IsNullOrWhiteSpace(user.AvatarUrl) ? request.AvatarUrl : user.AvatarUrl;
                    user.IsProfileCompleted = !string.IsNullOrWhiteSpace(user.PhoneNumber);
                    user.UpdatedAt = now;

                    await _authRepository.UpdateUserAsync(user);
                }

                return ApiResponse<AuthResponse>.Ok("Đăng nhập Google thành công.", CreateAuthResponse(user));
            }
            catch (Exception ex)
            {
                return ApiResponse<AuthResponse>.Fail($"Google login failed: {ex.Message}");
            }
        }

        public async Task<ApiResponse<AuthResponse>> CompleteProfileAsync(CompleteProfileRequest request)
        {
            try
            {
                var user = await _authRepository.GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    return ApiResponse<AuthResponse>.Fail("User was not found.");
                }

                if (string.IsNullOrWhiteSpace(request.PhoneNumber) ||
                    !Regex.IsMatch(request.PhoneNumber.Trim(), @"^0\d{9}$"))
                {
                    return ApiResponse<AuthResponse>.Fail("Số điện thoại không hợp lệ. Số điện thoại phải bắt đầu bằng 0 và gồm 10 chữ số.");
                }

                user.PhoneNumber = request.PhoneNumber.Trim();
                user.IsProfileCompleted = true;
                user.UpdatedAt = DateTime.UtcNow;

                await _authRepository.UpdateUserAsync(user);

                return ApiResponse<AuthResponse>.Ok("Cập nhật thông tin thành công.", CreateAuthResponse(user));
            }
            catch (Exception ex)
            {
                return ApiResponse<AuthResponse>.Fail($"Complete profile failed: {ex.Message}");
            }
        }

        private async Task FillAndSavePendingRegistrationAsync(
            PendingRegistration pendingRegistration,
            RegisterSendOtpRequest request,
            User passwordSeedUser,
            bool isNew)
        {
            var now = DateTime.UtcNow;
            pendingRegistration.FullName = request.FullName.Trim();
            pendingRegistration.Email = NormalizeEmail(request.Email);
            pendingRegistration.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
            pendingRegistration.PasswordHash = _passwordHasher.HashPassword(passwordSeedUser, request.Password);
            pendingRegistration.IsVerified = false;
            pendingRegistration.UpdatedAt = now;

            if (isNew)
            {
                await _otpRepository.SavePendingRegistrationAsync(pendingRegistration);
            }
            else
            {
                await _otpRepository.UpdatePendingRegistrationAsync(pendingRegistration);
            }
        }

        private OtpVerification CreateOtp(string email, string otpCode, string purpose)
        {
            var now = DateTime.UtcNow;

            return new OtpVerification
            {
                Email = email,
                OtpCode = otpCode,
                Purpose = purpose,
                ExpiredAt = now.AddMinutes(_configuration.GetValue("Otp:ExpiryMinutes", 5)),
                IsUsed = false,
                CreatedAt = now
            };
        }

        private AuthResponse CreateAuthResponse(User user)
        {
            var expiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetValue("Jwt:ExpiresInMinutes", 60));
            var isProfileCompleted = user.IsProfileCompleted || !string.IsNullOrWhiteSpace(user.PhoneNumber);

            return new AuthResponse
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role?.RoleName,
                Token = GenerateJwtToken(user, expiresAt),
                ExpiresAt = expiresAt,
                IsProfileCompleted = isProfileCompleted,
                RequiresProfileCompletion = !isProfileCompleted,
                User = new UserResponse
                {
                    Id = user.UserId,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    IsProfileCompleted = isProfileCompleted,
                    AvatarUrl = user.AvatarUrl,
                    Provider = user.Provider
                }
            };
        }

        private string GenerateJwtToken(User user, DateTime expiresAt)
        {
            var secretKey = _configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                throw new InvalidOperationException("JWT key is not configured.");
            }

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            if (!string.IsNullOrWhiteSpace(user.Role?.RoleName))
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Role.RoleName));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateOtpCode()
        {
            return RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        }

        private static string GenerateSecureToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        private static string BuildOtpEmailBody(string title, string otpCode)
        {
            return $@"
                <h3>{System.Net.WebUtility.HtmlEncode(title)}</h3>
                <p>Ma OTP cua ban la: <b>{otpCode}</b></p>
                <p>Ma nay co hieu luc trong 5 phut.</p>";
        }

        private static bool IsStrongPassword(string password)
        {
            return Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,}$");
        }

        private static string NormalizeEmail(string email)
        {
            return email.Trim().ToLowerInvariant();
        }
    }
}
