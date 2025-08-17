using IDC.Utilities;

namespace IDC.DBDeployTools.Utilities.Middlewares;

/// <summary>
/// Middleware for comprehensive HTTP request logging with performance metrics and error tracking.
/// Provides comprehensive logging of HTTP requests, including:
/// </summary>
/// <remarks>
/// Provides comprehensive logging of HTTP requests, including:
/// - Request method and path
/// - Request timing and performance metrics
/// - Response status codes and outcomes
/// - Error tracking and exception details
///
/// > [!IMPORTANT]
/// > Place this middleware early in the pipeline to capture all subsequent middleware execution times.
///
/// > [!NOTE]
/// > All timestamps are recorded in UTC to ensure consistent logging across time zones.
///
/// > [!TIP]
/// > Enable debug logging in development for additional request details.
///
/// Example implementation in Program.cs:
/// <code>
/// var builder = WebApplication.CreateBuilder(args);
/// var app = builder.Build();
///
/// // Configure logging middleware
/// app.UseMiddleware&lt;RequestLoggingMiddleware&gt;(
///     new SystemLogging(logPath: "logs/requests.log")
/// );
///
/// // Other middleware
/// app.UseRouting();
/// app.UseEndpoints(...);
/// </code>
/// </remarks>
/// <param name="next">The delegate representing the remaining middleware pipeline</param>
/// <param name="systemLogging">The logging service for recording request information</param>
/// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/">ASP.NET Core Middleware</seealso>
/// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/">Logging in .NET Core</seealso>
/// <seealso cref="SystemLogging"/>
public class RequestLoggingMiddleware(RequestDelegate next, SystemLogging systemLogging)
{
    /// <summary>
    /// Processes an individual HTTP request and logs comprehensive execution details.
    /// </summary>
    /// <param name="context"><see cref="HttpContext"/> The HTTP context for the current request.</param>
    /// <returns><see cref="Task"/> A Task representing the asynchronous middleware operation.</returns>
    /// <remarks>
    /// Provides detailed request tracking and performance monitoring by logging request execution flow
    /// and timing information.
    ///
    /// Processing steps:
    /// <list type="number">
    ///   <item><description>Records request start time (UTC)</description></item>
    ///   <item><description>Captures request method and path</description></item>
    ///   <item><description>Executes remaining middleware pipeline</description></item>
    ///   <item><description>Calculates execution duration</description></item>
    ///   <item><description>Logs completion or failure details</description></item>
    /// </list>
    ///
    /// Example request format:
    /// <code>
    /// {
    ///   "request": {
    ///     "method": "POST",
    ///     "path": "/api/users",
    ///     "headers": {
    ///       "Content-Type": "application/json",
    ///       "Authorization": "Bearer {token}"
    ///     },
    ///     "body": {
    ///       "username": "john.doe",
    ///       "email": "john@example.com"
    ///     }
    ///   }
    /// }
    /// </code>
    ///
    /// Example log entries:
    /// <example>
    /// <code>
    /// // Success case
    /// [2024-01-20 10:15:30 UTC] INFO: POST /api/users completed in 123.45ms with status 200
    ///
    /// // Error case
    /// [2024-01-20 10:16:45 UTC] ERROR: POST /api/users failed
    /// System.Exception: Invalid user data
    ///    at UserController.Create(UserDto dto)
    /// </code>
    /// </example>
    ///
    /// > [!NOTE]
    /// > All timestamps are recorded in UTC for consistent logging across time zones
    ///
    /// > [!TIP]
    /// > Enable debug level logging in development for additional request details
    ///
    /// > [!IMPORTANT]
    /// > Place this middleware early in the pipeline to capture complete execution times
    ///
    /// > [!CAUTION]
    /// > High-traffic applications may generate significant log volume
    ///
    /// > [!WARNING]
    /// > Ensure proper log rotation and storage management
    /// </remarks>
    /// <exception cref="Exception">Rethrows any exceptions from the middleware pipeline.</exception>
    /// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/">ASP.NET Core Middleware</seealso>
    /// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/">Logging in .NET Core</seealso>
    /// <seealso cref="SystemLogging"/>
    public async Task InvokeAsync(HttpContext context)
    {
        var start = DateTime.UtcNow;
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;

        try
        {
            await next(context: context);
            systemLogging.LogInformation(
                message: $"{requestMethod} {requestPath} completed in {(DateTime.UtcNow - start).TotalMilliseconds:F3}ms with status {context.Response.StatusCode}"
            );
        }
        catch (Exception ex)
        {
            systemLogging.LogError($"{requestMethod} {requestPath} failed");
            systemLogging.LogError(ex);
            throw;
        }
    }
}
