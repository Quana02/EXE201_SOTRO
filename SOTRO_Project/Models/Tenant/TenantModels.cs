namespace SOTRO_Project.Models.Tenant
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
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string IdentityNumber { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; }
        public string? PermanentAddress { get; set; }
        public int RoomId { get; set; }
        public DateOnly StartDate { get; set; }
        public decimal? DepositAmount { get; set; }
        public decimal? MonthlyRent { get; set; }
    }

    public class UpdateTenantRequest : CreateTenantRequest
    {
        public DateOnly? EndDate { get; set; }
        public string Status { get; set; } = "Active";
    }
}
