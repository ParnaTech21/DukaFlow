using DukaFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DukaFlow.Infrastructure.Persistence;

public class DukaFlowDbContext : DbContext
{
    public DukaFlowDbContext(DbContextOptions<DukaFlowDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Restaurant> Restaurants => Set<Restaurant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DukaFlowDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
