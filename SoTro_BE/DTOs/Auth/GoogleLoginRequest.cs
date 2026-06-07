using System.ComponentModel.DataAnnotations;

namespace SoTro_BE.DTOs.Auth
{
    public class GoogleLoginRequest
    {
        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên không được để trống.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "GoogleId không được để trống.")]
        public string GoogleId { get; set; } = string.Empty;

        public string? AvatarUrl { get; set; }
    }
}
