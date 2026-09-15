using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DukaFlow.Application.Menu.DTOs
{
    public record MenuItemDto(
    Guid Id,
    Guid MenuCategoryId,
    string Name,
    string? Description,
    decimal Price,
    string? ImageUrl,
    bool IsAvailable,
    bool IsActive,
    int DisplayOrder);
}
