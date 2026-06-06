using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class TenantMember
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MemberId { get; set; }

        public int? RentalId { get; set; }

        [StringLength(100)]
        public string? FullName { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(50)]
        public string? IdentityNumber { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        [StringLength(50)]
        public string? Relationship { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        [ForeignKey(nameof(RentalId))]
        public virtual RentalRecord? RentalRecord { get; set; }
    }
}
