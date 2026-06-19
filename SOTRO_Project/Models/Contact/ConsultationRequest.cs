using System.ComponentModel.DataAnnotations;

namespace SOTRO_Project.Models.Contact
{
    public class ConsultationRequest
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;
    }
}
