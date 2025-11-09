using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegisterApp.Auth;
using RegisterApp.DB;
using RegisterApp.Entities;
using RegisterApp.Interfaces;
using RegisterApp.Models.Request;
using RegisterApp.Models.Response;
using System.Numerics;
using static System.Net.WebRequestMethods;

namespace RegisterApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly IAuthService authService;

        public RegisterController(IAuthService authService)
        {
            this.authService=authService;
        }
        [HttpPost]
        public async Task<IResult> Register(AuthRequest request, CancellationToken cancellationToken)
        {
           var result = await authService.RegisterAsync(request.MobileNumber, cancellationToken);
            if (result.ok) 
                return Results.Ok(result.message);
            return Results.Conflict(result.message);
        }

        [HttpPost("verify")]
        public async Task<IResult> Verify(VerifyRequest request, CancellationToken cancellationToken)
        {
            var result = await authService.VerifyRegisterAsync(request.MobileNumber, request.Code, cancellationToken);
            if (result.ok)
                return Results.Ok(new VerifyResponse(token: result.token, tokenType: result.tokenType));
            return Results.Conflict(result.error);

        }
    }
}
