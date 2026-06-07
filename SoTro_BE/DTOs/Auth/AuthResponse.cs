namespace SoTro_BE.DTOs.Auth
{
    public class AuthResponse
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Role { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool RequiresProfileCompletion { get; set; }
        public bool IsProfileCompleted { get; set; }
        public UserResponse? User { get; set; }
    }
}
