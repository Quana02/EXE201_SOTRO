using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class RentalRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RentalId { get; set; }

        public int? RoomId { get; set; }

        public int? TenantId { get; set; }

        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal? DepositAmount { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal? MonthlyRent { get; set; }

        public int? PaymentDueDay { get; set; }

        public int? NumberOfPeople { get; set; }

        [StringLength(30)]
        public string? Status { get; set; } // Active, Ended, Cancelled

        public string? Note { get; set; }

        public bool? IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(RoomId))]
        public virtual Room? Room { get; set; }

        [ForeignKey(nameof(TenantId))]
        public virtual Tenant? Tenant { get; set; }

        public virtual ICollection<RentalAttachment> RentalAttachments { get; set; } = new List<RentalAttachment>();
        public virtual ICollection<RoomOccupant> RoomOccupants { get; set; } = new List<RoomOccupant>();
        public virtual ICollection<TenantMember> TenantMembers { get; set; } = new List<TenantMember>();
        public virtual ICollection<DepositTransaction> DepositTransactions { get; set; } = new List<DepositTransaction>();
        public virtual ICollection<AdditionalCharge> AdditionalCharges { get; set; } = new List<AdditionalCharge>();
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}
