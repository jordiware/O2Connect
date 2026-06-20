using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using O2Connect.Api.Models.Store;

namespace O2Connect.Api.Persistence.EntityConfigurations;

public sealed class UserConsentConfiguration : IEntityTypeConfiguration<UserConsent>
{
    public void Configure(EntityTypeBuilder<UserConsent> builder)
    {
        builder.ToTable("user_consent");

        builder.HasKey(x => new { x.UserId, x.ClientId });

        builder.Property(x => x.CreatedAt).IsRequired();

        // Relationships
        builder.HasOne(x => x.User)
               .WithMany(x => x.Consents)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Client)
               .WithMany(x => x.Consents)
               .HasForeignKey(x => x.ClientId)
               .OnDelete(DeleteBehavior.Cascade);

        // Store GrantedScopes
        builder.Property(x => x.GrantedScopes)
               .HasConversion(
                   v => string.Join(',', v),
                   v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToHashSet()
               );
    }
}
