using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebAPI.Contracts;
using WebAPI.Models;
using WebAPI.Models.Auth;

namespace WebAPI.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtOptions _jwt;

        public TokenService(IOptions<JwtOptions> options)
        {
            _jwt = options.Value;
        }

        public LoginResponse CreateAccessToken(AppUser user, IList<string> roles)
        {
            var claims = new List<Claim>()
            {
                new("sub", user.Id),
                new("email", user.Email!),
                new("jti", Guid.NewGuid().ToString())
            };
            claims.AddRange(roles.Select(r => new Claim("role", r)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_jwt.AccessMinutes);

            var token = new JwtSecurityToken(
               issuer: _jwt.Issuer,
               audience: _jwt.Audience,
               claims: claims,
               notBefore: DateTime.UtcNow,
               expires: expires,
               signingCredentials: creds
            );

            return new LoginResponse()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAt = expires
            };
        }

    }
}
