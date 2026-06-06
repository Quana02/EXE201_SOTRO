using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class RoomOccupant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OccupantId { get; set; }

        public int? RoomId { get; set; }

        public int? RentalId { get; set; }

        public int? TenantId { get; set; }

        public bool? IsPrimaryTenant { get; set; }

        public DateOnly? MoveInDate { get; set; }
        public DateOnly? MoveOutDate { get; set; }

        [StringLength(30)]
        public string? Status { get; set; } // Living, MovedOut

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(RoomId))]
        public virtual Room? Room { get; set; }

        [ForeignKey(nameof(RentalId))]
        public virtual RentalRecord? RentalRecord { get; set; }

        [ForeignKey(nameof(TenantId))]
        public virtual Tenant? Tenant { get; set; }
    }
}
