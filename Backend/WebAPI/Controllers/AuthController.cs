using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WebAPI.Contracts;
using WebAPI.Models;
using WebAPI.Models.Auth;


namespace WebAPI.Controllers
{
    [Route("api/v1/auth")]
    [ApiController]
    public class AuthController(UserManager<AppUser> userManager, ITokenService tokenService) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
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
            var loginResponse = tokenService.GetLoginToken(user, roles);

            return Ok(loginResponse);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<LoginResponse>> Refresh([Required] RefreshRequest request)
        {
            // TODO : Implement new class
            /*
                Response {
                    IsSuccess,
                    StatusCode,
                    Message,
                    ErrorMessage
                }
                 
            */

            LoginResponse loginResponse;
            try
            {
                loginResponse = await tokenService.RefreshAccessToken(request);
            }
            catch(Exception ex)
            {
                loginResponse = null;
            }
            return loginResponse;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var user = new AppUser
            {
                UserName = request.Email,
                Email = request.Email
            };

            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(error.Code, error.Description);

                return ValidationProblem(ModelState);   // 400 with ProblemDetails
            }

            var roleResult = await userManager.AddToRoleAsync(user, "Customer");
            if (!roleResult.Succeeded)
            {
                await userManager.DeleteAsync(user);    // don't leave a user with no role
                return Problem("Could not complete registration.", statusCode: 500);
            }

            return StatusCode(StatusCodes.Status201Created,
                new RegisterResponse(user.Id, user.Email!));
        }
    }
}
