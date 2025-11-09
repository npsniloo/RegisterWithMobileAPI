using Microsoft.EntityFrameworkCore;
using RegisterApp.Entities;
using RegisterApp.Interfaces;

namespace RegisterApp.DB
{
    public class OtpRepository : IOtpRepository
    {
        private readonly AuthDbContext db;

        public OtpRepository(AuthDbContext authDbContext)
        {
            this.db = authDbContext;
        }
        public async Task<bool> AddOtp(OtpTicket otp, string purpose, CancellationToken cancellationToken)
        {
           db.OtpTickets.Add(otp);
           return await db.SaveChangesAsync() > 0;
        }

        public async Task<OtpTicket?> GetLastValidSentOtp(string phoneNumber, string purpose, CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;
            return await db.OtpTickets
                 .Where(t => t.PhoneE164 == phoneNumber && !t.Consumed && t.Purpose == purpose && t.ExpiresAt > now)
                 .OrderByDescending(t => t.CreatedAt)
                 .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> UpdateOtp(OtpTicket otp, CancellationToken cancellationToken)
        {
            var old = await db.OtpTickets.FirstOrDefaultAsync(o=> o.Id == otp.Id, cancellationToken);
            if (old != null)
            {
                old.AttemptCount = otp.AttemptCount;
                old.OtpHash = otp.OtpHash;
                old.Consumed = otp.Consumed;
                return await db.SaveChangesAsync(cancellationToken) > 0;
            }
            return false;
        }
    }
}
