using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class Landlord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LandlordId { get; set; }

        public int? UserId { get; set; }

        [StringLength(150)]
        public string? DisplayName { get; set; }

        [StringLength(50)]
        public string? IdentityNumber { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }

        public virtual ICollection<LandlordSubscription> LandlordSubscriptions { get; set; } = new List<LandlordSubscription>();
        public virtual ICollection<Building> Buildings { get; set; } = new List<Building>();
        public virtual ICollection<RoomType> RoomTypes { get; set; } = new List<RoomType>();
        public virtual ICollection<Tenant> Tenants { get; set; } = new List<Tenant>();
        public virtual ICollection<Service> Services { get; set; } = new List<Service>();
        public virtual ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();
        public virtual SystemSetting? SystemSetting { get; set; }
        public virtual ICollection<IntegrationSetting> IntegrationSettings { get; set; } = new List<IntegrationSetting>();
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}
