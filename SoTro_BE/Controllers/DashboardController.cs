using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoTro_BE.Services;

namespace SoTro_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var landlordId = GetLandlordId();
            if (landlordId == null)
                return Unauthorized(new { Success = false, Message = "Không tìm thấy thông tin chủ trọ." });

            var response = await _dashboardService.GetSummaryAsync(landlordId.Value);
            return Ok(response);
        }

        private int? GetLandlordId()
        {
            var landlordIdClaim = User.FindFirst("LandlordId")?.Value;
            return int.TryParse(landlordIdClaim, out var landlordId) ? landlordId : null;
        }
    }
}
