using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using O2Connect.Api.Models.Store;

namespace O2Connect.Api.Persistence.EntityConfigurations;

public class DeviceAuthorizationConfiguration : IEntityTypeConfiguration<DeviceAuthorization>
{
    public void Configure(EntityTypeBuilder<DeviceAuthorization> entity)
    {
        entity.ToTable("device_authorizations");

        entity.HasKey(x => x.DeviceCodeHash);

        entity.Property(x => x.DeviceCodeHash).IsRequired().HasMaxLength(200);
        entity.Property(x => x.UserCodeHash).IsRequired().HasMaxLength(200);
        entity.Property(x => x.ClientId).IsRequired().HasMaxLength(100);
        entity.Property(x => x.Scopes).IsRequired().HasColumnType("text[]");
        entity.Property(x => x.CreatedAtUtc).IsRequired();
        entity.Property(x => x.ExpiresAtUtc).IsRequired();
        entity.Property(x => x.PollCount).IsRequired();
        entity.Property(x => x.Interval).IsRequired();

        entity.HasOne(x => x.Client)
              .WithMany(c => c.DeviceAuthorizations)
              .HasForeignKey(x => x.ClientId)
              .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(x => x.User)
              .WithMany(u => u.DeviceAuthorizations)
              .HasForeignKey(x => x.UserId)
              .OnDelete(DeleteBehavior.SetNull);

        entity.HasIndex(x => x.UserCodeHash).IsUnique();
        entity.HasIndex(x => x.ClientId);
        entity.HasIndex(x => x.ExpiresAtUtc);
    }
}
