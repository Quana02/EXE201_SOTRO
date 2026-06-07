namespace SoTro_BE.Models.DTOs
{
    public class RoomDto
    {
        public int RoomId { get; set; }
        public int? BuildingId { get; set; }
        public int? RoomTypeId { get; set; }
        public string RoomCode { get; set; } = null!;
        public int? FloorNumber { get; set; }
        public decimal? Area { get; set; }
        public decimal? BasePrice { get; set; }
        public decimal? ElectricPrice { get; set; }
        public decimal? WaterPrice { get; set; }
        public int? Capacity { get; set; }
        public int? CurrentTenantCount { get; set; }
        public string? Status { get; set; } // Available, Occupied, Maintenance
        public string? Note { get; set; }
    }

    public class CreateRoomDto
    {
        public int BuildingId { get; set; }
        public int? RoomTypeId { get; set; }
        public string RoomCode { get; set; } = null!;
        public int? FloorNumber { get; set; }
        public decimal? Area { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? ElectricPrice { get; set; }
        public decimal? WaterPrice { get; set; }
        public int? Capacity { get; set; }
        public string? Note { get; set; }
    }

    public class UpdateRoomDto
    {
        public int RoomId { get; set; }
        public string RoomCode { get; set; } = null!;
        public int? FloorNumber { get; set; }
        public decimal? Area { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? ElectricPrice { get; set; }
        public decimal? WaterPrice { get; set; }
        public int? Capacity { get; set; }
        public int? RoomTypeId { get; set; }
        public string? Note { get; set; }
    }

    public class RoomDetailDto
    {
        public int RoomId { get; set; }
        public string RoomCode { get; set; } = null!;
        public int? FloorNumber { get; set; }
        public decimal? Area { get; set; }
        public decimal? BasePrice { get; set; }
        public decimal? ElectricPrice { get; set; }
        public decimal? WaterPrice { get; set; }
        public int? Capacity { get; set; }
        public int? CurrentTenantCount { get; set; }
        public string? Status { get; set; }
        public string? Note { get; set; }
        public string? RoomTypeName { get; set; }
        public List<TenantInfoDto> CurrentTenants { get; set; } = new();
        public BillingInfoDto? LatestBilling { get; set; }
        public PaymentInfoDto? LatestPayment { get; set; }
    }

    public class TenantInfoDto
    {
        public int TenantId { get; set; }
        public string FullName { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
    }

    public class BillingInfoDto
    {
        public int InvoiceId { get; set; }
        public string? InvoiceCode { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal? RoomPrice { get; set; }
        public decimal? ElectricCost { get; set; }
        public decimal? WaterCost { get; set; }
        public decimal? ServiceCost { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? RemainingAmount { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class PaymentInfoDto
    {
        public int PaymentId { get; set; }
        public decimal? Amount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TransactionCode { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}