using AuthenticationAndAuthorizationJWT.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationAndAuthorizationJWT.DataServices.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

    }
}
