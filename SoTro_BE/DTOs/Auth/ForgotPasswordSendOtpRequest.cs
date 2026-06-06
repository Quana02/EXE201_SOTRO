using System.ComponentModel.DataAnnotations;

namespace SoTro_BE.DTOs.Auth
{
    public class ForgotPasswordSendOtpRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
