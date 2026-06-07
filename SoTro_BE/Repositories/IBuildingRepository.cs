using SoTro_BE.Models;

namespace SoTro_BE.Repositories
{
    public interface IBuildingRepository
    {
        Task<Building?> GetByIdAsync(int buildingId);
        Task<List<Building>> GetByLandlordIdAsync(int landlordId);
        Task<Building> CreateAsync(Building building);
        Task<Building> UpdateAsync(Building building);
        Task<List<BuildingType>> GetBuildingTypesAsync();
        Task<BuildingType?> GetBuildingTypeByIdAsync(int buildingTypeId);
    }
}
