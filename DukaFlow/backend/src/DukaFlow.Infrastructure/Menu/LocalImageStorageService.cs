using System.ComponentModel.DataAnnotations;
using DukaFlow.Application.Menu.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace DukaFlow.Infrastructure.Menu;

/// <summary>
/// Saves menu images to wwwroot/uploads/menu on local disk and serves them via
/// ASP.NET Core static files. Per Phase 2 CLAUDE.md Section 11, this is a
/// development-friendly strategy; only the ImageUrl string is ever persisted
/// in SQL Server, so swapping this out for Azure Blob Storage later only
/// requires a new IImageStorageService implementation.
/// </summary>
public class LocalImageStorageService : IImageStorageService
{
    private static readonly Dictionary<string, string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp",
    };

    private readonly IWebHostEnvironment _env;

    public LocalImageStorageService(IWebHostEnvironment env) => _env = env;

    public async Task<string> SaveMenuImageAsync(Stream content, string originalFileName, string contentType, CancellationToken ct)
    {
        if (!AllowedContentTypes.TryGetValue(contentType, out var extension))
            throw new ValidationException("Only JPEG, PNG, or WEBP images are allowed.");

        var webRootPath = _env.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
            webRootPath = Path.Combine(_env.ContentRootPath, "wwwroot");

        var uploadsFolder = Path.Combine(webRootPath, "uploads", "menu");
        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(uploadsFolder, fileName);

        await using (var fileStream = new FileStream(fullPath, FileMode.Create))
        {
            await content.CopyToAsync(fileStream, ct);
        }

        return $"/uploads/menu/{fileName}";
    }
}
