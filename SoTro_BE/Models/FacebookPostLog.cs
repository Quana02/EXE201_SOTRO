using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class FacebookPostLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PostId { get; set; }

        public int? RoomId { get; set; }

        [StringLength(100)]
        public string? FacebookPageId { get; set; }

        public string? PostContent { get; set; }

        [StringLength(500)]
        public string? PostUrl { get; set; }

        [StringLength(30)]
        public string? Status { get; set; } // Pending, Posted, Failed

        public DateTime? PostedAt { get; set; }

        public DateTime? CreatedAt { get; set; }

        // Navigation property
        [ForeignKey(nameof(RoomId))]
        public virtual Room? Room { get; set; }
    }
}
