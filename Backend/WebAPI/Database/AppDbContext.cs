using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebAPI.Models;
using WebAPI.Models.Auth;

namespace WebAPI.Database
{
    public class AppDbContext(DbContextOptions<AppDbContext> options)
      : IdentityDbContext<AppUser>(options)
    {

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<RefreshToken>(e =>
            {
                e.HasIndex(x => x.TokenHash).IsUnique();
                e.HasIndex(x => x.FamilyId);
                e.Property(x => x.TokenHash).HasMaxLength(128);
                e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = "role-admin",
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = "a1b2c3d4-0001-4000-8000-000000000001"
                },
                new IdentityRole
                {
                    Id = "role-organizer",
                    Name = "Organizer",
                    NormalizedName = "ORGANIZER",
                    ConcurrencyStamp = "a1b2c3d4-0002-4000-8000-000000000002"
                },
                new IdentityRole
                {
                    Id = "role-customer",
                    Name = "Customer",
                    NormalizedName = "CUSTOMER",
                    ConcurrencyStamp = "a1b2c3d4-0003-4000-8000-000000000003"
                }
            );
        }
    }
}
