using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RegisterApp.Auth;
using RegisterApp.Interfaces;
using RegisterApp.Models.Request;
using RegisterApp.Models.Response;

namespace RegisterApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IAuthService authService;

        public LoginController(IAuthService authService)
        {
            this.authService=authService;
        }
        [HttpPost]
        public async Task<IResult> StartLogin(AuthRequest request,CancellationToken cancellationToken)
        {
            var result = await authService.LoginAsync(request.MobileNumber, cancellationToken);
            if (result.ok)
                return Results.Ok(result.message);
            return Results.Conflict(result.message);
        }

        [HttpPost("verify")]
        public async Task<IResult> VerifyLogin(VerifyRequest request, CancellationToken cancellationToken)
        {
            var result = await authService.VerifyLoginAsync(request.MobileNumber, request.Code, cancellationToken);
            if (result.ok)
                return Results.Ok(new VerifyResponse(token:result.token, tokenType: result.tokenType));
            return Results.Conflict(result.error);
        }
    }
}
