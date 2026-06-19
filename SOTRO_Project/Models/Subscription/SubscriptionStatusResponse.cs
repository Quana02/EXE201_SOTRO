namespace SOTRO_Project.Models.Subscription
{
    public class SubscriptionStatusResponse
    {
        public bool IsPremium { get; set; }
        public string PlanName { get; set; } = "Free";
        public DateTime? EndDate { get; set; }
    }
}
