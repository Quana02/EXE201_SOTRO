using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class RentalAttachment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AttachmentId { get; set; }

        public int? RentalId { get; set; }

        [StringLength(500)]
        public string? FileUrl { get; set; }

        [StringLength(255)]
        public string? FileName { get; set; }

        [StringLength(50)]
        public string? FileType { get; set; }

        [StringLength(255)]
        public string? Description { get; set; }

        public DateTime? CreatedAt { get; set; }

        // Navigation property
        [ForeignKey(nameof(RentalId))]
        public virtual RentalRecord? RentalRecord { get; set; }
    }
}
