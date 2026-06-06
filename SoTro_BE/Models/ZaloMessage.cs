using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class ZaloMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MessageId { get; set; }

        public int? InvoiceId { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(50)]
        public string? MessageType { get; set; } // Invoice, Reminder

        public string? Content { get; set; }

        [StringLength(30)]
        public string? Status { get; set; } // Pending, Sent, Failed

        public DateTime? SentAt { get; set; }

        public string? ResponseData { get; set; }

        public DateTime? CreatedAt { get; set; }

        // Navigation property
        [ForeignKey(nameof(InvoiceId))]
        public virtual Invoice? Invoice { get; set; }
    }
}
