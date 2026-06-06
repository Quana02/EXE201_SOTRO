using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class SubscriptionPlan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PlanId { get; set; }

        [Required]
        [StringLength(100)]
        public string PlanName { get; set; } = null!;

        [Column(TypeName = "numeric(18,2)")]
        public decimal? Price { get; set; }

        public int? DurationDays { get; set; }

        public int? MaxBuildings { get; set; }
        public int? MaxRooms { get; set; }

        public bool? CanUseZalo { get; set; }
        public bool? CanUseFacebookPosting { get; set; }
        public bool? CanUseOCR { get; set; }
        public bool? CanExportExcel { get; set; }

        public string? Description { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public virtual ICollection<LandlordSubscription> LandlordSubscriptions { get; set; } = new List<LandlordSubscription>();
    }
}
