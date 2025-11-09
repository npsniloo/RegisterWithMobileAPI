using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RegisterApp.Interfaces;

namespace RegisterApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository userRepository;


        private readonly string UserMobileNumber;

        public UserController(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
            UserMobileNumber = User?.FindFirst("phone")?.Value;
        }


        [HttpGet("profile")]
        public async Task<IActionResult> GetProfileAsync(CancellationToken cancellationToken)
        {
            var user = await userRepository.GetUser(UserMobileNumber, cancellationToken);
            if (user is null)
                return NotFound();

            return Ok(new
            {
                id = user.Id,
                displayName = user.DisplayName,
                phone = user.PhoneE164,
                lastLoginAt = user.LastLoginAt
            });
        }
    }
}
