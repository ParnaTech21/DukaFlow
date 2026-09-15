using DukaFlow.Domain.Common;
using DukaFlow.Domain.Enums;

namespace DukaFlow.Domain.Entities;

public class User : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.RestaurantOwner;
    public bool IsActive { get; set; } = true;

    public Restaurant? Restaurant { get; set; }
}
