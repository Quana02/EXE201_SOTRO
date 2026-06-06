using Microsoft.EntityFrameworkCore;
using SoTro_BE.Data;
using SoTro_BE.Models;

namespace SoTro_BE.Repositories
{
    public class OtpRepository : IOtpRepository
    {
        private readonly SoTroDbContext _context;

        public OtpRepository(SoTroDbContext context)
        {
            _context = context;
        }

        public async Task SaveOtpAsync(OtpVerification otp)
        {
            _context.OtpVerifications.Add(otp);
            await _context.SaveChangesAsync();
        }

        public async Task<OtpVerification?> GetValidOtpAsync(string email, string otp, string purpose)
        {
            var normalizedEmail = NormalizeEmail(email);

            return await _context.OtpVerifications
                .Where(item => item.Email.ToLower() == normalizedEmail &&
                               item.OtpCode == otp &&
                               item.Purpose == purpose &&
                               !item.IsUsed &&
                               item.ExpiredAt >= DateTime.UtcNow)
                .OrderByDescending(item => item.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<OtpVerification?> GetValidResetTokenAsync(string email, string resetPasswordToken)
        {
            var normalizedEmail = NormalizeEmail(email);

            return await _context.OtpVerifications
                .Where(item => item.Email.ToLower() == normalizedEmail &&
                               item.Purpose == "ForgotPassword" &&
                               item.ResetPasswordToken == resetPasswordToken &&
                               item.ResetPasswordTokenExpiry != null &&
                               item.ResetPasswordTokenExpiry >= DateTime.UtcNow &&
                               !item.IsUsed)
                .OrderByDescending(item => item.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task MarkOtpAsUsedAsync(OtpVerification otp)
        {
            otp.IsUsed = true;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateOtpAsync(OtpVerification otp)
        {
            _context.OtpVerifications.Update(otp);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveOldOtpsAsync(string email, string purpose)
        {
            var normalizedEmail = NormalizeEmail(email);

            var oldOtps = await _context.OtpVerifications
                .Where(item => item.Email.ToLower() == normalizedEmail &&
                               item.Purpose == purpose &&
                               !item.IsUsed)
                .ToListAsync();

            foreach (var otp in oldOtps)
            {
                otp.IsUsed = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<PendingRegistration?> GetPendingRegistrationByEmailAsync(string email)
        {
            var normalizedEmail = NormalizeEmail(email);

            return await _context.PendingRegistrations
                .FirstOrDefaultAsync(item => item.Email.ToLower() == normalizedEmail);
        }

        public async Task SavePendingRegistrationAsync(PendingRegistration pendingRegistration)
        {
            _context.PendingRegistrations.Add(pendingRegistration);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePendingRegistrationAsync(PendingRegistration pendingRegistration)
        {
            _context.PendingRegistrations.Update(pendingRegistration);
            await _context.SaveChangesAsync();
        }

        private static string NormalizeEmail(string email)
        {
            return email.Trim().ToLowerInvariant();
        }
    }
}
