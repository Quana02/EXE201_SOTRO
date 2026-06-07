using System.ComponentModel.DataAnnotations;

namespace SOTRO_Project.Models.Auth
{
    public class CompleteProfileRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^0\d{9}$")]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
