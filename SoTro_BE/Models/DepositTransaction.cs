using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class DepositTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DepositTransactionId { get; set; }

        public int? RentalId { get; set; }

        [StringLength(30)]
        public string? TransactionType { get; set; } // Collect, Refund, Deduct

        [Column(TypeName = "numeric(18,2)")]
        public decimal? Amount { get; set; }

        [StringLength(30)]
        public string? PaymentMethod { get; set; } // Cash, BankTransfer, Momo, Other

        public DateTime? TransactionDate { get; set; }

        public string? Note { get; set; }

        public DateTime? CreatedAt { get; set; }

        // Navigation property
        [ForeignKey(nameof(RentalId))]
        public virtual RentalRecord? RentalRecord { get; set; }
    }
}
