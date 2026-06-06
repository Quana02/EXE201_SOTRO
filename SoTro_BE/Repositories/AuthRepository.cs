using Microsoft.EntityFrameworkCore;
using SoTro_BE.Data;
using SoTro_BE.Models;

namespace SoTro_BE.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly SoTroDbContext _context;

        public AuthRepository(SoTroDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var normalizedEmail = email.Trim().ToLower();

            return await _context.Users
                .Include(user => user.Role)
                .FirstOrDefaultAsync(user => user.Email.ToLower() == normalizedEmail);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            var normalizedEmail = email.Trim().ToLower();
            return await _context.Users.AnyAsync(user => user.Email.ToLower() == normalizedEmail);
        }
    }
}
