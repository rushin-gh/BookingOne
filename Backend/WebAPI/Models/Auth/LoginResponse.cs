namespace WebAPI.Models.Auth
{
    public class LoginResponse
    {
        public string Token { get; set; }

        public DateTime ExpiresAt { get; set; }
    }
}
