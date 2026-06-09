namespace SOTRO_Project.Models.Auth
{
    public class LinkGoogleRequest
    {
        public string CurrentEmail { get; set; } = string.Empty;
        public string GoogleEmail { get; set; } = string.Empty;
        public string GoogleId { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
    }
}
