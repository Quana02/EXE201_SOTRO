using SoTro_BE.Models;

namespace SoTro_BE.Repositories
{
    public interface IAuthRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<User> CreateUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task<bool> EmailExistsAsync(string email);
    }
}
