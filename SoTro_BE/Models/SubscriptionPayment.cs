using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class SubscriptionPayment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SubscriptionPaymentId { get; set; }

        public int? SubscriptionId { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal? Amount { get; set; }

        [StringLength(30)]
        public string? PaymentMethod { get; set; } // Cash, BankTransfer, Momo, VNPay

        [StringLength(100)]
        public string? TransactionCode { get; set; }

        public DateTime? PaidAt { get; set; }

        [StringLength(30)]
        public string? Status { get; set; } // Pending, Paid, Failed, Cancelled

        public string? Note { get; set; }

        public DateTime? CreatedAt { get; set; }

        // Navigation property
        [ForeignKey(nameof(SubscriptionId))]
        public virtual LandlordSubscription? LandlordSubscription { get; set; }
    }
}
