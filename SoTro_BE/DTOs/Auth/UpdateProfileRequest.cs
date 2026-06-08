using System.ComponentModel.DataAnnotations;

namespace SoTro_BE.DTOs.Auth
{
    public class UpdateProfileRequest
    {
        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên không được để trống.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2 đến 100 ký tự.")]
        [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]+$", ErrorMessage = "Họ tên chỉ được chứa chữ cái và khoảng trắng.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống.")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại không hợp lệ. Số điện thoại phải bắt đầu bằng 0 và gồm 10 chữ số.")]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
