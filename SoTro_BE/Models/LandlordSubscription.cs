using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class LandlordSubscription
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SubscriptionId { get; set; }

        public int? LandlordId { get; set; }

        public int? PlanId { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [StringLength(30)]
        public string? Status { get; set; } // Active, Expired, Cancelled

        public bool? AutoRenew { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(LandlordId))]
        public virtual Landlord? Landlord { get; set; }

        [ForeignKey(nameof(PlanId))]
        public virtual SubscriptionPlan? SubscriptionPlan { get; set; }

        public virtual ICollection<SubscriptionPayment> SubscriptionPayments { get; set; } = new List<SubscriptionPayment>();
    }
}
