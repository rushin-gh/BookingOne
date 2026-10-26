using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models
{
    public class JwtOptions
    {
        public const string SectionName = "JWT";

        [Required] public string Issuer { get; init; } = default!;
        [Required] public string Audience { get; init; } = default!;
        [Required, MinLength(32)] public string Key { get; init; } = default!;
        [Range(1, 1440)] public int AccessMinutes { get; init; } = 15;
        [Range(1, 90)] public int RefreshDays { get; init; } = 7;
    }
}