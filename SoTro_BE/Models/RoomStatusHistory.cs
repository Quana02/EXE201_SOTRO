using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class RoomStatusHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StatusHistoryId { get; set; }

        public int? RoomId { get; set; }

        [StringLength(30)]
        public string? OldStatus { get; set; }

        [StringLength(30)]
        public string? NewStatus { get; set; }

        public int? ChangedBy { get; set; }

        public string? Reason { get; set; }

        public DateTime? ChangedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(RoomId))]
        public virtual Room? Room { get; set; }

        [ForeignKey(nameof(ChangedBy))]
        public virtual User? Changer { get; set; }
    }
}
