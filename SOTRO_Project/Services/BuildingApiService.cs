using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
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
                var response = await _httpClient.GetAsync("api/building");
                return await ReadApiResponseAsync<List<BuildingResponse>>(response);
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
                var response = await _httpClient.GetAsync($"api/building/{id}");
                return await ReadApiResponseAsync<BuildingResponse>(response);
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
                return await ReadApiResponseAsync<BuildingResponse>(response);
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
                return await ReadApiResponseAsync<BuildingResponse>(response);
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
                return await ReadApiResponseAsync<bool>(response);
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
                var response = await _httpClient.GetAsync("api/building/types");
                return await ReadApiResponseAsync<List<BuildingTypeResponse>>(response);
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
                var response = await _httpClient.GetAsync($"api/building/suggest-address?query={Uri.EscapeDataString(query)}");
                return await ReadApiResponseAsync<List<string>>(response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<string>> { Success = false, Message = $"Lỗi kết nối: {ex.Message}" };
            }
        }

        private static async Task<ApiResponse<T>> ReadApiResponseAsync<T>(HttpResponseMessage response)
        {
            var body = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(body))
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return new ApiResponse<T>
                    {
                        Success = false,
                        Message = "Phiên đăng nhập đã hết hạn hoặc không hợp lệ. Vui lòng đăng xuất rồi đăng nhập lại."
                    };
                }

                return new ApiResponse<T>
                {
                    Success = false,
                    Message = $"API không trả về nội dung. Status: {(int)response.StatusCode} {response.ReasonPhrase}."
                };
            }

            try
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiResponse != null)
                {
                    if (!response.IsSuccessStatusCode && string.IsNullOrWhiteSpace(apiResponse.Message))
                    {
                        apiResponse.Message = $"API trả về lỗi {(int)response.StatusCode} {response.ReasonPhrase}.";
                    }

                    return apiResponse;
                }
            }
            catch (JsonException)
            {
                var preview = body.Length > 200 ? body[..200] + "..." : body;
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = $"API không trả về JSON hợp lệ. Status: {(int)response.StatusCode} {response.ReasonPhrase}. Nội dung: {preview}"
                };
            }

            return new ApiResponse<T>
            {
                Success = false,
                Message = $"Không đọc được phản hồi từ API. Status: {(int)response.StatusCode} {response.ReasonPhrase}."
            };
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
