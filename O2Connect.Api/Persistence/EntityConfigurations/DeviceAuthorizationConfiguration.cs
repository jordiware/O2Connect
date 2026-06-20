using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using O2Connect.Api.Models.Store;

namespace O2Connect.Api.Persistence.EntityConfigurations;

public class DeviceAuthorizationConfiguration : IEntityTypeConfiguration<DeviceAuthorization>
{
    public void Configure(EntityTypeBuilder<DeviceAuthorization> builder)
    {
        builder.ToTable("device_authorizations");

        builder.HasKey(x => x.DeviceCodeHash);

        builder.Property(x => x.DeviceCodeHash).IsRequired().HasMaxLength(200);
        builder.Property(x => x.UserCodeHash).IsRequired().HasMaxLength(200);
        builder.Property(x => x.ClientId).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Scopes).IsRequired().HasColumnType("text[]");
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.ExpiresAtUtc).IsRequired();
        builder.Property(x => x.PollCount).IsRequired();
        builder.Property(x => x.Interval).IsRequired();

        builder.HasOne(x => x.Client)
               .WithMany(c => c.DeviceAuthorizations)
               .HasForeignKey(x => x.ClientId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
               .WithMany(u => u.DeviceAuthorizations)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.UserCodeHash).IsUnique();
        builder.HasIndex(x => x.ClientId);
        builder.HasIndex(x => x.ExpiresAtUtc);
    }
}
