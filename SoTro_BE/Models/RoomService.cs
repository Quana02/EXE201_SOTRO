using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class RoomService
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoomServiceId { get; set; }

        public int? RoomId { get; set; }

        public int? ServiceId { get; set; }

        [Column(TypeName = "numeric(10,2)")]
        public decimal? Quantity { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal? CustomPrice { get; set; }

        public DateOnly? EffectiveFrom { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(RoomId))]
        public virtual Room? Room { get; set; }

        [ForeignKey(nameof(ServiceId))]
        public virtual Service? Service { get; set; }

        public virtual ICollection<ServicePriceHistory> ServicePriceHistories { get; set; } = new List<ServicePriceHistory>();
    }
}
