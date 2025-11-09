using System.ComponentModel.DataAnnotations;

namespace RegisterApp.Entities
{
    public class OtpTicket
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Link to user (can be null during pre-registration)
        public Guid? UserId { get; set; }
        public AppUser? User { get; set; }

        [MaxLength(20)]
        public string PhoneE164 { get; set; } = default!;

        // Store only a hash of the OTP (never the raw code)
        [MaxLength(256)]
        public string OtpHash { get; set; } = default!;

        public DateTimeOffset ExpiresAt { get; set; }
        public int AttemptCount { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Throttle resends
        public DateTimeOffset? LastSentAt { get; set; }

        // Purpose: "register" | "login"
        [MaxLength(16)]
        public string Purpose { get; set; } = "login";

        public bool Consumed { get; set; }
    }
}
