using AuthenticationAndAuthorizationJWT.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationAndAuthorizationJWT.DataServices.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<IdentityUser> IdentityUsers { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

    }
}
