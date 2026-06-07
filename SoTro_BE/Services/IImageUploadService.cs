namespace SoTro_BE.Services
{
    public interface IImageUploadService
    {
        Task<string> UploadImageAsync(Stream fileStream, string fileName);
        Task<bool> DeleteImageAsync(string imageUrl);
    }
}
