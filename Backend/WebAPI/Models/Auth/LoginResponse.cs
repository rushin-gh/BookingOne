namespace WebAPI.Models.Auth
{
    public record LoginResponse(
        string Token,
        DateTime ExpiresAt
    );
}
