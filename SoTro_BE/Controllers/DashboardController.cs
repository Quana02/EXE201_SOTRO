using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoTro_BE.DTOs.Auth;
using SoTro_BE.DTOs.Dashboard;
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
        public async Task<IActionResult> GetSummary([FromQuery] int? buildingId, [FromQuery] int? month, [FromQuery] int? year)
        {
            var landlordId = GetLandlordId();
            if (landlordId == null)
                return Unauthorized(new { Success = false, Message = "Không tìm thấy thông tin chủ trọ." });

            if (buildingId is null or <= 0)
                return BadRequest(ApiResponse<DashboardSummaryResponse>.Fail("Vui long chon nha tro truoc khi xem dashboard."));

            var now = DateTime.UtcNow;
            var selectedMonth = month is >= 1 and <= 12 ? month.Value : now.Month;
            var selectedYear = year is >= 2000 and <= 2100 ? year.Value : now.Year;

            var response = await _dashboardService.GetSummaryAsync(landlordId.Value, buildingId, selectedMonth, selectedYear);
            return Ok(response);
        }

        private int? GetLandlordId()
        {
            var landlordIdClaim = User.FindFirst("LandlordId")?.Value;
            return int.TryParse(landlordIdClaim, out var landlordId) ? landlordId : null;
        }
    }
}
