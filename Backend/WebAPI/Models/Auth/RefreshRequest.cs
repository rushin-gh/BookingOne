using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Auth
{
    public record RefreshRequest([Required] string RefreshToken);
}
