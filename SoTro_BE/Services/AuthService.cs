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
        private readonly IImageUploadService _imageUploadService;
        private readonly PasswordHasher<User> _passwordHasher;

        public AuthService(
            IAuthRepository authRepository,
            IOtpRepository otpRepository,
            IEmailService emailService,
            IConfiguration configuration,
            IImageUploadService imageUploadService)
        {
            _authRepository = authRepository;
            _otpRepository = otpRepository;
            _emailService = emailService;
            _configuration = configuration;
            _imageUploadService = imageUploadService;
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
                    return ApiResponse<string>.Fail("Email đã tồn tại.");
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

                Console.WriteLine($"[DEVELOPMENT OTP] Email: {email}, OTP Code: {otpCode}");

                try
                {
                    await _emailService.SendEmailAsync(
                        email,
                        "Xac thuc dang ky tai khoan So Tro",
                        BuildOtpEmailBody("Xac thuc dang ky tai khoan So Tro", otpCode));
                }
                catch (Exception emailEx)
                {
                    Console.WriteLine($"[WARNING] Failed to send registration email to {email}: {emailEx.Message}");
                }

                return ApiResponse<string>.Ok("Mã OTP đăng ký đã được gửi đến email của bạn.");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail($"Gửi mã OTP đăng ký thất bại: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> VerifyRegisterOtpAsync(RegisterVerifyOtpRequest request)
        {
            try
            {
                var email = NormalizeEmail(request.Email);

                if (await _authRepository.EmailExistsAsync(email))
                {
                    return ApiResponse<string>.Fail("Email đã tồn tại.");
                }

                var otp = await _otpRepository.GetValidOtpAsync(email, request.Otp, RegisterPurpose);
                if (otp == null)
                {
                    return ApiResponse<string>.Fail("Mã OTP không hợp lệ hoặc đã hết hạn.");
                }

                var pendingRegistration = await _otpRepository.GetPendingRegistrationByEmailAsync(email);
                if (pendingRegistration == null)
                {
                    return ApiResponse<string>.Fail("Không tìm thấy thông tin đăng ký chờ xác thực.");
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

                return ApiResponse<string>.Ok("Xác thực OTP đăng ký thành công.");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail($"Xác thực OTP đăng ký thất bại: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> ResendRegisterOtpAsync(RegisterResendOtpRequest request)
        {
            try
            {
                var email = NormalizeEmail(request.Email);

                if (await _authRepository.EmailExistsAsync(email))
                {
                    return ApiResponse<string>.Fail("Email đã tồn tại.");
                }

                var pendingRegistration = await _otpRepository.GetPendingRegistrationByEmailAsync(email);
                if (pendingRegistration == null || pendingRegistration.IsVerified)
                {
                    return ApiResponse<string>.Fail("Không tìm thấy thông tin đăng ký chờ xác thực.");
                }

                var otpCode = GenerateOtpCode();
                await _otpRepository.RemoveOldOtpsAsync(email, RegisterPurpose);
                await _otpRepository.SaveOtpAsync(CreateOtp(email, otpCode, RegisterPurpose));

                Console.WriteLine($"[DEVELOPMENT OTP RESEND] Email: {email}, OTP Code: {otpCode}");

                try
                {
                    await _emailService.SendEmailAsync(
                        email,
                        "Xac thuc dang ky tai khoan So Tro",
                        BuildOtpEmailBody("Xac thuc dang ky tai khoan So Tro", otpCode));
                }
                catch (Exception emailEx)
                {
                    Console.WriteLine($"[WARNING] Failed to resend registration email to {email}: {emailEx.Message}");
                }

                return ApiResponse<string>.Ok("Mã OTP đăng ký đã được gửi lại đến email của bạn.");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail($"Gửi lại mã OTP đăng ký thất bại: {ex.Message}");
            }
        }

        public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                var user = await _authRepository.GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    return ApiResponse<AuthResponse>.Fail("Email hoặc mật khẩu không chính xác.");
                }

                if (string.Equals(user.Status, "Locked", StringComparison.OrdinalIgnoreCase))
                {
                    return ApiResponse<AuthResponse>.Fail("Tài khoản đã bị khóa.");
                }

                var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
                if (passwordResult == PasswordVerificationResult.Failed)
                {
                    return ApiResponse<AuthResponse>.Fail("Email hoặc mật khẩu không chính xác.");
                }

                return ApiResponse<AuthResponse>.Ok("Đăng nhập thành công.", CreateAuthResponse(user));
            }
            catch (Exception ex)
            {
                return ApiResponse<AuthResponse>.Fail($"Đăng nhập thất bại: {ex.Message}");
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
                    return ApiResponse<string>.Ok("Nếu email tồn tại trên hệ thống, một mã OTP đã được gửi.");
                }

                var otpCode = GenerateOtpCode();
                await _otpRepository.RemoveOldOtpsAsync(email, ForgotPasswordPurpose);
                await _otpRepository.SaveOtpAsync(CreateOtp(email, otpCode, ForgotPasswordPurpose));

                Console.WriteLine($"[DEVELOPMENT OTP FORGOT PASSWORD] Email: {email}, OTP Code: {otpCode}");

                try
                {
                    await _emailService.SendEmailAsync(
                        email,
                        "Xac thuc quen mat khau So Tro",
                        BuildOtpEmailBody("Xac thuc quen mat khau So Tro", otpCode));
                }
                catch (Exception emailEx)
                {
                    Console.WriteLine($"[WARNING] Failed to send forgot password email to {email}: {emailEx.Message}");
                }

                return ApiResponse<string>.Ok("Nếu email tồn tại trên hệ thống, một mã OTP đã được gửi.");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail($"Gửi mã OTP quên mật khẩu thất bại: {ex.Message}");
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
                    return ApiResponse<string>.Fail("Mã OTP không hợp lệ hoặc đã hết hạn.");
                }

                otp.ResetPasswordToken = GenerateSecureToken();
                otp.ResetPasswordTokenExpiry = DateTime.UtcNow.AddMinutes(_configuration.GetValue("Otp:ResetTokenExpiryMinutes", 10));
                await _otpRepository.UpdateOtpAsync(otp);

                return ApiResponse<string>.Ok("Xác thực OTP quên mật khẩu thành công.", otp.ResetPasswordToken);
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail($"Xác thực OTP quên mật khẩu thất bại: {ex.Message}");
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
                    return ApiResponse<string>.Fail("Yêu cầu đặt lại mật khẩu không hợp lệ.");
                }

                var otp = await _otpRepository.GetValidResetTokenAsync(request.Email, request.ResetPasswordToken);
                if (otp == null)
                {
                    return ApiResponse<string>.Fail("Mã khôi phục mật khẩu không hợp lệ hoặc đã hết hạn.");
                }

                user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _authRepository.UpdateUserAsync(user);
                await _otpRepository.MarkOtpAsUsedAsync(otp);

                return ApiResponse<string>.Ok("Đặt lại mật khẩu thành công.");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail($"Đặt lại mật khẩu thất bại: {ex.Message}");
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
                return ApiResponse<AuthResponse>.Fail($"Đăng nhập Google thất bại: {ex.Message}");
            }
        }

        public async Task<ApiResponse<AuthResponse>> CompleteProfileAsync(CompleteProfileRequest request)
        {
            try
            {
                var user = await _authRepository.GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    return ApiResponse<AuthResponse>.Fail("Không tìm thấy người dùng.");
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
                return ApiResponse<AuthResponse>.Fail($"Cập nhật thông tin thất bại: {ex.Message}");
            }
        }

        public async Task<ApiResponse<AuthResponse>> UpdateProfileAsync(UpdateProfileRequest request, Stream? avatarStream, string? avatarFileName)
        {
            try
            {
                var user = await _authRepository.GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    return ApiResponse<AuthResponse>.Fail("Không tìm thấy người dùng.");
                }

                var fullName = request.FullName?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(fullName))
                {
                    return ApiResponse<AuthResponse>.Fail("Họ tên không được để trống.");
                }
                if (fullName.Length < 2)
                {
                    return ApiResponse<AuthResponse>.Fail("Họ tên phải có ít nhất 2 ký tự.");
                }
                if (!Regex.IsMatch(fullName, @"^[a-zA-ZÀ-ỹ\s]{2,100}$"))
                {
                    return ApiResponse<AuthResponse>.Fail("Họ tên chỉ được chứa chữ cái và khoảng trắng.");
                }

                var phoneNumber = request.PhoneNumber?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(phoneNumber) || !Regex.IsMatch(phoneNumber, @"^0\d{9}$"))
                {
                    return ApiResponse<AuthResponse>.Fail("Số điện thoại không hợp lệ. Số điện thoại phải bắt đầu bằng 0 và gồm 10 chữ số.");
                }

                // Handle avatar upload
                if (avatarStream != null && !string.IsNullOrWhiteSpace(avatarFileName))
                {
                    // Delete old avatar if any
                    if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
                    {
                        try
                        {
                            await _imageUploadService.DeleteImageAsync(user.AvatarUrl);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[WARNING] Failed to delete old avatar: {ex.Message}");
                        }
                    }

                    // Upload new avatar
                    user.AvatarUrl = await _imageUploadService.UploadImageAsync(avatarStream, avatarFileName);
                }

                user.FullName = fullName;
                user.PhoneNumber = phoneNumber;
                user.IsProfileCompleted = true;
                user.UpdatedAt = DateTime.UtcNow;

                // If user is a landlord, sync display name
                if (user.Landlord != null)
                {
                    user.Landlord.DisplayName = fullName;
                    user.Landlord.UpdatedAt = DateTime.UtcNow;
                }

                await _authRepository.UpdateUserAsync(user);

                return ApiResponse<AuthResponse>.Ok("Cập nhật thông tin cá nhân thành công.", CreateAuthResponse(user));
            }
            catch (Exception ex)
            {
                return ApiResponse<AuthResponse>.Fail($"Cập nhật hồ sơ thất bại: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> ChangePasswordAsync(ChangePasswordRequest request)
        {
            try
            {
                var user = await _authRepository.GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    return ApiResponse<string>.Fail("Không tìm thấy người dùng.");
                }

                if (user.IsExternalLogin && string.IsNullOrWhiteSpace(user.PasswordHash))
                {
                    return ApiResponse<string>.Fail("Tài khoản này được đăng nhập bằng Google, không thể đổi mật khẩu.");
                }

                var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
                if (passwordResult == PasswordVerificationResult.Failed)
                {
                    return ApiResponse<string>.Fail("Mật khẩu hiện tại không chính xác.");
                }

                if (!IsStrongPassword(request.NewPassword))
                {
                    return ApiResponse<string>.Fail("Mật khẩu mới phải có ít nhất 8 ký tự, gồm 1 chữ in hoa, 1 chữ thường, 1 chữ số và 1 ký tự đặc biệt.");
                }

                if (request.NewPassword != request.ConfirmPassword)
                {
                    return ApiResponse<string>.Fail("Mật khẩu xác nhận không khớp.");
                }

                user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _authRepository.UpdateUserAsync(user);

                return ApiResponse<string>.Ok("Đổi mật khẩu thành công.");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail($"Đổi mật khẩu thất bại: {ex.Message}");
            }
        }

        public async Task<ApiResponse<AuthResponse>> LinkGoogleAccountAsync(LinkGoogleRequest request)
        {
            try
            {
                var currentEmail = NormalizeEmail(request.CurrentEmail);
                var googleEmail = NormalizeEmail(request.GoogleEmail);

                var currentUser = await _authRepository.GetUserByEmailAsync(currentEmail);
                if (currentUser == null)
                {
                    return ApiResponse<AuthResponse>.Fail("Không tìm thấy người dùng hiện tại.");
                }

                // Check if the Google ID is already linked to another account
                var existingUserByGoogleId = await _authRepository.GetUserByGoogleIdAsync(request.GoogleId);
                if (existingUserByGoogleId != null && existingUserByGoogleId.UserId != currentUser.UserId)
                {
                    return ApiResponse<AuthResponse>.Fail("Tài khoản Google này đã được liên kết với một tài khoản khác.");
                }

                // Check if Google Email is already in use by another user
                var existingUserByEmail = await _authRepository.GetUserByEmailAsync(googleEmail);
                if (existingUserByEmail != null && existingUserByEmail.UserId != currentUser.UserId)
                {
                    return ApiResponse<AuthResponse>.Fail("Email Google này đang được sử dụng bởi một tài khoản khác.");
                }

                // Update the current user account
                currentUser.Email = googleEmail;
                currentUser.GoogleId = request.GoogleId;
                currentUser.Provider = "Google";
                currentUser.IsExternalLogin = true;
                currentUser.EmailConfirmed = true;
                currentUser.UpdatedAt = DateTime.UtcNow;

                if (string.IsNullOrWhiteSpace(currentUser.AvatarUrl) && !string.IsNullOrWhiteSpace(request.AvatarUrl))
                {
                    currentUser.AvatarUrl = request.AvatarUrl;
                }

                await _authRepository.UpdateUserAsync(currentUser);

                return ApiResponse<AuthResponse>.Ok("Liên kết tài khoản Google thành công.", CreateAuthResponse(currentUser));
            }
            catch (Exception ex)
            {
                return ApiResponse<AuthResponse>.Fail($"Liên kết tài khoản Google thất bại: {ex.Message}");
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

            if (user.Landlord != null)
            {
                claims.Add(new Claim("LandlordId", user.Landlord.LandlordId.ToString()));
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
