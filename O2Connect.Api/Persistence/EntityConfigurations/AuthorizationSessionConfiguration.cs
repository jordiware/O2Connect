using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using O2Connect.Api.Models.Store;

namespace O2Connect.Api.Persistence.EntityConfigurations;

public class AuthorizationSessionConfiguration : IEntityTypeConfiguration<AuthorizationSession>
{
    public void Configure(EntityTypeBuilder<AuthorizationSession> builder)
    {
        builder.ToTable("authorization_sessions");

        builder.HasKey(x => x.SessionId);

        builder.Property(x => x.SessionId).ValueGeneratedNever();
        builder.Property(x => x.Status).HasConversion<string>().IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.ExpiresAt).IsRequired();
        builder.Property(x => x.ClientId).IsRequired();
        builder.Property(x => x.ClientDisplayName).IsRequired();
        builder.Property(x => x.UserId);
        builder.Property(x => x.UserDisplayName);
        builder.Property(x => x.RequestUriCode);
        builder.Property(x => x.RequestedScopes).HasColumnType("text[]").IsRequired(false);
        builder.Property(x => x.MissingScopes).HasColumnType("text[]").IsRequired(false);

        builder.OwnsOne(x => x.Request).ToJson();

        builder.Navigation(x => x.Request).IsRequired();

        builder.HasOne(x => x.Client)
               .WithMany(c => c.AuthorizationSessions)
               .HasForeignKey(x => x.ClientId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
               .WithMany(u => u.AuthorizationSessions)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.ExpiresAt);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.ClientId);
        builder.HasIndex(x => x.UserId);
    }
}
