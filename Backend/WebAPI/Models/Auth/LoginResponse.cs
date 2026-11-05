namespace WebAPI.Models.Auth
{
    public record LoginResponse(
        string AccessToken,
        DateTime ExpiresAt,
        string RefreshToken
    );
}
