namespace WebAPI.Models.Auth
{
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; } = default!;
        public AppUser User { get; set; } = default!;

        public string TokenHash { get; set; } = default!;
        public Guid FamilyId { get; set; }            // same for every token in one login chain

        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? ReplacedByHash { get; set; }

        public bool IsActive(DateTime now) => RevokedAt is null && ExpiresAt > now;
    }
}