using IDC.DBDeployTools.Utilities.DI;
using IDC.Utilities;
using IDC.Utilities.Models.API;

namespace IDC.DBDeployTools.Utilities.Middlewares;

/// <summary>
/// Middleware for filtering allowed HTTP methods based on configuration.
/// </summary>
/// <param name="next">Delegate for the next middleware in the pipeline.</param>
/// <param name="appConfigs">Application configuration handler for retrieving allowed methods.</param>
/// <param name="language">Service for localizing error messages.</param>
/// <remarks>
/// This middleware validates the HTTP request method against the list of allowed methods
/// from the configuration Security.Cors.AllowedMethods.
///
/// If the method is not allowed, it returns HTTP 405 Method Not Allowed.
///
/// Configuration example in appconfigs.jsonc:
/// <code>
/// {
///   "Security": {
///     "Cors": {
///       "AllowedMethods": [
///         "GET",
///         "POST"
///       ]
///     }
///   }
/// }
/// </code>
///
/// > [!IMPORTANT]
/// > This middleware must be registered before the routing middleware.
///
/// > [!NOTE]
/// > The OPTIONS method is always allowed to support CORS preflight requests.
///
/// > [!TIP]
/// > The middleware automatically adds the Allow header with the permitted methods.
/// </remarks>
/// <example>
/// Example usage in Program.Middlewares.cs:
/// <code>
/// // HTTP Method Filter - must be before routing
/// if (_appConfigs.Get(path: "Security.HttpMethodFilter.Enabled", defaultValue: true))
///     app.UseMiddleware&lt;HttpMethodFilterMiddleware&gt;();
/// </code>
/// </example>
/// <seealso cref="AppConfigsHandler"/>
/// <seealso cref="Language"/>
/// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/write">Write custom ASP.NET Core middleware</seealso>
public class HttpMethodFilterMiddleware(
    RequestDelegate next,
    AppConfigsHandler appConfigs,
    Language language
)
{
    /// <summary>
    /// Processes the HTTP request and validates the method used.
    /// </summary>
    /// <param name="context">The HTTP context of the request being processed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method will:
    /// 1. Retrieve the list of allowed methods from configuration.
    /// 2. Compare the request method with the allowed list.
    /// 3. Proceed to the next middleware if allowed.
    /// 4. Return HTTP 405 if not allowed.
    ///
    /// > [!NOTE]
    /// > The OPTIONS method is always permitted to support CORS preflight requests.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
    public async Task InvokeAsync(HttpContext context)
    {
        var allowedMethods =
            appConfigs.Get<string[]>(path: "Security.Cors.AllowedMethods")
            ?? ["GET", "POST", "PUT", "DELETE", "OPTIONS", "HEAD", "PATCH", "TRACE", "CONNECT"];

        var currentMethod = context.Request.Method.ToUpperInvariant();

        // OPTIONS is always permitted for CORS preflight requests
        if (currentMethod == "OPTIONS" || allowedMethods.Contains(value: currentMethod))
        {
            await next(context: context);
            return;
        }

        // Method is not permitted
        context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
        context.Response.Headers.Allow = string.Join(
            separator: ", ",
            values: (IEnumerable<string>)allowedMethods
        );

        await context.Response.WriteAsJsonAsync(
            value: new APIResponse()
                .ChangeStatus(language: language, key: "api.status.failed")
                .ChangeMessage(
                    message: $"HTTP method '{currentMethod}' is not allowed. Allowed methods: {string.Join(separator: ", ", values: (IEnumerable<string>)allowedMethods)}"
                )
        );
    }
}
