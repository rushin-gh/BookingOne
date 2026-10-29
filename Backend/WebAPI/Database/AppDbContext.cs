using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebAPI.Models;

namespace WebAPI.Database
{
    public class AppDbContext(DbContextOptions<AppDbContext> options)
      : IdentityDbContext<AppUser>(options)
    {
    }
}
