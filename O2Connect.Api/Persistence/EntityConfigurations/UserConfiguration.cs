using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using O2Connect.Api.Models.Store;

namespace O2Connect.Api.Persistence.EntityConfigurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .HasMaxLength(100);

        builder.Property(x => x.Username)
               .IsRequired()
               .HasMaxLength(16);

        builder.Property(x => x.NormalizedUsername)
               .IsRequired()
               .HasMaxLength(24);

        builder.Property(x => x.Email)
               .IsRequired()
               .HasMaxLength(256);

        builder.Property(x => x.PasswordHash)
               .IsRequired();

        builder.Property(x => x.Role)
               .IsRequired()
               .HasMaxLength(16);

        builder.Property(x => x.Status)
               .IsRequired();

        builder.Property(x => x.CreatedAt)
               .IsRequired();

        builder.Property(x => x.DisplayName)
               .HasMaxLength(64);

        builder.Property(x => x.ImageUrl)
               .HasMaxLength(500);

        builder.Property(x => x.Scopes)
               .HasConversion(v => string.Join(' ', v),
                              v => v.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                    .ToHashSet()
               );

        builder.HasIndex(x => x.NormalizedUsername)
               .IsUnique();

        builder.HasIndex(x => x.Email)
               .IsUnique();
    }
}
