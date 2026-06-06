using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class MaintenanceReport
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReportId { get; set; }

        public int? RoomId { get; set; }

        public int? ReportedByTenantId { get; set; }

        [StringLength(150)]
        public string? Title { get; set; }

        public string? Description { get; set; }

        [StringLength(30)]
        public string? Priority { get; set; } // Low, Medium, High

        [StringLength(30)]
        public string? Status { get; set; } // Pending, Processing, Resolved, Cancelled

        public DateTime? ResolvedAt { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(RoomId))]
        public virtual Room? Room { get; set; }

        [ForeignKey(nameof(ReportedByTenantId))]
        public virtual Tenant? Reporter { get; set; }
    }
}
