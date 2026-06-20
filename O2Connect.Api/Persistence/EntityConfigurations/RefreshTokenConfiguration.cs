using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using O2Connect.Api.Models.Store;

namespace O2Connect.Api.Persistence.EntityConfigurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(x => x.Token);

        builder.Property(x => x.Scopes).IsRequired().HasColumnType("text[]");
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.ExpiresAt).IsRequired();

        builder.HasOne(x => x.Client)
               .WithMany(c => c.RefreshTokens)
               .HasForeignKey(x => x.ClientId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
               .WithMany(u => u.RefreshTokens)
               .HasForeignKey(x => x.Subject)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ReplacedBy)
               .WithMany()
               .HasForeignKey(x => x.ReplacedByToken)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.SessionId);
        builder.HasIndex(x => x.Subject);
        builder.HasIndex(x => x.ClientId);
        builder.HasIndex(x => new { x.Subject, x.ClientId });
        builder.HasIndex(x => x.ExpiresAt);
    }
}
