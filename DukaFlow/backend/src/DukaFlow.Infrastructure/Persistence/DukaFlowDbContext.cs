using DukaFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DukaFlow.Infrastructure.Persistence;

public class DukaFlowDbContext : DbContext
{
    public DukaFlowDbContext(DbContextOptions<DukaFlowDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<MenuCategory> MenuCategories => Set<MenuCategory>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();

    // Phase 3 - Ordering Engine
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    // Phase 4 - WhatsApp Integration
    public DbSet<WhatsAppBusinessConfiguration> WhatsAppBusinessConfigurations => Set<WhatsAppBusinessConfiguration>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<InboundMessage> InboundMessages => Set<InboundMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DukaFlowDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
