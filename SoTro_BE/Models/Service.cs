using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class Service
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ServiceId { get; set; }

        public int? LandlordId { get; set; }

        [Required]
        [StringLength(100)]
        public string ServiceName { get; set; } = null!;

        [StringLength(50)]
        public string? UnitName { get; set; } // room, person, vehicle, month, time, kWh, m3

        [Column(TypeName = "numeric(18,2)")]
        public decimal? UnitPrice { get; set; }

        [StringLength(30)]
        public string? CalculationType { get; set; } // Fixed, PerRoom, PerPerson, ByQuantity, ByMeter, Custom

        public string? Description { get; set; }

        public bool? IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(LandlordId))]
        public virtual Landlord? Landlord { get; set; }

        public virtual ICollection<RoomService> RoomServices { get; set; } = new List<RoomService>();
    }
}
