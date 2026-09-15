using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DukaFlow.Application.Menu.DTOs
{
    public record CreateMenuCategoryRequest(
    string Name,
    string? Description,
    int DisplayOrder);
}
