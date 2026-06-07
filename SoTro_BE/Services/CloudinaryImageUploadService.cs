using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace SoTro_BE.Services
{
    public class CloudinaryImageUploadService : IImageUploadService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryImageUploadService(IConfiguration configuration)
        {
            var cloudName = configuration["Cloudinary:CloudName"];
            var apiKey = configuration["Cloudinary:ApiKey"];
            var apiSecret = configuration["Cloudinary:ApiSecret"];

            if (string.IsNullOrWhiteSpace(cloudName) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiSecret))
            {
                throw new InvalidOperationException("Cloudinary configuration is missing. Please set Cloudinary:CloudName, Cloudinary:ApiKey, and Cloudinary:ApiSecret in appsettings.json");
            }

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }

        public async Task<string> UploadImageAsync(Stream fileStream, string fileName)
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                Folder = "sotro/buildings",
                Transformation = new Transformation()
                    .Quality("auto")
                    .FetchFormat("auto")
                    .Width(1200)
                    .Height(800)
                    .Crop("limit")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                throw new Exception($"Tải lên ảnh thất bại: {uploadResult.Error.Message}");
            }

            return uploadResult.SecureUrl.ToString();
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl)) return false;

            try
            {
                // Extract public ID from URL
                var uri = new Uri(imageUrl);
                var path = uri.AbsolutePath;
                // Path format: /v12345/sotro/buildings/filename.ext
                var startIndex = path.IndexOf("sotro/");
                if (startIndex < 0) return false;

                var publicIdWithExt = path[startIndex..];
                var publicId = Path.GetFileNameWithoutExtension(publicIdWithExt) != null
                    ? publicIdWithExt[..publicIdWithExt.LastIndexOf('.')]
                    : publicIdWithExt;

                var deleteParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deleteParams);
                return result.Result == "ok";
            }
            catch
            {
                return false;
            }
        }
    }
}
