using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using SoTro_BE.DTOs.Auth;
using SoTro_BE.DTOs.Building;
using SoTro_BE.Models;
using SoTro_BE.Repositories;

namespace SoTro_BE.Services
{
    public class BuildingService : IBuildingService
    {
        private readonly IBuildingRepository _buildingRepository;
        private readonly IImageUploadService _imageUploadService;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public BuildingService(IBuildingRepository buildingRepository, IImageUploadService imageUploadService, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _buildingRepository = buildingRepository;
            _imageUploadService = imageUploadService;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ApiResponse<BuildingResponse>> CreateBuildingAsync(int landlordId, CreateBuildingRequest request, List<(Stream Stream, string FileName)> imageFiles)
        {
            try
            {
                // Validate building type exists
                var buildingType = await _buildingRepository.GetBuildingTypeByIdAsync(request.BuildingTypeId);
                if (buildingType == null)
                {
                    return new ApiResponse<BuildingResponse>
                    {
                        Success = false,
                        Message = "Loại nhà trọ không tồn tại."
                    };
                }

                // Upload first image as main ImageUrl for backward compatibility
                string? mainImageUrl = null;
                var buildingImages = new List<BuildingImage>();

                foreach (var (stream, fileName) in imageFiles)
                {
                    var uploadedUrl = await _imageUploadService.UploadImageAsync(stream, fileName);
                    if (mainImageUrl == null)
                    {
                        mainImageUrl = uploadedUrl;
                    }
                    buildingImages.Add(new BuildingImage
                    {
                        ImageUrl = uploadedUrl,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                var building = new Building
                {
                    LandlordId = landlordId,
                    BuildingTypeId = request.BuildingTypeId,
                    BuildingName = request.BuildingName,
                    Address = request.Address,
                    Description = request.Description,
                    TotalFloors = request.TotalFloors,
                    TotalRooms = 0,
                    BillingDay = request.BillingDay,
                    DueDay = request.DueDay,
                    ImageUrl = mainImageUrl,
                    Status = "Active",
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    BuildingImages = buildingImages
                };

                var created = await _buildingRepository.CreateAsync(building);

                return new ApiResponse<BuildingResponse>
                {
                    Success = true,
                    Message = "Tạo nhà trọ thành công!",
                    Data = MapToResponse(created)
                };
            }
            catch (Exception ex)
            {
                var details = ex.InnerException != null ? $"{ex.Message} (Inner: {ex.InnerException.Message})" : ex.Message;
                return new ApiResponse<BuildingResponse>
                {
                    Success = false,
                    Message = $"Lỗi khi tạo nhà trọ: {details}"
                };
            }
        }

        public async Task<ApiResponse<List<BuildingResponse>>> GetBuildingsByLandlordAsync(int landlordId)
        {
            try
            {
                var buildings = await _buildingRepository.GetByLandlordIdAsync(landlordId);
                var responses = buildings.Select(MapToResponse).ToList();

                return new ApiResponse<List<BuildingResponse>>
                {
                    Success = true,
                    Data = responses
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<BuildingResponse>>
                {
                    Success = false,
                    Message = $"Lỗi khi lấy danh sách nhà trọ: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<BuildingResponse>> GetBuildingByIdAsync(int buildingId, int landlordId)
        {
            try
            {
                var building = await _buildingRepository.GetByIdAsync(buildingId);
                if (building == null || building.LandlordId != landlordId)
                {
                    return new ApiResponse<BuildingResponse>
                    {
                        Success = false,
                        Message = "Không tìm thấy nhà trọ."
                    };
                }

                return new ApiResponse<BuildingResponse>
                {
                    Success = true,
                    Data = MapToResponse(building)
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<BuildingResponse>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<BuildingResponse>> UpdateBuildingAsync(int buildingId, int landlordId, CreateBuildingRequest request, List<(Stream Stream, string FileName)>? imageFiles)
        {
            try
            {
                var building = await _buildingRepository.GetByIdAsync(buildingId);
                if (building == null || building.LandlordId != landlordId)
                {
                    return new ApiResponse<BuildingResponse>
                    {
                        Success = false,
                        Message = "Không tìm thấy nhà trọ."
                    };
                }

                var buildingType = await _buildingRepository.GetBuildingTypeByIdAsync(request.BuildingTypeId);
                if (buildingType == null)
                {
                    return new ApiResponse<BuildingResponse>
                    {
                        Success = false,
                        Message = "Loại nhà trọ không tồn tại."
                    };
                }

                // Upload new images if provided
                if (imageFiles != null && imageFiles.Count > 0)
                {
                    // Delete old main image
                    if (!string.IsNullOrWhiteSpace(building.ImageUrl))
                    {
                        await _imageUploadService.DeleteImageAsync(building.ImageUrl);
                    }

                    string? mainImageUrl = null;
                    foreach (var (stream, fileName) in imageFiles)
                    {
                        var uploadedUrl = await _imageUploadService.UploadImageAsync(stream, fileName);
                        if (mainImageUrl == null)
                        {
                            mainImageUrl = uploadedUrl;
                        }
                        building.BuildingImages.Add(new BuildingImage
                        {
                            ImageUrl = uploadedUrl,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                    building.ImageUrl = mainImageUrl;
                }

                building.BuildingTypeId = request.BuildingTypeId;
                building.BuildingName = request.BuildingName;
                building.Address = request.Address;
                building.Description = request.Description;
                building.TotalFloors = request.TotalFloors;
                building.BillingDay = request.BillingDay;
                building.DueDay = request.DueDay;
                building.UpdatedAt = DateTime.UtcNow;

                var updated = await _buildingRepository.UpdateAsync(building);

                return new ApiResponse<BuildingResponse>
                {
                    Success = true,
                    Message = "Cập nhật nhà trọ thành công!",
                    Data = MapToResponse(updated)
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<BuildingResponse>
                {
                    Success = false,
                    Message = $"Lỗi khi cập nhật: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteBuildingAsync(int buildingId, int landlordId)
        {
            try
            {
                var building = await _buildingRepository.GetByIdAsync(buildingId);
                if (building == null || building.LandlordId != landlordId)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Không tìm thấy nhà trọ."
                    };
                }

                building.IsDeleted = true;
                building.DeletedAt = DateTime.UtcNow;
                building.UpdatedAt = DateTime.UtcNow;

                await _buildingRepository.UpdateAsync(building);

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Xóa nhà trọ thành công!",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Lỗi khi xóa: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<List<BuildingTypeResponse>>> GetBuildingTypesAsync()
        {
            try
            {
                var types = await _buildingRepository.GetBuildingTypesAsync();
                var responses = types.Select(t => new BuildingTypeResponse
                {
                    BuildingTypeId = t.BuildingTypeId,
                    TypeName = t.TypeName,
                    Description = t.Description
                }).ToList();

                return new ApiResponse<List<BuildingTypeResponse>>
                {
                    Success = true,
                    Data = responses
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<BuildingTypeResponse>>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        private static BuildingResponse MapToResponse(Building building)
        {
            var totalRooms = building.Rooms?.Count(r => r.IsDeleted != true) ?? 0;
            var occupiedRooms = building.Rooms?.Count(r => r.IsDeleted != true && r.Status == "Occupied") ?? 0;

            return new BuildingResponse
            {
                BuildingId = building.BuildingId,
                BuildingName = building.BuildingName,
                BuildingTypeId = building.BuildingTypeId,
                BuildingTypeName = building.BuildingType?.TypeName,
                Address = building.Address,
                ImageUrl = building.ImageUrl,
                Description = building.Description,
                TotalFloors = building.TotalFloors,
                TotalRooms = totalRooms,
                OccupiedRooms = occupiedRooms,
                BillingDay = building.BillingDay,
                DueDay = building.DueDay,
                Status = building.Status,
                CreatedAt = building.CreatedAt
            };
        }

        public async Task<ApiResponse<List<string>>> SuggestAddressAsync(string query)
        {
            try
            {
                var goongKey = _configuration["Goong:ApiKey"];
                var mapboxToken = _configuration["Mapbox:AccessToken"];

                // 1. Try Goong Maps
                if (!string.IsNullOrWhiteSpace(goongKey))
                {
                    try
                    {
                        var client = _httpClientFactory.CreateClient();
                        var url = $"https://rsapi.goong.io/Place/AutoComplete?api_key={goongKey}&input={Uri.EscapeDataString(query)}&limit=5";
                        var response = await client.GetAsync(url);
                        if (response.IsSuccessStatusCode)
                        {
                            var data = await response.Content.ReadFromJsonAsync<GoongAutocompleteResponse>();
                            if (data?.Predictions != null)
                            {
                                var suggestions = data.Predictions.Select(p => p.Description).ToList();
                                return new ApiResponse<List<string>> { Success = true, Data = suggestions };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error calling Goong API: {ex.Message}");
                    }
                }

                // 2. Try Mapbox
                if (!string.IsNullOrWhiteSpace(mapboxToken))
                {
                    try
                    {
                        var client = _httpClientFactory.CreateClient();
                        var url = $"https://api.mapbox.com/geocoding/v5/mapbox.places/{Uri.EscapeDataString(query)}.json?access_token={mapboxToken}&country=vn&limit=5&language=vi";
                        var response = await client.GetAsync(url);
                        if (response.IsSuccessStatusCode)
                        {
                            var data = await response.Content.ReadFromJsonAsync<MapboxGeocodingResponse>();
                            if (data?.Features != null)
                            {
                                var suggestions = data.Features.Select(f => f.Place_Name).ToList();
                                return new ApiResponse<List<string>> { Success = true, Data = suggestions };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error calling Mapbox API: {ex.Message}");
                    }
                }

                // 3. Fallback to OpenStreetMap Nominatim
                try
                {
                    var client = _httpClientFactory.CreateClient();
                    client.DefaultRequestHeaders.Add("User-Agent", "SoTroApp/1.0");
                    var url = $"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(query)}&countrycodes=vn&limit=5";
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadFromJsonAsync<List<OsmNominatimResponse>>();
                        if (data != null)
                        {
                            var suggestions = data.Select(d => d.Display_Name).ToList();
                            return new ApiResponse<List<string>> { Success = true, Data = suggestions };
                        }
                    }
                }
                catch (Exception ex)
                {
                    return new ApiResponse<List<string>>
                    {
                        Success = false,
                        Message = $"Lỗi tìm gợi ý địa chỉ: {ex.Message}"
                    };
                }

                return new ApiResponse<List<string>> { Success = true, Data = new List<string>() };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<string>>
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }
    }

    public class GoongAutocompleteResponse
    {
        public List<GoongPrediction>? Predictions { get; set; }
    }

    public class GoongPrediction
    {
        public string Description { get; set; } = null!;
    }

    public class MapboxGeocodingResponse
    {
        public List<MapboxFeature>? Features { get; set; }
    }

    public class MapboxFeature
    {
        public string Place_Name { get; set; } = null!;
    }

    public class OsmNominatimResponse
    {
        public string Display_Name { get; set; } = null!;
    }
}
