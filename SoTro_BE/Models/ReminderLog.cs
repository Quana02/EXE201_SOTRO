using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class ReminderLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReminderId { get; set; }

        public int? InvoiceId { get; set; }

        public DateTime? ReminderDate { get; set; }

        [StringLength(50)]
        public string? ReminderType { get; set; } // BeforeDueDate, AfterDueDate, Manual

        [StringLength(30)]
        public string? Status { get; set; } // Pending, Sent, Failed

        public DateTime? CreatedAt { get; set; }

        // Navigation property
        [ForeignKey(nameof(InvoiceId))]
        public virtual Invoice? Invoice { get; set; }
    }
}
