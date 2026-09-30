using DukaFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DukaFlow.Infrastructure.Persistence.Configurations;

public class InboundMessageConfiguration : IEntityTypeConfiguration<InboundMessage>
{
    public void Configure(EntityTypeBuilder<InboundMessage> builder)
    {
        builder.ToTable("InboundMessages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.ExternalMessageId).IsRequired().HasMaxLength(100);
        builder.Property(m => m.CustomerPhoneNumber).IsRequired().HasMaxLength(30);
        builder.Property(m => m.MessageType).IsRequired().HasMaxLength(30);
        builder.Property(m => m.ProcessingStatus).IsRequired().HasMaxLength(20);

        // The whole point of this table: turn a Meta redelivery into a
        // no-op via a unique-constraint hit rather than a duplicate
        // cart/order. See Phase 4 CLAUDE.md #7.
        builder.HasIndex(m => m.ExternalMessageId).IsUnique();
    }
}
