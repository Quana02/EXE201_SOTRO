using System.ComponentModel.DataAnnotations;

namespace SOTRO_Project.Models.Auth
{
    public class RegisterResendOtpRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
