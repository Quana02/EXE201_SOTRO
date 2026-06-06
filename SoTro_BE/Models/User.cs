using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        public int? RoleId { get; set; }

        [Required]
        [StringLength(100)]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = null!;

        [StringLength(100)]
        public string? FullName { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(500)]
        public string? AvatarUrl { get; set; }

        [StringLength(30)]
        public string? Status { get; set; } // Active, Inactive, Locked

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(RoleId))]
        public virtual Role? Role { get; set; }

        public virtual Landlord? Landlord { get; set; }
        
        public virtual ICollection<RoomStatusHistory> RoomStatusHistories { get; set; } = new List<RoomStatusHistory>();
        public virtual ICollection<MeterReading> MeterReadings { get; set; } = new List<MeterReading>();
        public virtual ICollection<Payment> ReceivedPayments { get; set; } = new List<Payment>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
