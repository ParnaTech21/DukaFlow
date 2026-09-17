using System.ComponentModel.DataAnnotations;
using DukaFlow.Application.Menu.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DukaFlow.API.Controllers;

[ApiController]
[Authorize]
[Route("api/menu/images")]
public class MenuImagesController : ControllerBase
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    private readonly IImageStorageService _imageStorageService;
    public MenuImagesController(IImageStorageService imageStorageService) => _imageStorageService = imageStorageService;

    [HttpPost]
    [RequestSizeLimit(MaxFileSizeBytes)]
    public async Task<IActionResult> Upload(IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            throw new ValidationException("No image file was uploaded.");

        if (file.Length > MaxFileSizeBytes)
            throw new ValidationException("Image must be 5MB or smaller.");

        string relativeUrl;
        await using (var stream = file.OpenReadStream())
        {
            relativeUrl = await _imageStorageService.SaveMenuImageAsync(stream, file.FileName, file.ContentType, ct);
        }

        // Return an absolute URL so the frontend can store/display it directly as ImageUrl
        // without needing to know the API's host separately.
        var absoluteUrl = $"{Request.Scheme}://{Request.Host}{relativeUrl}";

        return Ok(new { success = true, data = new { url = absoluteUrl } });
    }
}
