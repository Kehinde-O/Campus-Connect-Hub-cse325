using System.Net;
using System.Text.Json;

namespace CampusConnectHub.Server.Middleware;

/// <summary>
/// Global exception handling middleware that catches all unhandled exceptions
/// and returns user-friendly error responses.
/// </summary>
public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        var message = "An error occurred while processing your request.";

        // Handle specific exception types
        switch (exception)
        {
            case UnauthorizedAccessException:
                code = HttpStatusCode.Unauthorized;
                message = "You do not have permission to perform this action.";
                break;
            case ArgumentNullException argEx:
                code = HttpStatusCode.BadRequest;
                message = $"Invalid request: {argEx.ParamName} is required.";
                break;
            case ArgumentException argEx:
                code = HttpStatusCode.BadRequest;
                message = $"Invalid request: {argEx.Message}";
                break;
            case KeyNotFoundException:
                code = HttpStatusCode.NotFound;
                message = "The requested resource was not found.";
                break;
        }

        var result = JsonSerializer.Serialize(new
        {
            error = message,
            statusCode = (int)code
        });

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        return context.Response.WriteAsync(result);
    }
}

