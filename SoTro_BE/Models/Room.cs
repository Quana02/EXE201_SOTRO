using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class Room
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoomId { get; set; }

        public int? BuildingId { get; set; }

        public int? RoomTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string RoomCode { get; set; } = null!;

        public int? FloorNumber { get; set; }

        [Column(TypeName = "numeric(10,2)")]
        public decimal? Area { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal? BasePrice { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal? ElectricPrice { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal? WaterPrice { get; set; }

        public int? Capacity { get; set; }

        public int? CurrentTenantCount { get; set; }

        [StringLength(30)]
        public string? Status { get; set; } // Available, Occupied, Reserved, Maintenance, Inactive

        public string? Note { get; set; }

        public bool? IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(BuildingId))]
        public virtual Building? Building { get; set; }

        [ForeignKey(nameof(RoomTypeId))]
        public virtual RoomType? RoomType { get; set; }

        public virtual ICollection<RoomImage> RoomImages { get; set; } = new List<RoomImage>();
        public virtual ICollection<RoomStatusHistory> RoomStatusHistories { get; set; } = new List<RoomStatusHistory>();
        public virtual ICollection<RoomAsset> RoomAssets { get; set; } = new List<RoomAsset>();
        public virtual ICollection<RentalRecord> RentalRecords { get; set; } = new List<RentalRecord>();
        public virtual ICollection<RoomOccupant> RoomOccupants { get; set; } = new List<RoomOccupant>();
        public virtual ICollection<RoomService> RoomServices { get; set; } = new List<RoomService>();
        public virtual ICollection<MeterReading> MeterReadings { get; set; } = new List<MeterReading>();
        public virtual ICollection<AdditionalCharge> AdditionalCharges { get; set; } = new List<AdditionalCharge>();
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public virtual ICollection<FacebookPostLog> FacebookPostLogs { get; set; } = new List<FacebookPostLog>();
        public virtual ICollection<MaintenanceReport> MaintenanceReports { get; set; } = new List<MaintenanceReport>();
    }
}
