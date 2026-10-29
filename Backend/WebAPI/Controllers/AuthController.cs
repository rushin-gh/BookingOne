using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Contracts;
using WebAPI.Models;
using WebAPI.Models.Auth;


namespace WebAPI.Controllers
{
    [Route("api/v1/auth")]
    [ApiController]
    public class AuthController(UserManager<AppUser> userManager, ITokenService tokens) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
                return Unauthorized(new { error = "Invalid email or password." });

            if (await userManager.IsLockedOutAsync(user))
                return Unauthorized(new { error = "Account temporarily locked." });

            if (!await userManager.CheckPasswordAsync(user, request.Password))
            {
                await userManager.AccessFailedAsync(user);   // counts toward lockout
                return Unauthorized(new { error = "Invalid email or password." });
            }

            await userManager.ResetAccessFailedCountAsync(user);

            var roles = await userManager.GetRolesAsync(user);
            var loginResponse = tokens.CreateAccessToken(user, roles);

            return Ok(loginResponse);
        }
    }
}
