using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebAPI.Models;

namespace WebAPI.Database
{
    public class AppDbContext(DbContextOptions<AppDbContext> options)
      : IdentityDbContext<AppUser>(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

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
