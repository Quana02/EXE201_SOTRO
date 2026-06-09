using System.ComponentModel.DataAnnotations;

namespace SoTro_BE.DTOs.Tenant
{
    public class TenantResponse
    {
        public int TenantId { get; set; }
        public string TenantCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? IdentityNumber { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? PermanentAddress { get; set; }
        public string? Status { get; set; }
        public int? RoomId { get; set; }
        public string? RoomCode { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public decimal? DepositAmount { get; set; }
        public decimal? MonthlyRent { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class TenantStatsResponse
    {
        public int TotalTenants { get; set; }
        public int ActiveTenants { get; set; }
        public decimal TotalDepositAmount { get; set; }
        public int ExpiringContracts { get; set; }
        public int NewThisMonth { get; set; }
        public decimal OccupancyRate { get; set; }
    }

    public class CreateTenantRequest
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [Required]
        [StringLength(50)]
        public string IdentityNumber { get; set; } = string.Empty;

        public DateOnly? DateOfBirth { get; set; }

        [StringLength(255)]
        public string? PermanentAddress { get; set; }

        [Required]
        public int RoomId { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? DepositAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MonthlyRent { get; set; }
    }

    public class UpdateTenantRequest : CreateTenantRequest
    {
        public DateOnly? EndDate { get; set; }

        [RegularExpression("Active|Inactive|Reserved|Blacklist")]
        public string Status { get; set; } = "Active";
    }
}
