using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.JSInterop;
using SOTRO_Project.Models.Auth;
using SOTRO_Project.Models.Room;

namespace SOTRO_Project.Services
{
    public class RoomApiService : IRoomApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        public RoomApiService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        private async Task SetAuthorizationHeaderAsync()
        {
            try
            {
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "sotro_token");
                _httpClient.DefaultRequestHeaders.Authorization = !string.IsNullOrWhiteSpace(token)
                    ? new AuthenticationHeaderValue("Bearer", token)
                    : null;
            }
            catch
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<ApiResponse<List<RoomResponse>>> GetRoomsByBuildingAsync(int buildingId)
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<RoomResponse>>>($"api/rooms?buildingId={buildingId}");
                return response ?? Fail<List<RoomResponse>>("Không nhận được phản hồi từ máy chủ.");
            }
            catch (Exception ex)
            {
                return Fail<List<RoomResponse>>($"Lỗi kết nối: {ex.Message}");
            }
        }

        public async Task<ApiResponse<RoomDetailResponse>> GetRoomByIdAsync(int roomId)
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<RoomDetailResponse>>($"api/rooms/{roomId}");
                return response ?? Fail<RoomDetailResponse>("Không nhận được phản hồi từ máy chủ.");
            }
            catch (Exception ex)
            {
                return Fail<RoomDetailResponse>($"Lỗi kết nối: {ex.Message}");
            }
        }

        public async Task<ApiResponse<RoomStatsModel>> GetRoomStatsAsync(int buildingId)
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<RoomStatsModel>>($"api/rooms/stats?buildingId={buildingId}");
                return response ?? Fail<RoomStatsModel>("Không nhận được phản hồi từ máy chủ.");
            }
            catch (Exception ex)
            {
                return Fail<RoomStatsModel>($"Lỗi kết nối: {ex.Message}");
            }
        }

        public async Task<ApiResponse<RoomResponse>> CreateRoomAsync(int buildingId, CreateRoomRequest request)
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/rooms?buildingId={buildingId}", request);
                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<RoomResponse>>();
                return apiResponse ?? Fail<RoomResponse>("Không đọc được phản hồi từ API.");
            }
            catch (Exception ex)
            {
                return Fail<RoomResponse>($"Lỗi: {ex.Message}");
            }
        }

        public async Task<ApiResponse<RoomResponse>> UpdateRoomAsync(int roomId, UpdateRoomRequest request)
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/rooms/{roomId}", request);
                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<RoomResponse>>();
                return apiResponse ?? Fail<RoomResponse>("Không đọc được phản hồi từ API.");
            }
            catch (Exception ex)
            {
                return Fail<RoomResponse>($"Lỗi: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteRoomAsync(int roomId)
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                var response = await _httpClient.DeleteAsync($"api/rooms/{roomId}");
                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                return apiResponse ?? Fail<bool>("Không đọc được phản hồi từ API.");
            }
            catch (Exception ex)
            {
                return Fail<bool>($"Lỗi: {ex.Message}");
            }
        }

        public async Task<ApiResponse<RoomResponse>> ChangeRoomStatusAsync(int roomId, ChangeRoomStatusRequest request)
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                var response = await _httpClient.PatchAsJsonAsync($"api/rooms/{roomId}/status", request);
                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<RoomResponse>>();
                return apiResponse ?? Fail<RoomResponse>("Không đọc được phản hồi từ API.");
            }
            catch (Exception ex)
            {
                return Fail<RoomResponse>($"Lỗi: {ex.Message}");
            }
        }

        private static ApiResponse<T> Fail<T>(string message) =>
            new ApiResponse<T> { Success = false, Message = message };
    }
}
