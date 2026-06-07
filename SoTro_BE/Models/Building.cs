using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class Building
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BuildingId { get; set; }

        public int? LandlordId { get; set; }

        public int? BuildingTypeId { get; set; }

        [Required]
        [StringLength(150)]
        public string BuildingName { get; set; } = null!;

        [StringLength(255)]
        public string? Address { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public string? Description { get; set; }

        public int? TotalFloors { get; set; }
        public int? TotalRooms { get; set; }

        public int? BillingDay { get; set; }
        public int? DueDay { get; set; }

        [StringLength(30)]
        public string? Status { get; set; } // Active, Inactive, Maintenance

        public bool? IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(LandlordId))]
        public virtual Landlord? Landlord { get; set; }

        [ForeignKey(nameof(BuildingTypeId))]
        public virtual BuildingType? BuildingType { get; set; }

        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
        public virtual ICollection<BillingSchedule> BillingSchedules { get; set; } = new List<BillingSchedule>();
        public virtual ICollection<BuildingImage> BuildingImages { get; set; } = new List<BuildingImage>();
    }
}
