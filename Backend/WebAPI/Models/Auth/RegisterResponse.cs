namespace WebAPI.Models.Auth
{
    public record RegisterResponse
    {
        public string Id { get; init; } = "";
        public string Email { get; init; } = "";

        public RegisterResponse(string id, string email)
        {
            Id = id;
            Email = email;
        }
    }
}
