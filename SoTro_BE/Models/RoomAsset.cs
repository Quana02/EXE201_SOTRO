using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class RoomAsset
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AssetId { get; set; }

        public int? RoomId { get; set; }

        [Required]
        [StringLength(100)]
        public string AssetName { get; set; } = null!;

        public int? Quantity { get; set; }

        [StringLength(50)]
        public string? Condition { get; set; } // New, Good, Damaged, Lost

        [Column(TypeName = "numeric(18,2)")]
        public decimal? EstimatedValue { get; set; }

        public string? Note { get; set; }

        public bool? IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        [ForeignKey(nameof(RoomId))]
        public virtual Room? Room { get; set; }
    }
}
