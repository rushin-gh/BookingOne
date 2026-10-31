using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Auth
{
    public record RegisterRequest(
        [Required, EmailAddress, MaxLength(256)] string Email,
        [Required, MinLength(8), MaxLength(100)] string Password
    );
}
