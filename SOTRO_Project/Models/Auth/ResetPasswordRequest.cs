using System.ComponentModel.DataAnnotations;

namespace SOTRO_Project.Models.Auth
{
    public class ResetPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string ResetPasswordToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui long nhap mat khau moi.")]
        [MinLength(8, ErrorMessage = "Mat khau toi thieu 8 ky tu.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui long nhap lai mat khau moi.")]
        [Compare(nameof(NewPassword), ErrorMessage = "Mat khau xac nhan khong khop.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
