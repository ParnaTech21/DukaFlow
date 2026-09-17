namespace DukaFlow.Application.Menu.Interfaces;

public interface IImageStorageService
{
    /// <summary>
    /// Saves an uploaded menu image and returns a relative URL path (e.g. "/uploads/menu/{file}").
    /// The caller is responsible for turning this into an absolute URL if needed.
    /// </summary>
    Task<string> SaveMenuImageAsync(Stream content, string originalFileName, string contentType, CancellationToken ct);
}