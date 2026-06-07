namespace SOTRO_Project.Models.Auth
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public bool IsProfileCompleted { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Provider { get; set; }
    }
}
