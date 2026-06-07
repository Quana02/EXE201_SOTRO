namespace SoTro_BE.DTOs.Building
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
}
