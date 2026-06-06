using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class MeterReading
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReadingId { get; set; }

        public int? RoomId { get; set; }

        public int? Month { get; set; }
        public int? Year { get; set; }

        [Column(TypeName = "numeric(10,2)")]
        public decimal? OldElectricNumber { get; set; }

        [Column(TypeName = "numeric(10,2)")]
        public decimal? NewElectricNumber { get; set; }

        [Column(TypeName = "numeric(10,2)")]
        public decimal? ElectricUsage { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal? ElectricCost { get; set; }

        [Column(TypeName = "numeric(10,2)")]
        public decimal? OldWaterNumber { get; set; }

        [Column(TypeName = "numeric(10,2)")]
        public decimal? NewWaterNumber { get; set; }

        [Column(TypeName = "numeric(10,2)")]
        public decimal? WaterUsage { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal? WaterCost { get; set; }

        public bool? IsLocked { get; set; }

        public int? RecordedBy { get; set; }

        public DateTime? RecordedAt { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(RoomId))]
        public virtual Room? Room { get; set; }

        [ForeignKey(nameof(RecordedBy))]
        public virtual User? Recorder { get; set; }
    }
}
