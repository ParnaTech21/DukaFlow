using DukaFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DukaFlow.Infrastructure.Persistence.Configurations;

public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.ToTable("MenuItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(i => i.Description)
            .HasMaxLength(1000);

        builder.Property(i => i.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.ImageUrl)
            .HasMaxLength(2048);

        builder.HasOne(i => i.Restaurant)
            .WithMany()
            .HasForeignKey(i => i.RestaurantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.MenuCategory)
            .WithMany(c => c.MenuItems)
            .HasForeignKey(i => i.MenuCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => new { i.RestaurantId, i.MenuCategoryId, i.DisplayOrder });
    }
}