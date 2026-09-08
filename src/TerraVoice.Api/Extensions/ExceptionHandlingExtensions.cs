using Microsoft.AspNetCore.Diagnostics;
using TerraVoice.Api.Errors;

namespace TerraVoice.Api.Extensions;

/// <summary>
///     Provides a global exception-handling middleware for the application.
///     This ensures all unhandled exceptions are captured and returned as
///     consistent JSON responses instead of the default HTML error page.
/// </summary>
public static class ExceptionHandlingExtensions
{
    /// <summary>
    ///     Registers a global exception handler that:
    ///     - Captures all unhandled exceptions.
    ///     - Maps known <see cref="AppException" /> instances to their
    ///     corresponding HTTP status codes and messages.
    ///     - Maps unknown exceptions to HTTP 500 with a generic error message.
    ///     - Returns errors in a unified JSON format: { error = "..." }.
    /// </summary>
    /// <param name="app">The application to configure.</param>
    public static void UseGlobalExceptionHandler(this WebApplication app)
    {
        // Register the global exception handler
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                // Retrieve the exception captured by the built-in handler.
                var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                var exception = exceptionHandlerFeature?.Error;

                // Log the exception
                EventLogException(context, exception);

                // Determine the appropriate status code.
                // If it's a known AppException, use its status code.
                // Otherwise, default to 500 Internal Server Error.
                var statusCode = exception is AppException appException
                    ? appException.StatusCode
                    : StatusCodes.Status500InternalServerError;

                // Determine the error message to return.
                // Known AppException → use its message.
                // Unknown exception → return a generic message to avoid leaking details.
                var message = exception is AppException ? exception.Message : "An unexpected error occurred.";

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";

                // Write a consistent JSON error response.
                await context.Response.WriteAsJsonAsync(new { detail = message });
            });
        });
    }

    private static void EventLogException(HttpContext context, Exception? exception)
    {
        if (exception is null) return;

        var logger = context.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("GlobalExceptionHandler");

        if (exception is AppException appException)
            logger.LogWarning(exception, "Handled application error: {Message}", appException.Message);
        else
            logger.LogError(exception, "Unhandled exception occurred.");
    }
}