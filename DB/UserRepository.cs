using Microsoft.EntityFrameworkCore;
using RegisterApp.Entities;
using RegisterApp.Interfaces;
using System.Numerics;
using System.Threading;

namespace RegisterApp.DB
{
    public class UserRepository : IUserRepository
    {
        private readonly AuthDbContext db;

        public UserRepository(AuthDbContext authDbContext)
        {
            this.db=authDbContext;
        }
        public async Task<bool> AddUser(AppUser user, CancellationToken cancellationToken)
        {
            db.Users.Add(user);
           return await db.SaveChangesAsync(cancellationToken)>0;
        }

        public async Task<AppUser?> GetUser(string phoneNumber, CancellationToken cancellationToken)
        {
            return await db.Users.FirstOrDefaultAsync(u => u.PhoneE164 == phoneNumber, cancellationToken);
        }

        public async Task<bool> UpdateUser(AppUser user, CancellationToken cancellationToken)
        {
            var old = await GetUser(user.PhoneE164, cancellationToken);
            if (old != null)
            {
                old.PhoneVerified = user.PhoneVerified;
                old.LastLoginAt = user.LastLoginAt;
                old.DisplayName = user.DisplayName;
                return await db.SaveChangesAsync(cancellationToken) > 0;
            }
            return false;
        }

        public async Task<bool> UserExist(string phoneNumber, CancellationToken cancellationToken)
        {
            return await db.Users.AnyAsync(u => u.PhoneE164 == phoneNumber, cancellationToken);
        }
    }
}
