using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using O2Connect.Api.Models.Store;

namespace O2Connect.Api.Persistence.EntityConfigurations;

public class ParEntryConfiguration : IEntityTypeConfiguration<ParEntry>
{
    public void Configure(EntityTypeBuilder<ParEntry> builder)
    {
        builder.ToTable("par_entries");

        builder.HasKey(x => x.RequestUriCode);

        builder.Property(x => x.RequestUriCode).IsRequired().HasMaxLength(200);
        builder.Property(x => x.ClientId).IsRequired().HasMaxLength(100);
        builder.Property(x => x.RedirectUri).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Scopes).IsRequired().HasColumnType("text[]");
        builder.Property(x => x.ResponseType).IsRequired().HasMaxLength(50);
        builder.Property(x => x.CodeChallenge).IsRequired().HasMaxLength(200);
        builder.Property(x => x.CodeChallengeMethod).IsRequired().HasMaxLength(20);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.ExpiresAt).IsRequired();
        builder.Property(x => x.State).HasMaxLength(200);

        builder.HasOne(x => x.Client)
               .WithMany(c => c.ParEntries)
               .HasForeignKey(x => x.ClientId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.ClientId);
        builder.HasIndex(x => x.ExpiresAt);
    }
}
