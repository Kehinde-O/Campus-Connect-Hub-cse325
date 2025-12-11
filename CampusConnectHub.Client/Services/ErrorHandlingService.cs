using Microsoft.JSInterop;

namespace CampusConnectHub.Client.Services;

/// <summary>
/// Service for handling and displaying errors to users in a user-friendly manner.
/// Provides centralized error handling with user feedback.
/// </summary>
public class ErrorHandlingService
{
    private readonly IJSRuntime _jsRuntime;

    public ErrorHandlingService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Handles an exception and returns a user-friendly error message.
    /// </summary>
    /// <param name="ex">The exception to handle</param>
    /// <returns>A user-friendly error message</returns>
    public string GetUserFriendlyErrorMessage(Exception ex)
    {
        return ex switch
        {
            HttpRequestException httpEx when httpEx.Message.Contains("401") || httpEx.Message.Contains("Unauthorized") 
                => "You are not authorized to perform this action. Please log in and try again.",
            
            HttpRequestException httpEx when httpEx.Message.Contains("403") || httpEx.Message.Contains("Forbidden")
                => "You do not have permission to access this resource.",
            
            HttpRequestException httpEx when httpEx.Message.Contains("404") || httpEx.Message.Contains("Not Found")
                => "The requested resource was not found. It may have been removed or moved.",
            
            HttpRequestException httpEx when httpEx.Message.Contains("400") || httpEx.Message.Contains("Bad Request")
                => "Invalid request. Please check your input and try again.",
            
            HttpRequestException httpEx when httpEx.Message.Contains("500") || httpEx.Message.Contains("Internal Server Error")
                => "A server error occurred. Please try again later or contact support if the problem persists.",
            
            HttpRequestException httpEx when httpEx.Message.Contains("Failed to fetch") || httpEx.Message.Contains("NetworkError")
                => "Unable to connect to the server. Please check your internet connection and ensure the server is running.",
            
            TimeoutException
                => "The request timed out. Please check your connection and try again.",
            
            TaskCanceledException
                => "The request was cancelled. Please try again.",
            
            _ => "An unexpected error occurred. Please try again or contact support if the problem persists."
        };
    }

    /// <summary>
    /// Logs an error to the browser console for debugging purposes.
    /// </summary>
    /// <param name="ex">The exception to log</param>
    /// <param name="context">Additional context about where the error occurred</param>
    public async Task LogErrorAsync(Exception ex, string context = "")
    {
        try
        {
            var errorMessage = string.IsNullOrEmpty(context) 
                ? $"{ex.GetType().Name}: {ex.Message}" 
                : $"{context} - {ex.GetType().Name}: {ex.Message}";
            
            await _jsRuntime.InvokeVoidAsync("console.error", errorMessage, ex.StackTrace);
        }
        catch
        {
            // Silently fail if logging fails
        }
    }
}

