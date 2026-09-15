using DukaFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DukaFlow.Infrastructure.Persistence.Configurations;

public class RestaurantConfiguration : IEntityTypeConfiguration<Restaurant>
{
    public void Configure(EntityTypeBuilder<Restaurant> builder)
    {
        builder.ToTable("Restaurants");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Description).HasMaxLength(1000);
        builder.Property(r => r.PhoneNumber).HasMaxLength(30);
        builder.Property(r => r.WhatsAppNumber).HasMaxLength(30);
        builder.Property(r => r.Address).HasMaxLength(300);

        builder.HasIndex(r => r.OwnerId).IsUnique();
    }
}
