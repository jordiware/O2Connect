using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using O2Connect.Api.Models.Store;

namespace O2Connect.Api.Persistence.EntityConfigurations;

public sealed class AuthorizationCodeConfiguration : IEntityTypeConfiguration<AuthorizationCode>
{
    public void Configure(EntityTypeBuilder<AuthorizationCode> builder)
    {
        builder.ToTable("authorization_codes");

        builder.HasKey(x => x.Code);

        builder.Property(x => x.Code).IsRequired().HasMaxLength(200);
        builder.Property(x => x.ClientId).IsRequired().HasMaxLength(100);
        builder.Property(x => x.UserId).IsRequired().HasMaxLength(100);
        builder.Property(x => x.CodeChallenge).IsRequired().HasMaxLength(200);
        builder.Property(x => x.CodeChallengeMethod).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Scopes).IsRequired().HasColumnType("text[]");
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.ExpiresAt).IsRequired();

        builder.HasOne(x => x.Client)
               .WithMany(c => c.AuthorizationCodes)
               .HasForeignKey(x => x.ClientId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
               .WithMany(u => u.AuthorizationCodes)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.ClientId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.ExpiresAt);
    }
}
