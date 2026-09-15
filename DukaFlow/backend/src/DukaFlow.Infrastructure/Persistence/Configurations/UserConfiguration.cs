using DukaFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DukaFlow.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.LastName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.Role).IsRequired().HasConversion<string>().HasMaxLength(30);

        builder.HasIndex(u => u.Email).IsUnique();

        builder.HasOne(u => u.Restaurant)
            .WithOne(r => r.Owner)
            .HasForeignKey<Restaurant>(r => r.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
