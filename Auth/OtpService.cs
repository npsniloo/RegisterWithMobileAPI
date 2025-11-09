using Microsoft.EntityFrameworkCore;
using RegisterApp.DB;
using RegisterApp.Entities;
using RegisterApp.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace RegisterApp.Auth
{
    public class OtpService(IConfiguration cfg, ISmsSender sms, IOtpRepository otpRepository)
    {
    
        private readonly ISmsSender _sms = sms;
        private readonly int _len = int.Parse(cfg["Otp:CodeLength"] ?? "6");
        private readonly int _exp = int.Parse(cfg["Otp:ExpiresMinutes"] ?? "5");
        private readonly int _max = int.Parse(cfg["Otp:MaxAttempts"] ?? "5");
        private readonly int _resendCooldown = int.Parse(cfg["Otp:ResendCooldownSeconds"] ?? "60");
        private readonly IOtpRepository otpRepository= otpRepository;

        

        public static string Hash(string raw)
        {
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(raw)));
        }

        public string GenerateNumericCode()
        {
            // 6-digit by default, cryptographically strong
            var bytes = RandomNumberGenerator.GetBytes(4);
            var num = BitConverter.ToUInt32(bytes, 0) % (uint)Math.Pow(10, _len);
            return num.ToString(new string('0', _len));
        }

        public async Task<bool> HasOtpSentBefore(string phoneE164, string purpose, CancellationToken ct = default)
        {
            var now = DateTimeOffset.UtcNow;

            // throttle resends for latest active ticket
            var last = await otpRepository.GetLastValidSentOtp(phoneE164, purpose, ct);

            if (last is not null && last.LastSentAt.HasValue && (now - last.LastSentAt.Value).TotalSeconds < _resendCooldown)
                return true;
            return false;

        }
        public async Task<OtpTicket> CreateAndSendCodeAsync(string phoneE164, string purpose, Guid? userId = null, CancellationToken ct = default)
        {
            var now = DateTimeOffset.UtcNow;

            var code = GenerateNumericCode();
            var ticket = new OtpTicket
            {
                PhoneE164 = phoneE164,
                Purpose = purpose,
                ExpiresAt = now.AddMinutes(_exp),
                OtpHash = Hash(code),
                UserId = userId,
                LastSentAt = now
            };
            await otpRepository.AddOtp(ticket, purpose, ct);

            await _sms.SendAsync(phoneE164, $"Your {purpose} code is: {code}. It expires in {_exp} minutes.", ct);

            return ticket;
        }

        public async Task<(bool ok, OtpTicket? ticket, string? error)> VerifyAsync(string phoneE164, string code, string purpose, CancellationToken ct = default)
        {
            // validate code
            var now = DateTimeOffset.UtcNow;
            var ticket = await otpRepository.GetLastValidSentOtp(phoneE164, purpose, ct);
            if (ticket is null) return (false, null, "No active code. Request a new one.");
            if (ticket.ExpiresAt <= now) return (false, ticket, "Code expired.");

            ticket.AttemptCount++;
            if (ticket.AttemptCount > _max)
            {
                await otpRepository.UpdateOtp(ticket, ct);
                return (false, ticket, "Too many attempts. Request a new code.");
            }

            var match = ticket.OtpHash == Hash(code);
            if (!match)
            {
                await otpRepository.UpdateOtp(ticket, ct);
                return (false, ticket, "Invalid code.");
            }

            ticket.Consumed = true;
            await otpRepository.UpdateOtp(ticket, ct);
            return (true, ticket, null);
        }
    }
}
