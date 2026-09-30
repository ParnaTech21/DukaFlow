using DukaFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DukaFlow.Infrastructure.Persistence.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Channel).IsRequired().HasMaxLength(30);
        builder.Property(c => c.ExternalConversationReference).HasMaxLength(200);

        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(c => c.CurrentState).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(c => c.PendingFulfillmentType).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.PendingDeliveryAddress).HasMaxLength(500);

        // One conversation per customer/restaurant for the MVP - no
        // concept of "closing" a conversation yet, it just idles at
        // ConversationState.Idle between orders.
        builder.HasIndex(c => new { c.RestaurantId, c.CustomerId }).IsUnique();

        builder.HasOne(c => c.Customer)
            .WithMany()
            .HasForeignKey(c => c.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Messages)
            .WithOne(m => m.Conversation)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Channel).IsRequired().HasMaxLength(30);
        builder.Property(m => m.ExternalMessageId).HasMaxLength(100);
        builder.Property(m => m.MessageType).IsRequired().HasMaxLength(30);
        builder.Property(m => m.Status).IsRequired().HasMaxLength(20);

        // Defensive cap - see Phase 4 CLAUDE.md #9 on not storing excessive
        // webhook payload content. ConversationService already truncates
        // before insert; this is the DB-level backstop.
        builder.Property(m => m.Content).IsRequired().HasMaxLength(1000);

        builder.HasIndex(m => m.ExternalMessageId);
    }
}
