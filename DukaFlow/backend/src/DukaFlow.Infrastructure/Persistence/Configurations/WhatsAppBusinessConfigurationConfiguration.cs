using DukaFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DukaFlow.Infrastructure.Persistence.Configurations;

public class WhatsAppBusinessConfigurationConfiguration : IEntityTypeConfiguration<WhatsAppBusinessConfiguration>
{
    public void Configure(EntityTypeBuilder<WhatsAppBusinessConfiguration> builder)
    {
        builder.ToTable("WhatsAppBusinessConfigurations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.PhoneNumberId).IsRequired().HasMaxLength(64);
        builder.Property(c => c.BusinessAccountId).HasMaxLength(64);
        builder.Property(c => c.DisplayPhoneNumber).HasMaxLength(30);

        // A given Meta phone number ID resolves to exactly one restaurant -
        // this is the tenant-routing lookup, so it must be unique. See
        // Phase 4 CLAUDE.md #15.
        builder.HasIndex(c => c.PhoneNumberId).IsUnique();

        builder.HasOne(c => c.Restaurant)
            .WithMany()
            .HasForeignKey(c => c.RestaurantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
