using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using O2Connect.Api.Models.Store;

namespace O2Connect.Api.Persistence.EntityConfigurations;

public sealed class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("clients");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasMaxLength(100);
        builder.Property(x => x.DisplayName).IsRequired().HasMaxLength(32);
        builder.Property(x => x.NormalizedName).IsRequired().HasMaxLength(48);
        builder.Property(x => x.ImageUrl).HasMaxLength(500);
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.OwnerId).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.ClientSecret);
        builder.Property(x => x.JsonWebKeysUri).HasMaxLength(500);
        builder.Property(x => x.RequiresSecret).IsRequired();
        builder.Property(x => x.RequiresPkce).IsRequired();
        builder.Property(x => x.RequiresConsent).IsRequired();
        builder.Property(x => x.AllowPlainPkce).IsRequired();
        builder.Property(x => x.AllowPar).IsRequired();
        builder.Property(x => x.RedirectUris).IsRequired().HasColumnType("text[]");
        builder.Property(x => x.AllowedGrantTypes).IsRequired().HasColumnType("text[]");
        builder.Property(x => x.AllowedScopes).IsRequired().HasColumnType("text[]");
        builder.Property(x => x.AllowedAuthenticationMethods).IsRequired().HasColumnType("text[]");
        builder.Property(x => x.AllowedResponseTypes).IsRequired().HasColumnType("text[]");

        builder.HasOne<User>()
               .WithMany(x => x.Clients)
               .HasForeignKey(x => x.OwnerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.NormalizedName).IsUnique();
        builder.HasIndex(x => x.OwnerId);
        builder.HasIndex(x => x.Status);
    }
}
