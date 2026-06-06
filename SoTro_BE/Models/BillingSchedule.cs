using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class BillingSchedule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ScheduleId { get; set; }

        public int? BuildingId { get; set; }

        public int? Month { get; set; }
        public int? Year { get; set; }

        public DateOnly? ScheduledDate { get; set; }

        [StringLength(30)]
        public string? Status { get; set; } // Pending, Processing, Completed, Failed

        public DateTime? ProcessedAt { get; set; }

        public int? GeneratedInvoiceCount { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        [ForeignKey(nameof(BuildingId))]
        public virtual Building? Building { get; set; }
    }
}
