namespace SOTRO_Project.Models.Room
{
    // ─────────── RESPONSE ───────────
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

        // Giá
        public decimal? BasePrice { get; set; }
        public decimal? ElectricPrice { get; set; }
        public decimal? WaterPrice { get; set; }
        public decimal? ServiceFee { get; set; }
        public decimal? IncidentFee { get; set; }

        // Sức chứa
        public int? Capacity { get; set; }
        public int? CurrentTenantCount { get; set; }

        // Bill
        public int? BillingDay { get; set; }
        public int? PaymentDueDay { get; set; }
        public string? PaymentStatus { get; set; }

        // Ngân hàng
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankQrUrl { get; set; }

        // Ghi chú
        public string? Note { get; set; }

        // Người thuê đại diện
        public TenantSummaryModel? PrimaryTenant { get; set; }

        // Danh sách người ở
        public List<OccupantModel> Occupants { get; set; } = new();

        // Hóa đơn mới nhất
        public LatestInvoiceModel? LatestInvoice { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Computed
        public string StatusLabel => Status switch
        {
            "Available" => "Trống",
            "Occupied" => "Đang thuê",
            "Maintenance" => "Bảo trì",
            _ => "Không xác định"
        };

        public string StatusColor => Status switch
        {
            "Available" => "outline",
            "Occupied" => "primary",
            "Maintenance" => "tertiary",
            _ => "outline"
        };

        public string PaymentStatusLabel => PaymentStatus switch
        {
            "Paid" => "Đã thanh toán",
            "Overdue" => "Quá hạn",
            "Pending" => "Chờ thanh toán",
            _ => "—"
        };

        public string DisplayName => !string.IsNullOrEmpty(RoomName)
            ? $"{RoomCode} - {RoomName}"
            : RoomCode;
    }

    public class RoomDetailResponse : RoomResponse
    {
        public List<RoomStatusHistoryModel> StatusHistory { get; set; } = new();
        public List<MaintenanceReportModel> MaintenanceReports { get; set; } = new();
    }

    public class TenantSummaryModel
    {
        public int TenantId { get; set; }
        public string FullName { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public DateOnly? StartDate { get; set; }
    }

    public class OccupantModel
    {
        public int OccupantId { get; set; }
        public int? TenantId { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public bool? IsPrimary { get; set; }
        public DateOnly? MoveInDate { get; set; }
    }

    public class LatestInvoiceModel
    {
        public int InvoiceId { get; set; }
        public string? InvoiceCode { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? RemainingAmount { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class RoomStatusHistoryModel
    {
        public int StatusHistoryId { get; set; }
        public string? OldStatus { get; set; }
        public string? NewStatus { get; set; }
        public string? Reason { get; set; }
        public DateTime? ChangedAt { get; set; }
    }

    public class MaintenanceReportModel
    {
        public int ReportId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Priority { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }

    public class RoomStatsModel
    {
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public int MaintenanceRooms { get; set; }
        public decimal OccupancyRate { get; set; }
    }

    // ─────────── REQUEST ───────────
    public class CreateRoomRequest
    {
        public string RoomCode { get; set; } = string.Empty;
        public string? RoomName { get; set; }
        public string? Zone { get; set; }
        public int? FloorNumber { get; set; }
        public int? RoomTypeId { get; set; }
        public string Status { get; set; } = "Available";

        public decimal? BasePrice { get; set; }
        public decimal? ElectricPrice { get; set; }
        public decimal? WaterPrice { get; set; }
        public decimal? ServiceFee { get; set; }
        public decimal? IncidentFee { get; set; }

        public int? Capacity { get; set; }
        public int? CurrentTenantCount { get; set; }
        public int? BillingDay { get; set; }
        public int? PaymentDueDay { get; set; }

        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankQrUrl { get; set; }
        public string? Note { get; set; }
    }

    public class UpdateRoomRequest
    {
        public string RoomCode { get; set; } = string.Empty;
        public string? RoomName { get; set; }
        public string? Zone { get; set; }
        public int? FloorNumber { get; set; }
        public int? RoomTypeId { get; set; }
        public string? Status { get; set; }

        public decimal? BasePrice { get; set; }
        public decimal? ElectricPrice { get; set; }
        public decimal? WaterPrice { get; set; }
        public decimal? ServiceFee { get; set; }
        public decimal? IncidentFee { get; set; }

        public int? Capacity { get; set; }
        public int? CurrentTenantCount { get; set; }
        public int? BillingDay { get; set; }
        public int? PaymentDueDay { get; set; }

        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankQrUrl { get; set; }
        public string? Note { get; set; }
    }

    public class ChangeRoomStatusRequest
    {
        public string NewStatus { get; set; } = null!;
        public string? Reason { get; set; }
    }
}
