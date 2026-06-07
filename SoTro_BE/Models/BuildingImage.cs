using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class BuildingImage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BuildingImageId { get; set; }

        public int BuildingId { get; set; }

        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; } = null!;

        public DateTime? CreatedAt { get; set; }

        // Navigation property
        [ForeignKey(nameof(BuildingId))]
        public virtual Building? Building { get; set; }
    }
}
