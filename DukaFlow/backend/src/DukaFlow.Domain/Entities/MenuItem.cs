using DukaFlow.Domain.Common;

namespace DukaFlow.Domain.Entities;

public class MenuItem : BaseEntity
{
    public Guid RestaurantId { get; set; }
    public Guid MenuCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    // Navigation
    public Restaurant Restaurant { get; set; } = null!;
    public MenuCategory MenuCategory { get; set; } = null!;
}