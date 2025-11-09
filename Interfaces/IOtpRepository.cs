using RegisterApp.Entities;

namespace RegisterApp.Interfaces
{
    public interface IOtpRepository
    {
        Task<OtpTicket?> GetLastValidSentOtp(string phoneNumber, string purpose, CancellationToken cancellationToken);
        Task<bool> UpdateOtp(OtpTicket otp, CancellationToken cancellationToken);
        Task<bool> AddOtp(OtpTicket otp, string purpose, CancellationToken cancellationToken);
    }
}
