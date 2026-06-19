using System.ComponentModel.DataAnnotations;

namespace SOTRO_Project.Models.Auth
{
    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        public string Email { get; set; } = string.Empty;
    }
}
