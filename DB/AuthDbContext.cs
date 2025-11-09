using Microsoft.EntityFrameworkCore;
using RegisterApp.Entities;

namespace RegisterApp.DB
{
    public class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
    {
        public DbSet<AppUser> Users => Set<AppUser>();
        public DbSet<OtpTicket> OtpTickets => Set<OtpTicket>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);
            b.Entity<AppUser>().Property(p => p.PhoneE164).IsRequired();
            b.Entity<OtpTicket>().Property(p => p.PhoneE164).IsRequired();
            b.Entity<OtpTicket>().HasIndex(p => new { p.PhoneE164, p.Purpose, p.Consumed });
        }
    }
}
