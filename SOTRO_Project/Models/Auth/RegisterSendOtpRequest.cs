using System.ComponentModel.DataAnnotations;

namespace SOTRO_Project.Models.Auth
{
    public class RegisterSendOtpRequest
    {
        [Required(ErrorMessage = "Vui long nhap ho ten.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui long nhap email.")]
        [EmailAddress(ErrorMessage = "Email khong dung dinh dang.")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "So dien thoai khong dung dinh dang.")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui long nhap mat khau.")]
        [MinLength(8, ErrorMessage = "Mat khau toi thieu 8 ky tu.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui long nhap lai mat khau.")]
        [Compare(nameof(Password), ErrorMessage = "Mat khau xac nhan khong khop.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
