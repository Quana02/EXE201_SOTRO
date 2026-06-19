namespace SOTRO_Project.Models.Admin
{
    public class AdminAccountResponse
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public int? RoleId { get; set; }
        public string? RoleName { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? LandlordId { get; set; }
        public int? CurrentPlanId { get; set; }
        public string? CurrentPlanName { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
    }

    public class AdminSubscriptionPlanResponse
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public int? DurationDays { get; set; }
        public int? MaxBuildings { get; set; }
        public int? MaxRooms { get; set; }
    }

    public class UpdateAccountStatusRequest
    {
        public string? Status { get; set; }
    }

    public class UpdateAccountSubscriptionRequest
    {
        public int? PlanId { get; set; }
    }
}
