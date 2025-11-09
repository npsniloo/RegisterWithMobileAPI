using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace RegisterApp.Entities
{
    [Index(nameof(PhoneE164), IsUnique = true)]
    public class AppUser
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(64)]
        public string? DisplayName { get; set; }

        // E.164 normalized, e.g., +14155550123
        [MaxLength(20)]
        public string PhoneE164 { get; set; } = default!;

        public bool PhoneVerified { get; set; } = false;

        // For auditing/abuse prevention:
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? LastLoginAt { get; set; }
    }
}
