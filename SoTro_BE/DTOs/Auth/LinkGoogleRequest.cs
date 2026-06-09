using System.ComponentModel.DataAnnotations;

namespace SoTro_BE.DTOs.Auth
{
    public class LinkGoogleRequest
    {
        [Required(ErrorMessage = "Email hiện tại không được để trống.")]
        [EmailAddress(ErrorMessage = "Email hiện tại không đúng định dạng.")]
        public string CurrentEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email Google không được để trống.")]
        [EmailAddress(ErrorMessage = "Email Google không đúng định dạng.")]
        public string GoogleEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "GoogleId không được để trống.")]
        public string GoogleId { get; set; } = string.Empty;

        public string? AvatarUrl { get; set; }
    }
}
