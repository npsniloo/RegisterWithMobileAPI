using Microsoft.IdentityModel.Tokens;
using RegisterApp.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RegisterApp.Auth
{

    public class JwtService(IConfiguration cfg)
    {
        private readonly IConfiguration _cfg = cfg;

        public string IssueToken(AppUser user)
        {
            var issuer = _cfg["Jwt:Issuer"];
            var audience = _cfg["Jwt:Audience"];
            var key = _cfg["Jwt:Key"];
            var minutes = int.Parse(_cfg["Jwt:ExpiresMinutes"] ?? "30");

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(ClaimTypes.Name, user.DisplayName ?? user.PhoneE164),
                new("phone", user.PhoneE164),
                new("phone_verified", user.PhoneVerified.ToString().ToLowerInvariant())
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!));
            var cred = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer, audience, claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(minutes),
                signingCredentials: cred);

            return new JwtSecurityTokenHandler().WriteToken(token);
        
        }
    }

}
