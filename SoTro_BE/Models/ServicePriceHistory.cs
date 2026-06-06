using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class ServicePriceHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int HistoryId { get; set; }

        public int? RoomServiceId { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal? Price { get; set; }

        public DateOnly? EffectiveFrom { get; set; }
        public DateOnly? EffectiveTo { get; set; }

        public DateTime? CreatedAt { get; set; }

        // Navigation property
        [ForeignKey(nameof(RoomServiceId))]
        public virtual RoomService? RoomService { get; set; }
    }
}
