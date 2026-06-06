using System.ComponentModel.DataAnnotations;

namespace SoTro_BE.DTOs.Auth
{
    public class RegisterResendOtpRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
