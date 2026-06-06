using SoTro_BE.Models;

namespace SoTro_BE.Repositories
{
    public interface IOtpRepository
    {
        Task SaveOtpAsync(OtpVerification otp);
        Task<OtpVerification?> GetValidOtpAsync(string email, string otp, string purpose);
        Task<OtpVerification?> GetValidResetTokenAsync(string email, string resetPasswordToken);
        Task UpdateOtpAsync(OtpVerification otp);
        Task MarkOtpAsUsedAsync(OtpVerification otp);
        Task RemoveOldOtpsAsync(string email, string purpose);
        Task<PendingRegistration?> GetPendingRegistrationByEmailAsync(string email);
        Task SavePendingRegistrationAsync(PendingRegistration pendingRegistration);
        Task UpdatePendingRegistrationAsync(PendingRegistration pendingRegistration);
    }
}
