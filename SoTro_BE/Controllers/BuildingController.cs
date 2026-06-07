using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoTro_BE.DTOs.Auth;
using SoTro_BE.DTOs.Building;
using SoTro_BE.Services;
using System.Security.Claims;

namespace SoTro_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BuildingController : ControllerBase
    {
        private readonly IBuildingService _buildingService;

        public BuildingController(IBuildingService buildingService)
        {
            _buildingService = buildingService;
        }

        /// <summary>
        /// Lấy danh sách loại hình nhà trọ (cho dropdown)
        /// </summary>
        [HttpGet("types")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBuildingTypes()
        {
            var response = await _buildingService.GetBuildingTypesAsync();
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Gợi ý địa chỉ (Backend Proxy tích hợp Goong, Mapbox, OpenStreetMap)
        /// </summary>
        [HttpGet("suggest-address")]
        [AllowAnonymous]
        public async Task<IActionResult> SuggestAddress([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Ok(new ApiResponse<List<string>> { Success = true, Data = new List<string>() });
            }

            var response = await _buildingService.SuggestAddressAsync(query);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Lấy danh sách nhà trọ của landlord hiện tại
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetBuildings()
        {
            var landlordId = GetLandlordId();
            if (landlordId == null)
                return Unauthorized(new { Success = false, Message = "Không tìm thấy thông tin chủ trọ." });

            var response = await _buildingService.GetBuildingsByLandlordAsync(landlordId.Value);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Lấy chi tiết nhà trọ theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBuilding(int id)
        {
            var landlordId = GetLandlordId();
            if (landlordId == null)
                return Unauthorized(new { Success = false, Message = "Không tìm thấy thông tin chủ trọ." });

            var response = await _buildingService.GetBuildingByIdAsync(id, landlordId.Value);
            return response.Success ? Ok(response) : NotFound(response);
        }

        /// <summary>
        /// Tạo nhà trọ mới (multipart/form-data, bắt buộc kèm ảnh)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateBuilding([FromForm] CreateBuildingRequest request, List<IFormFile> images)
        {
            var landlordId = GetLandlordId();
            if (landlordId == null)
                return Unauthorized(new { Success = false, Message = "Không tìm thấy thông tin chủ trọ." });

            if (images == null || images.Count == 0)
            {
                return BadRequest(new { Success = false, Message = "Vui lòng tải lên ít nhất 1 ảnh." });
            }

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
            var imageFiles = new List<(Stream Stream, string FileName)>();

            foreach (var image in images)
            {
                if (image.Length == 0) continue;

                if (!allowedTypes.Contains(image.ContentType.ToLower()))
                {
                    return BadRequest(new { Success = false, Message = $"File '{image.FileName}' không hợp lệ. Chỉ chấp nhận JPG, PNG, WebP, GIF." });
                }

                if (image.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { Success = false, Message = $"File '{image.FileName}' vượt quá 5MB." });
                }

                imageFiles.Add((image.OpenReadStream(), image.FileName));
            }

            if (imageFiles.Count == 0)
            {
                return BadRequest(new { Success = false, Message = "Vui lòng tải lên ít nhất 1 ảnh hợp lệ." });
            }

            try
            {
                var response = await _buildingService.CreateBuildingAsync(landlordId.Value, request, imageFiles);
                return response.Success ? Ok(response) : BadRequest(response);
            }
            finally
            {
                foreach (var (stream, _) in imageFiles)
                {
                    stream.Dispose();
                }
            }
        }

        /// <summary>
        /// Cập nhật nhà trọ (multipart/form-data, kèm ảnh tùy chọn)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBuilding(int id, [FromForm] CreateBuildingRequest request, List<IFormFile>? images)
        {
            var landlordId = GetLandlordId();
            if (landlordId == null)
                return Unauthorized(new { Success = false, Message = "Không tìm thấy thông tin chủ trọ." });

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
            List<(Stream Stream, string FileName)>? imageFiles = null;

            if (images != null && images.Count > 0)
            {
                imageFiles = new List<(Stream, string)>();
                foreach (var image in images)
                {
                    if (image.Length == 0) continue;

                    if (!allowedTypes.Contains(image.ContentType.ToLower()))
                    {
                        return BadRequest(new { Success = false, Message = $"File '{image.FileName}' không hợp lệ. Chỉ chấp nhận JPG, PNG, WebP, GIF." });
                    }

                    if (image.Length > 5 * 1024 * 1024)
                    {
                        return BadRequest(new { Success = false, Message = $"File '{image.FileName}' vượt quá 5MB." });
                    }

                    imageFiles.Add((image.OpenReadStream(), image.FileName));
                }
            }

            try
            {
                var response = await _buildingService.UpdateBuildingAsync(id, landlordId.Value, request, imageFiles);
                return response.Success ? Ok(response) : BadRequest(response);
            }
            finally
            {
                if (imageFiles != null)
                {
                    foreach (var (stream, _) in imageFiles)
                    {
                        stream.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Xóa nhà trọ (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBuilding(int id)
        {
            var landlordId = GetLandlordId();
            if (landlordId == null)
                return Unauthorized(new { Success = false, Message = "Không tìm thấy thông tin chủ trọ." });

            var response = await _buildingService.DeleteBuildingAsync(id, landlordId.Value);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        private int? GetLandlordId()
        {
            var landlordIdClaim = User.FindFirst("LandlordId")?.Value;
            if (int.TryParse(landlordIdClaim, out var landlordId))
                return landlordId;
            return null;
        }
    }
}
