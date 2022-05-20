using Microsoft.EntityFrameworkCore;

namespace RedesCookies_API.Db.Entity;

public class SocialAuthDbContext : DbContext
{
    public SocialAuthDbContext(DbContextOptions<SocialAuthDbContext> options) : base(options)
    {

    }

    public DbSet<Usuario> Usuarios { get; set; }
}