using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoTro_BE.Data;
using SoTro_BE.DTOs.Auth;

namespace SoTro_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubscriptionsController : ControllerBase
    {
        private readonly SoTroDbContext _context;

        public SubscriptionsController(SoTroDbContext context)
        {
            _context = context;
        }

        [HttpGet("current")]
        public async Task<ActionResult<ApiResponse<SubscriptionStatusResponse>>> GetCurrentSubscription()
        {
            var landlordId = GetLandlordId();
            if (landlordId == null)
                return Unauthorized(ApiResponse<SubscriptionStatusResponse>.Fail("Không tìm thấy thông tin chủ trọ."));

            var now = DateTime.UtcNow;
            var subscription = await _context.LandlordSubscriptions
                .AsNoTracking()
                .Include(s => s.SubscriptionPlan)
                .Where(s =>
                    s.LandlordId == landlordId.Value &&
                    s.Status == "Active" &&
                    (s.EndDate == null || s.EndDate >= now) &&
                    s.SubscriptionPlan != null &&
                    s.SubscriptionPlan.IsActive != false)
                .OrderByDescending(s => s.EndDate ?? DateTime.MaxValue)
                .FirstOrDefaultAsync();

            var data = new SubscriptionStatusResponse
            {
                IsPremium = subscription?.SubscriptionPlan != null && IsPremiumPlan(subscription.SubscriptionPlan),
                PlanName = subscription?.SubscriptionPlan?.PlanName ?? "Basic",
                EndDate = subscription?.EndDate
            };

            return Ok(ApiResponse<SubscriptionStatusResponse>.Ok("Lấy trạng thái gói dịch vụ thành công.", data));
        }

        private int? GetLandlordId()
        {
            var landlordIdClaim = User.FindFirst("LandlordId")?.Value;
            return int.TryParse(landlordIdClaim, out var landlordId) ? landlordId : null;
        }

        private static bool IsPremiumPlan(Models.SubscriptionPlan plan)
        {
            return string.Equals(plan.PlanName, "Premium", StringComparison.OrdinalIgnoreCase)
                || plan.CanUseZalo == true
                || plan.CanUseFacebookPosting == true
                || plan.CanUseOCR == true;
        }
    }

    public class SubscriptionStatusResponse
    {
        public bool IsPremium { get; set; }
        public string PlanName { get; set; } = "Free";
        public DateTime? EndDate { get; set; }
    }
}
