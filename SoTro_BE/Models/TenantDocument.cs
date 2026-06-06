using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class TenantDocument
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DocumentId { get; set; }

        public int? TenantId { get; set; }

        [StringLength(50)]
        public string? DocumentType { get; set; } // CCCD_Front, CCCD_Back, TemporaryResidence, Other

        [StringLength(500)]
        public string? FileUrl { get; set; }

        public DateTime? CreatedAt { get; set; }

        // Navigation property
        [ForeignKey(nameof(TenantId))]
        public virtual Tenant? Tenant { get; set; }
    }
}
