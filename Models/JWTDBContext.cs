using Microsoft.EntityFrameworkCore;

namespace WEB_API_JWT_AUTH.Models
{
    public class JWTDBContext : DbContext
    {
        public JWTDBContext(DbContextOptions<JWTDBContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}
