using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using SOTRO_Project.Models.Auth;
using SOTRO_Project.Models.Building;

namespace SOTRO_Project.Services
{
    public class BuildingApiService : IBuildingApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        public BuildingApiService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        private async Task SetAuthorizationHeaderAsync()
        {
            try
            {
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "sotro_token");
                if (!string.IsNullOrWhiteSpace(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                else
                {
                    _httpClient.DefaultRequestHeaders.Authorization = null;
                }
            }
            catch (Exception)
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<ApiResponse<List<BuildingResponse>>> GetBuildingsAsync()
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<BuildingResponse>>>("api/building");
                return response ?? new ApiResponse<List<BuildingResponse>> { Success = false, Message = "Không nhận được phản hồi từ máy chủ." };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<BuildingResponse>> { Success = false, Message = $"Lỗi kết nối: {ex.Message}" };
            }
        }

        public async Task<ApiResponse<BuildingResponse>> GetBuildingByIdAsync(int id)
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<BuildingResponse>>($"api/building/{id}");
                return response ?? new ApiResponse<BuildingResponse> { Success = false, Message = "Không nhận được phản hồi từ máy chủ." };
            }
            catch (Exception ex)
            {
                return new ApiResponse<BuildingResponse> { Success = false, Message = $"Lỗi kết nối: {ex.Message}" };
            }
        }

        public async Task<ApiResponse<BuildingResponse>> CreateBuildingAsync(CreateBuildingRequest request, List<(Stream Stream, string FileName)> imageFiles)
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                using var content = new MultipartFormDataContent();
                
                content.Add(new StringContent(request.BuildingName), nameof(request.BuildingName));
                content.Add(new StringContent(request.BuildingTypeId.ToString()), nameof(request.BuildingTypeId));
                content.Add(new StringContent(request.Address), nameof(request.Address));
                
                if (request.Description != null)
                    content.Add(new StringContent(request.Description), nameof(request.Description));
                
                if (request.TotalFloors.HasValue)
                    content.Add(new StringContent(request.TotalFloors.Value.ToString()), nameof(request.TotalFloors));
                
                if (request.BillingDay.HasValue)
                    content.Add(new StringContent(request.BillingDay.Value.ToString()), nameof(request.BillingDay));
                
                if (request.DueDay.HasValue)
                    content.Add(new StringContent(request.DueDay.Value.ToString()), nameof(request.DueDay));

                foreach (var (stream, fileName) in imageFiles)
                {
                    var streamContent = new StreamContent(stream);
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue(GetMimeType(fileName));
                    content.Add(streamContent, "images", fileName);
                }

                var response = await _httpClient.PostAsync("api/building", content);
                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<BuildingResponse>>();
                return apiResponse ?? new ApiResponse<BuildingResponse> { Success = false, Message = "Không đọc được phản hồi từ API." };
            }
            catch (Exception ex)
            {
                return new ApiResponse<BuildingResponse> { Success = false, Message = $"Lỗi: {ex.Message}" };
            }
        }

        public async Task<ApiResponse<BuildingResponse>> UpdateBuildingAsync(int id, CreateBuildingRequest request, List<(Stream Stream, string FileName)>? imageFiles)
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                using var content = new MultipartFormDataContent();
                
                content.Add(new StringContent(request.BuildingName), nameof(request.BuildingName));
                content.Add(new StringContent(request.BuildingTypeId.ToString()), nameof(request.BuildingTypeId));
                content.Add(new StringContent(request.Address), nameof(request.Address));
                
                if (request.Description != null)
                    content.Add(new StringContent(request.Description), nameof(request.Description));
                
                if (request.TotalFloors.HasValue)
                    content.Add(new StringContent(request.TotalFloors.Value.ToString()), nameof(request.TotalFloors));
                
                if (request.BillingDay.HasValue)
                    content.Add(new StringContent(request.BillingDay.Value.ToString()), nameof(request.BillingDay));
                
                if (request.DueDay.HasValue)
                    content.Add(new StringContent(request.DueDay.Value.ToString()), nameof(request.DueDay));

                if (imageFiles != null)
                {
                    foreach (var (stream, fileName) in imageFiles)
                    {
                        var streamContent = new StreamContent(stream);
                        streamContent.Headers.ContentType = new MediaTypeHeaderValue(GetMimeType(fileName));
                        content.Add(streamContent, "images", fileName);
                    }
                }

                var response = await _httpClient.PutAsync($"api/building/{id}", content);
                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<BuildingResponse>>();
                return apiResponse ?? new ApiResponse<BuildingResponse> { Success = false, Message = "Không đọc được phản hồi từ API." };
            }
            catch (Exception ex)
            {
                return new ApiResponse<BuildingResponse> { Success = false, Message = $"Lỗi: {ex.Message}" };
            }
        }

        public async Task<ApiResponse<bool>> DeleteBuildingAsync(int id)
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                var response = await _httpClient.DeleteAsync($"api/building/{id}");
                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                return apiResponse ?? new ApiResponse<bool> { Success = false, Message = "Không đọc được phản hồi từ API." };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = $"Lỗi: {ex.Message}" };
            }
        }

        public async Task<ApiResponse<List<BuildingTypeResponse>>> GetBuildingTypesAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<BuildingTypeResponse>>>("api/building/types");
                return response ?? new ApiResponse<List<BuildingTypeResponse>> { Success = false, Message = "Không nhận được phản hồi từ máy chủ." };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<BuildingTypeResponse>> { Success = false, Message = $"Lỗi kết nối: {ex.Message}" };
            }
        }

        public async Task<ApiResponse<List<string>>> SuggestAddressAsync(string query)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<string>>>($"api/building/suggest-address?query={Uri.EscapeDataString(query)}");
                return response ?? new ApiResponse<List<string>> { Success = false, Message = "Không nhận được phản hồi từ máy chủ." };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<string>> { Success = false, Message = $"Lỗi kết nối: {ex.Message}" };
            }
        }
        private static string GetMimeType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ext switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }
    }
}
