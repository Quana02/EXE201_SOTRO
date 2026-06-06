using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class Role
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoleId { get; set; }

        [Required]
        [StringLength(50)]
        public string RoleName { get; set; } = null!;

        [StringLength(255)]
        public string? Description { get; set; }

        public DateTime? CreatedAt { get; set; }

        // Navigation property
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
