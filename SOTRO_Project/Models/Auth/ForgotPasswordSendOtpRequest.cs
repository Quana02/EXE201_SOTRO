using System.ComponentModel.DataAnnotations;

namespace SOTRO_Project.Models.Auth
{
    public class ForgotPasswordSendOtpRequest
    {
        [Required(ErrorMessage = "Vui long nhap email.")]
        [EmailAddress(ErrorMessage = "Email khong dung dinh dang.")]
        public string Email { get; set; } = string.Empty;
    }
}
