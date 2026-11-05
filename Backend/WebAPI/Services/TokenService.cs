using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebAPI.Contracts;
using WebAPI.Database;
using WebAPI.Models;
using WebAPI.Models.Auth;

namespace WebAPI.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtOptions _jwt;
        private readonly AppDbContext _db;

        private readonly UserManager<AppUser> _userManager;

        public TokenService(
            IOptions<JwtOptions> options, 
            AppDbContext db, 
            UserManager<AppUser> userManager
        )
        {
            _jwt = options.Value;
            _db = db;
            _userManager = userManager;
        }

        public LoginResponse GetLoginToken(AppUser user, IList<string> roles)
        {
            var accessToken = GetAccessToken(user, roles);
            var refreshToken = GetRefreshToken(user.Id);

            return new LoginResponse(
                accessToken.AccessToken,
                accessToken.ExpiresAt,
                refreshToken
            );
        }

        public async Task<LoginResponse> RefreshAccessToken(RefreshRequest request)
        {
            var hash = Hash(request.RefreshToken);
            var now = DateTime.UtcNow;

            await using var transaction = await _db.Database.BeginTransactionAsync();

            var stored = await _db.RefreshTokens
                            .Include(x => x.User)
                            .FirstOrDefaultAsync(y => y.TokenHash == hash);

            if (stored == null)
            {
                throw new Exception("Invalid refresh token");
            }

            if (stored.RevokedAt != null)
            {
                await _db.RefreshTokens
                .Where(t => t.FamilyId == stored.FamilyId && t.RevokedAt == null)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now));
                await transaction.CommitAsync();
                throw new Exception("Refresh Token is already revoked");
            }

            if (stored.ExpiresAt <= now)
                throw new Exception("Refresh token in expired");


            var (newRaw, newHash) = NewRefreshToken();
            stored.RevokedAt = now;
            stored.ReplacedByHash = newHash;

            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = stored.UserId,
                TokenHash = newHash,
                FamilyId = stored.FamilyId,
                CreatedAt = now,
                ExpiresAt = now.AddDays(_jwt.RefreshDays)
            });

            var roles = await _userManager.GetRolesAsync(stored.User);
            var (access, expires) = GetAccessToken(stored.User, roles);

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return new LoginResponse(access, expires, newRaw);
        }

        private (string AccessToken, DateTime ExpiresAt) GetAccessToken(AppUser user, IList<string> roles)
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
            var expires = DateTime.Now.AddMinutes(_jwt.AccessMinutes);

            var token = new JwtSecurityToken(
               issuer: _jwt.Issuer,
               audience: _jwt.Audience,
               claims: claims,
               notBefore: DateTime.Now,
               expires: expires,
               signingCredentials: creds
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expires);
        }

        private string GetRefreshToken(string userId, Guid? familyId = null)
        {
            var (raw, hash) = NewRefreshToken();
            var now = DateTime.UtcNow;

            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = userId,
                TokenHash = hash,
                FamilyId = familyId ?? Guid.NewGuid(),
                CreatedAt = now,
                ExpiresAt = now.AddDays(_jwt.RefreshDays)
            });
            _db.SaveChanges();

            return raw;
        }

        private (string Raw, string Hash) NewRefreshToken()
        {
            var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            return (raw, Hash(raw));
        }

        private string Hash(string raw) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));

    }
}
