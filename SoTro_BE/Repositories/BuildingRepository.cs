using Microsoft.EntityFrameworkCore;
using SoTro_BE.Data;
using SoTro_BE.Models;

namespace SoTro_BE.Repositories
{
    public class BuildingRepository : IBuildingRepository
    {
        private readonly SoTroDbContext _context;

        public BuildingRepository(SoTroDbContext context)
        {
            _context = context;
        }

        public async Task<Building?> GetByIdAsync(int buildingId)
        {
            return await _context.Buildings
                .Include(b => b.BuildingType)
                .Include(b => b.Rooms)
                .FirstOrDefaultAsync(b => b.BuildingId == buildingId && b.IsDeleted != true);
        }

        public async Task<List<Building>> GetByLandlordIdAsync(int landlordId)
        {
            return await _context.Buildings
                .Include(b => b.BuildingType)
                .Include(b => b.Rooms)
                .Where(b => b.LandlordId == landlordId && b.IsDeleted != true)
                .OrderByDescending(b => b.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Building> CreateAsync(Building building)
        {
            _context.Buildings.Add(building);
            await _context.SaveChangesAsync();
            
            // Reload with includes
            return (await GetByIdAsync(building.BuildingId))!;
        }

        public async Task<Building> UpdateAsync(Building building)
        {
            _context.Buildings.Update(building);
            await _context.SaveChangesAsync();
            
            return (await GetByIdAsync(building.BuildingId))!;
        }

        public async Task<List<BuildingType>> GetBuildingTypesAsync()
        {
            return await _context.BuildingTypes
                .Where(bt => bt.IsDeleted != true)
                .OrderBy(bt => bt.BuildingTypeId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<BuildingType?> GetBuildingTypeByIdAsync(int buildingTypeId)
        {
            return await _context.BuildingTypes
                .FirstOrDefaultAsync(bt => bt.BuildingTypeId == buildingTypeId && bt.IsDeleted != true);
        }
    }
}
