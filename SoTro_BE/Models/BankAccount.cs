using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class BankAccount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BankAccountId { get; set; }

        public int? LandlordId { get; set; }

        [StringLength(100)]
        public string? BankName { get; set; }

        [StringLength(50)]
        public string? AccountNumber { get; set; }

        [StringLength(100)]
        public string? AccountHolder { get; set; }

        [StringLength(255)]
        public string? QrTemplate { get; set; }

        public bool? IsDefault { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        [ForeignKey(nameof(LandlordId))]
        public virtual Landlord? Landlord { get; set; }
    }
}
