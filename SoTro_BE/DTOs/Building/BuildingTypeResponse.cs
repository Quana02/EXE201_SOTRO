namespace SoTro_BE.DTOs.Building
{
    public class BuildingTypeResponse
    {
        public int BuildingTypeId { get; set; }
        public string TypeName { get; set; } = null!;
        public string? Description { get; set; }
    }
}
