using WebAPI.Models;
using WebAPI.Models.Auth;

namespace WebAPI.Contracts
{
    public interface ITokenService
    {
        LoginResponse CreateAccessToken(AppUser user, IList<string> roles);
    }
}
