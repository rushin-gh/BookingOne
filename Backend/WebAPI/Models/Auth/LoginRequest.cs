namespace WebAPI.Models.Auth
{
    public record LoginRequest(
        string Email,
        string Password
    );
}
