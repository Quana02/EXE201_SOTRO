using System.ComponentModel.DataAnnotations;

namespace SOTRO_Project.Models.Auth
{
    public class RegisterSendOtpRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [MinLength(8, ErrorMessage = "Mật khẩu tối thiểu 8 ký tự.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu.")]
        [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
