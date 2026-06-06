using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class IntegrationSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IntegrationId { get; set; }

        public int? LandlordId { get; set; }

        [StringLength(50)]
        public string? Provider { get; set; } // Zalo, Facebook

        public string? AccessToken { get; set; }

        public string? RefreshToken { get; set; }

        [StringLength(100)]
        public string? ExternalPageId { get; set; }

        [StringLength(30)]
        public string? Status { get; set; } // Active, Expired, Disconnected

        public DateTime? ExpiredAt { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        [ForeignKey(nameof(LandlordId))]
        public virtual Landlord? Landlord { get; set; }
    }
}
