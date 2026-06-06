using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class AuditLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LogId { get; set; }

        public int? UserId { get; set; }

        [StringLength(50)]
        public string? ActionType { get; set; } // Create, Update, Delete, Login, GenerateInvoice, RecordPayment

        [StringLength(100)]
        public string? TableName { get; set; }

        public int? RecordId { get; set; }

        public string? OldData { get; set; }

        public string? NewData { get; set; }

        public DateTime? CreatedAt { get; set; }

        // Navigation property
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
    }
}
