using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class Invoice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InvoiceId { get; set; }

        public int? LandlordId { get; set; }

        public int? RoomId { get; set; }

        public int? RentalId { get; set; }

        [StringLength(50)]
        public string? InvoiceCode { get; set; }

        public int? Month { get; set; }
        public int? Year { get; set; }

        public DateTime? IssueDate { get; set; }
        public DateTime? DueDate { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal? RoomPrice { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal? ElectricCost { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal? WaterCost { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal? ServiceCost { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal? OtherCost { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal? DiscountAmount { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal? TotalAmount { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal? PaidAmount { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal? RemainingAmount { get; set; }

        [StringLength(30)]
        public string? Status { get; set; } // Unpaid, PartiallyPaid, Paid, Overdue, Cancelled

        [StringLength(500)]
        public string? QrCodeUrl { get; set; }

        public bool? SentViaZalo { get; set; }

        public bool? IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(LandlordId))]
        public virtual Landlord? Landlord { get; set; }

        [ForeignKey(nameof(RoomId))]
        public virtual Room? Room { get; set; }

        [ForeignKey(nameof(RentalId))]
        public virtual RentalRecord? RentalRecord { get; set; }

        public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<ZaloMessage> ZaloMessages { get; set; } = new List<ZaloMessage>();
        public virtual ICollection<ReminderLog> ReminderLogs { get; set; } = new List<ReminderLog>();
    }
}
