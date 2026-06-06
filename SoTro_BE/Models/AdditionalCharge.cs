using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class AdditionalCharge
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ChargeId { get; set; }

        public int? RoomId { get; set; }

        public int? RentalId { get; set; }

        [StringLength(100)]
        public string? ChargeName { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal? Amount { get; set; }

        public int? ChargeMonth { get; set; }
        public int? ChargeYear { get; set; }

        public string? Description { get; set; }

        public bool? IsIncludedInInvoice { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(RoomId))]
        public virtual Room? Room { get; set; }

        [ForeignKey(nameof(RentalId))]
        public virtual RentalRecord? RentalRecord { get; set; }
    }
}
