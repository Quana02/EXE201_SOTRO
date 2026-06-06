using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class InvoiceItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InvoiceItemId { get; set; }

        public int? InvoiceId { get; set; }

        [StringLength(50)]
        public string? ItemType { get; set; } // Room, Electric, Water, Service, Additional, Discount

        [StringLength(100)]
        public string? ItemName { get; set; }

        [Column(TypeName = "numeric(10,2)")]
        public decimal? Quantity { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal? UnitPrice { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal? Amount { get; set; }

        public string? Description { get; set; }

        // Navigation property
        [ForeignKey(nameof(InvoiceId))]
        public virtual Invoice? Invoice { get; set; }
    }
}
