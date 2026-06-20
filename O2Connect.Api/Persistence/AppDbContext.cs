using Microsoft.EntityFrameworkCore;
using O2Connect.Api.Models.Store;
using O2Connect.Api.Persistence.EntityConfigurations;

namespace O2Connect.Api.Persistence;

public sealed class AppDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<UserConsent> UserConsents => Set<UserConsent>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfiguration(new UserConfiguration());
        builder.ApplyConfiguration(new ClientConfiguration());
        builder.ApplyConfiguration(new UserConsentConfiguration());
        builder.ApplyConfiguration(new RefreshTokenConfiguration());
        //builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
