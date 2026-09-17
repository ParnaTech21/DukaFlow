using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using DukaFlow.Application.Common;

namespace DukaFlow.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var (status, message) = ex switch
            {
                ConflictException => (HttpStatusCode.Conflict, ex.Message),
                UnauthorizedAppException => (HttpStatusCode.Unauthorized, ex.Message),
                NotFoundException => (HttpStatusCode.NotFound, ex.Message),
                ValidationException => (HttpStatusCode.BadRequest, ex.Message),
                _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
            };

            if (status == HttpStatusCode.InternalServerError)
                _logger.LogError(ex, "Unhandled exception");
            else
                _logger.LogWarning(ex, "Handled exception: {Message}", ex.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)status;

            var payload = JsonSerializer.Serialize(new { success = false, message, errors = Array.Empty<string>() });
            await context.Response.WriteAsync(payload);
        }
    }
}
