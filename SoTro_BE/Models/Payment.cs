using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class Payment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentId { get; set; }

        public int? InvoiceId { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal? Amount { get; set; }

        [StringLength(30)]
        public string? PaymentMethod { get; set; } // Cash, BankTransfer, Momo, Other

        [StringLength(100)]
        public string? TransactionCode { get; set; }

        public DateTime? PaidAt { get; set; }

        public int? ReceivedBy { get; set; }

        public string? Note { get; set; }

        public DateTime? CreatedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(InvoiceId))]
        public virtual Invoice? Invoice { get; set; }

        [ForeignKey(nameof(ReceivedBy))]
        public virtual User? Receiver { get; set; }
    }
}
