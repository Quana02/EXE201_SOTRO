using System.ComponentModel.DataAnnotations;

namespace SoTro_BE.DTOs.Auth
{
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu hiện tại không được để trống.")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới không được để trống.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,}$", ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự, gồm 1 chữ in hoa, 1 chữ thường, 1 chữ số và 1 ký tự đặc biệt.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu xác nhận không được để trống.")]
        [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
