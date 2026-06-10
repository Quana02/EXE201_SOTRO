using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoTro_BE.DTOs.Tenant;
using SoTro_BE.Services;

namespace SoTro_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TenantsController : ControllerBase
    {
        private readonly ITenantService _tenantService;

        public TenantsController(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTenants()
        {
            var landlordId = GetLandlordId();
            if (landlordId == null)
                return Unauthorized(new { Success = false, Message = "Không tìm thấy thông tin chủ trọ." });

            var response = await _tenantService.GetTenantsAsync(landlordId.Value);
            return Ok(response);
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var landlordId = GetLandlordId();
            if (landlordId == null)
                return Unauthorized(new { Success = false, Message = "Không tìm thấy thông tin chủ trọ." });

            var response = await _tenantService.GetTenantStatsAsync(landlordId.Value);
            return Ok(response);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetTenant(int id)
        {
            var landlordId = GetLandlordId();
            if (landlordId == null)
                return Unauthorized(new { Success = false, Message = "Không tìm thấy thông tin chủ trọ." });

            var response = await _tenantService.GetTenantByIdAsync(id, landlordId.Value);
            return response.Success ? Ok(response) : NotFound(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTenant([FromBody] CreateTenantRequest request)
        {
            var landlordId = GetLandlordId();
            if (landlordId == null)
                return Unauthorized(new { Success = false, Message = "Không tìm thấy thông tin chủ trọ." });

            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ.", Errors = errors });
            }

            var response = await _tenantService.CreateTenantAsync(landlordId.Value, request);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateTenant(int id, [FromBody] UpdateTenantRequest request)
        {
            var landlordId = GetLandlordId();
            if (landlordId == null)
                return Unauthorized(new { Success = false, Message = "Không tìm thấy thông tin chủ trọ." });

            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ.", Errors = errors });
            }

            var response = await _tenantService.UpdateTenantAsync(id, landlordId.Value, request);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTenant(int id)
        {
            var landlordId = GetLandlordId();
            if (landlordId == null)
                return Unauthorized(new { Success = false, Message = "Không tìm thấy thông tin chủ trọ." });

            var response = await _tenantService.DeleteTenantAsync(id, landlordId.Value);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        private int? GetLandlordId()
        {
            var landlordIdClaim = User.FindFirst("LandlordId")?.Value;
            return int.TryParse(landlordIdClaim, out var landlordId) ? landlordId : null;
        }
    }
}
