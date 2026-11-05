using WebAPI.Models;
using WebAPI.Models.Auth;

namespace WebAPI.Contracts
{
    public interface ITokenService
    {
        LoginResponse GetLoginToken(AppUser user, IList<string> roles);

        Task<LoginResponse> RefreshAccessToken(RefreshRequest request);
    }
}
