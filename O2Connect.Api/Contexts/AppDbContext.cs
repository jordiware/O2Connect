using Microsoft.EntityFrameworkCore;

namespace O2Connect.Api.Contexts;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // Example DbSets (you’ll expand this later)
    // public DbSet<User> Users => Set<User>();
    // public DbSet<Client> Clients => Set<Client>();
}
