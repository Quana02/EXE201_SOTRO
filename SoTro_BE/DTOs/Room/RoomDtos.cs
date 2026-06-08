using System.ComponentModel.DataAnnotations;

namespace SoTro_BE.DTOs.Room
{
    // ========== REQUEST DTOs ==========

    public class CreateRoomRequest
    {
        [Required(ErrorMessage = "Số phòng là bắt buộc")]
        [StringLength(50, ErrorMessage = "Số phòng tối đa 50 ký tự")]
        public string RoomCode { get; set; } = null!;

        [StringLength(100, ErrorMessage = "Tên phòng tối đa 100 ký tự")]
        public string? RoomName { get; set; }

        public string? Zone { get; set; }     // Khu / tầng mô tả dạng text (e.g. "Tầng 1", "Khu A")

        public int? FloorNumber { get; set; } // Số tầng (int)

        public int? RoomTypeId { get; set; }

        // Trạng thái ban đầu
        [RegularExpression("Available|Occupied|Maintenance", ErrorMessage = "Trạng thái không hợp lệ")]
        public string Status { get; set; } = "Available";

        // Thông tin giá
        [Required(ErrorMessage = "Tiền phòng là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Tiền phòng phải >= 0")]
        public decimal? BasePrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? ElectricPrice { get; set; }  // VNĐ/kWh

        [Range(0, double.MaxValue)]
        public decimal? WaterPrice { get; set; }     // VNĐ/m³

        [Range(0, double.MaxValue)]
        public decimal? ServiceFee { get; set; }     // Tiền dịch vụ (wifi, đỗ xe,...)

        [Range(0, double.MaxValue)]
        public decimal? IncidentFee { get; set; }    // Tiền sự cố dự phòng (sửa chữa,...)

        // Sức chứa
        public int? Capacity { get; set; }
        public int? CurrentTenantCount { get; set; }

        // Thông tin bill
        [Range(1, 15, ErrorMessage = "Ngày thu tiền phải từ ngày 1 đến ngày 15")]
        public int? BillingDay { get; set; }        // Ngày chốt điện nước (1-15)
        public int? PaymentDueDay { get; set; }     // Hạn thanh toán (1-31)

        // Thông tin thanh toán (ngân hàng)
        [StringLength(100)]
        public string? BankName { get; set; }

        [StringLength(50)]
        public string? BankAccountNumber { get; set; }

        [StringLength(500)]
        public string? BankQrUrl { get; set; }

        // Ghi chú vận hành
        public string? Note { get; set; }
    }

    public class UpdateRoomRequest
    {
        [Required(ErrorMessage = "Số phòng là bắt buộc")]
        [StringLength(50, ErrorMessage = "Số phòng tối đa 50 ký tự")]
        public string RoomCode { get; set; } = null!;

        [StringLength(100)]
        public string? RoomName { get; set; }

        public string? Zone { get; set; }

        public int? FloorNumber { get; set; }

        public int? RoomTypeId { get; set; }

        [RegularExpression("Available|Occupied|Maintenance", ErrorMessage = "Trạng thái không hợp lệ")]
        public string? Status { get; set; }

        [Required(ErrorMessage = "Tiền phòng là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Tiền phòng phải >= 0")]
        public decimal? BasePrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? ElectricPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? WaterPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? ServiceFee { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? IncidentFee { get; set; }

        public int? Capacity { get; set; }
        public int? CurrentTenantCount { get; set; }

        [Range(1, 15, ErrorMessage = "Ngày thu tiền phải từ ngày 1 đến ngày 15")]
        public int? BillingDay { get; set; }
        public int? PaymentDueDay { get; set; }

        [StringLength(100)]
        public string? BankName { get; set; }

        [StringLength(50)]
        public string? BankAccountNumber { get; set; }

        [StringLength(500)]
        public string? BankQrUrl { get; set; }

        public string? Note { get; set; }
    }

    public class ChangeRoomStatusRequest
    {
        [Required]
        [RegularExpression("Available|Occupied|Maintenance", ErrorMessage = "Trạng thái phải là: Available, Occupied, hoặc Maintenance")]
        public string NewStatus { get; set; } = null!;

        public string? Reason { get; set; }
    }

    // ========== RESPONSE DTOs ==========

    public class RoomResponse
    {
        public int RoomId { get; set; }
        public int? BuildingId { get; set; }
        public string? BuildingName { get; set; }
        public int? RoomTypeId { get; set; }
        public string? RoomTypeName { get; set; }

        // Thông tin cơ bản
        public string RoomCode { get; set; } = null!;
        public string? RoomName { get; set; }
        public string? Zone { get; set; }
        public int? FloorNumber { get; set; }
        public decimal? Area { get; set; }
        public string? Status { get; set; } // Available | Occupied | Maintenance

        // Thông tin giá
        public decimal? BasePrice { get; set; }
        public decimal? ElectricPrice { get; set; }
        public decimal? WaterPrice { get; set; }
        public decimal? ServiceFee { get; set; }
        public decimal? IncidentFee { get; set; }

        // Sức chứa
        public int? Capacity { get; set; }
        public int? CurrentTenantCount { get; set; }

        // Thông tin bill
        public int? BillingDay { get; set; }
        public int? PaymentDueDay { get; set; }
        public string? PaymentStatus { get; set; } // Paid | Overdue | Pending | null

        // Thông tin thanh toán
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankQrUrl { get; set; }

        // Ghi chú
        public string? Note { get; set; }

        // Thông tin người thuê đại diện (từ RentalRecord active)
        public TenantSummaryDto? PrimaryTenant { get; set; }

        // Danh sách người ở
        public List<OccupantDto> Occupants { get; set; } = new();

        // Hóa đơn mới nhất
        public LatestInvoiceDto? LatestInvoice { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class RoomDetailResponse : RoomResponse
    {
        // Lịch sử trạng thái
        public List<RoomStatusHistoryDto> StatusHistory { get; set; } = new();

        // Sự cố / bảo trì
        public List<MaintenanceReportDto> MaintenanceReports { get; set; } = new();
    }

    public class TenantSummaryDto
    {
        public int TenantId { get; set; }
        public string FullName { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public DateOnly? StartDate { get; set; }
    }

    public class OccupantDto
    {
        public int OccupantId { get; set; }
        public int? TenantId { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public bool? IsPrimary { get; set; }
        public DateOnly? MoveInDate { get; set; }
    }

    public class LatestInvoiceDto
    {
        public int InvoiceId { get; set; }
        public string? InvoiceCode { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? RemainingAmount { get; set; }
        public string? Status { get; set; }     // Paid | Overdue | Pending
        public DateTime? CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class RoomStatusHistoryDto
    {
        public int StatusHistoryId { get; set; }
        public string? OldStatus { get; set; }
        public string? NewStatus { get; set; }
        public string? Reason { get; set; }
        public DateTime? ChangedAt { get; set; }
    }

    public class MaintenanceReportDto
    {
        public int ReportId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Priority { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }

    public class RoomStatsResponse
    {
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public int MaintenanceRooms { get; set; }
        public decimal OccupancyRate { get; set; }  // % lấp đầy
    }
}
