using System.ComponentModel.DataAnnotations;

namespace SOTRO_Project.Models.Auth
{
    public class GoogleLoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string GoogleId { get; set; } = string.Empty;

        public string? AvatarUrl { get; set; }
    }
}
