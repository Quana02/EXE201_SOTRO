using System;
using System.ComponentModel.DataAnnotations;

namespace SOTRO_Project.Models.Building
{
    public class BuildingResponse
    {
        public int BuildingId { get; set; }
        public string BuildingName { get; set; } = null!;
        public int? BuildingTypeId { get; set; }
        public string? BuildingTypeName { get; set; }
        public string? Address { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public int? TotalFloors { get; set; }
        public int? TotalRooms { get; set; }
        public int? OccupiedRooms { get; set; }
        public int? BillingDay { get; set; }
        public int? DueDay { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class CreateBuildingRequest
    {
        [Required(ErrorMessage = "Tên nhà trọ là bắt buộc")]
        [StringLength(150, ErrorMessage = "Tên nhà trọ tối đa 150 ký tự")]
        public string BuildingName { get; set; } = null!;

        [Required(ErrorMessage = "Loại nhà trọ là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn loại hình nhà trọ")]
        public int BuildingTypeId { get; set; }

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        [StringLength(255, ErrorMessage = "Địa chỉ tối đa 255 ký tự")]
        public string Address { get; set; } = null!;

        [Required(ErrorMessage = "Mô tả nhà trọ là bắt buộc")]
        [StringLength(1000, ErrorMessage = "Mô tả tối đa 1000 ký tự")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Số tầng là bắt buộc")]
        [Range(1, 100, ErrorMessage = "Số tầng phải từ 1 đến 100")]
        public int? TotalFloors { get; set; }

        [Required(ErrorMessage = "Ngày lập hóa đơn là bắt buộc")]
        [Range(1, 31, ErrorMessage = "Ngày lập hóa đơn phải từ 1 đến 31")]
        public int? BillingDay { get; set; }

        [Required(ErrorMessage = "Ngày hết hạn thanh toán là bắt buộc")]
        [Range(1, 31, ErrorMessage = "Ngày hết hạn thanh toán phải từ 1 đến 31")]
        public int? DueDay { get; set; }
    }

    public class BuildingTypeResponse
    {
        public int BuildingTypeId { get; set; }
        public string TypeName { get; set; } = null!;
        public string? Description { get; set; }
    }
}
