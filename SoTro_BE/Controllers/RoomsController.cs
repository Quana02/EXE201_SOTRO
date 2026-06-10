using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoTro_BE.DTOs.Room;
using SoTro_BE.Services;
using System.Security.Claims;

namespace SoTro_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomsController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        /// <summary>
        /// Lấy danh sách phòng theo nhà trọ
        /// GET api/rooms?buildingId=1
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRooms([FromQuery] int buildingId)
        {
            var landlordId = GetLandlordId();
            if (landlordId == null)
                return Unauthorized(new { Success = false, Message = "Không tìm thấy thông tin chủ trọ." });

            var response = await _roomService.GetRoomsByBuildingAsync(buildingId, landlordId.Value);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Lấy thống kê phòng theo nhà trọ
        /// GET api/rooms/stats?buildingId=1
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetRoomStats([FromQuery] int buildingId)
        {
            var landlordId = GetLandlordId();
            if (landlordId == null)
                return Unauthorized(new { Success = false, Message = "Không tìm thấy thông tin chủ trọ." });

            var response = await _roomService.GetRoomStatsAsync(buildingId, landlordId.Value);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Lấy chi tiết phòng (kèm lịch sử, sự cố, người thuê)
        /// GET api/rooms/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoom(int id)
        {
            var landlordId = GetLandlordId();
            if (landlordId == null)
                return Unauthorized(new { Success = false, Message = "Không tìm thấy thông tin chủ trọ." });

            var response = await _roomService.GetRoomByIdAsync(id, landlordId.Value);
            return response.Success ? Ok(response) : NotFound(response);
        }

        /// <summary>
        /// Tạo phòng mới
        /// POST api/rooms?buildingId=1
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromQuery] int buildingId, [FromBody] CreateRoomRequest request)
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

            var response = await _roomService.CreateRoomAsync(buildingId, landlordId.Value, request);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Cập nhật thông tin phòng
        /// PUT api/rooms/{id}
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoom(int id, [FromBody] UpdateRoomRequest request)
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

            var response = await _roomService.UpdateRoomAsync(id, landlordId.Value, request);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Xóa phòng (soft delete)
        /// DELETE api/rooms/{id}
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var landlordId = GetLandlordId();
            if (landlordId == null)
                return Unauthorized(new { Success = false, Message = "Không tìm thấy thông tin chủ trọ." });

            var response = await _roomService.DeleteRoomAsync(id, landlordId.Value);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Đổi trạng thái phòng (Available / Occupied / Maintenance)
        /// PATCH api/rooms/{id}/status
        /// </summary>
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeRoomStatusRequest request)
        {
            var landlordId = GetLandlordId();
            if (landlordId == null)
                return Unauthorized(new { Success = false, Message = "Không tìm thấy thông tin chủ trọ." });

            var userId = GetUserId();
            if (userId == null)
                return Unauthorized(new { Success = false, Message = "Không tìm thấy thông tin người dùng." });

            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(new { Success = false, Message = "Dữ liệu không hợp lệ.", Errors = errors });
            }

            var response = await _roomService.ChangeRoomStatusAsync(id, landlordId.Value, userId.Value, request);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        private int? GetLandlordId()
        {
            var landlordIdClaim = User.FindFirst("LandlordId")?.Value;
            if (int.TryParse(landlordIdClaim, out var landlordId))
                return landlordId;
            return null;
        }

        private int? GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId))
                return userId;
            return null;
        }
    }
}
