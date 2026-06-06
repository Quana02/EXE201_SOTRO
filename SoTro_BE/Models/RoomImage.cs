using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class RoomImage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ImageId { get; set; }

        public int? RoomId { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [StringLength(255)]
        public string? Caption { get; set; }

        public int? SortOrder { get; set; }

        public bool? IsMain { get; set; }

        public DateTime? CreatedAt { get; set; }

        // Navigation property
        [ForeignKey(nameof(RoomId))]
        public virtual Room? Room { get; set; }
    }
}
