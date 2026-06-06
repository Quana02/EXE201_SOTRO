using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class Tenant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TenantId { get; set; }

        public int? LandlordId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = null!;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(50)]
        public string? IdentityNumber { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        [StringLength(255)]
        public string? PermanentAddress { get; set; }

        [StringLength(100)]
        public string? EmergencyContactName { get; set; }

        [StringLength(20)]
        public string? EmergencyContactPhone { get; set; }

        [StringLength(30)]
        public string? Status { get; set; } // Active, Inactive, Blacklist

        public bool? IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(LandlordId))]
        public virtual Landlord? Landlord { get; set; }

        public virtual ICollection<TenantDocument> TenantDocuments { get; set; } = new List<TenantDocument>();
        public virtual ICollection<RentalRecord> RentalRecords { get; set; } = new List<RentalRecord>();
        public virtual ICollection<RoomOccupant> RoomOccupants { get; set; } = new List<RoomOccupant>();
        public virtual ICollection<MaintenanceReport> MaintenanceReports { get; set; } = new List<MaintenanceReport>();
    }
}
