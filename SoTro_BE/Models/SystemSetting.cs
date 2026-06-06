using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoTro_BE.Models
{
    public class SystemSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SettingId { get; set; }

        public int? LandlordId { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal? DefaultElectricPrice { get; set; }

        [Column(TypeName = "numeric(18,2)")]
        public decimal? DefaultWaterPrice { get; set; }

        public int? ReminderAfterDays { get; set; }

        public bool? AutoSendInvoice { get; set; }

        public bool? AutoReminder { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        [ForeignKey(nameof(LandlordId))]
        public virtual Landlord? Landlord { get; set; }
    }
}
