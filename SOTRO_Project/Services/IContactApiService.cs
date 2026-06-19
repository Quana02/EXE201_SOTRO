using SOTRO_Project.Models.Auth;
using SOTRO_Project.Models.Contact;

namespace SOTRO_Project.Services
{
    public interface IContactApiService
    {
        Task<ApiResponse<bool>> SendConsultationAsync(ConsultationRequest request);
    }
}
