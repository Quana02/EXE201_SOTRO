using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class RoomType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoomTypeId { get; set; }

        public int? LandlordId { get; set; }

        [Required]
        [StringLength(100)]
        public string TypeName { get; set; } = null!;

        public string? Description { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal? DefaultPrice { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal? DefaultElectricPrice { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal? DefaultWaterPrice { get; set; }

        public int? MaxPeople { get; set; }

        public bool? IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(LandlordId))]
        public virtual Landlord? Landlord { get; set; }

        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}
