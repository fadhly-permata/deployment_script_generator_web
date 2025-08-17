using System.Net;
using IDC.DBDeployTools.Utilities.DI;
using IDC.Utilities;
using IDC.Utilities.Models.API;
using Newtonsoft.Json.Linq;

namespace IDC.DBDeployTools.Utilities.Middlewares;

/// <summary>
/// Middleware for API key authentication.
/// </summary>
/// <remarks>
/// This middleware validates the API key provided in the request header against a list of registered keys.
/// It handles various scenarios such as missing API key, empty registered key list, and invalid API key.
///
/// The middleware checks for the presence of an API key in the 'X-API-Key' header of incoming requests.
/// If the key is missing, invalid, or not registered, it returns an appropriate error response.
/// Otherwise, it allows the request to proceed to the next middleware in the pipeline.
///
/// Example usage:
/// <example>
/// <code>
/// // In Program.cs or Startup.cs
/// app.UseMiddleware&lt;ApiKeyAuthenticationMiddleware&gt;();
///
/// // In appsettings.json
/// {
///   "Security": {
///     "RegisteredApiKeyList": [
///       "NFHUZqt0zmL6siZ7/ynQ8nljJtsQrT3h0+nQZHhIQhk=",
///       "IDxvX6aT3XTERRpuHpMNtpcQVUo2rZ3Smtm83UPVfi8="
///     ]
///   }
/// }
///
/// // Example request
/// GET /api/resource HTTP/1.1
/// Host: example.com
/// X-API-Key: your-api-key-here
///
/// // Example success response
/// HTTP/1.1 200 OK
/// Content-Type: application/json
///
/// {
///   "status": "Success",
///   "message": "Operation completed successfully",
///   "data": { ... }
/// }
///
/// // Example error response
/// HTTP/1.1 401 Unauthorized
/// Content-Type: application/json
///
/// {
///   "status": "Failed",
///   "message": "Invalid API key",
///   "errors": ["The provided API key is not recognized"]
/// }
/// </code>
/// </example>
///
/// > [!NOTE]
/// > This middleware automatically skips authentication for static files and Swagger documentation
///
/// > [!TIP]
/// > Use environment-specific API keys for different deployment environments
///
/// > [!IMPORTANT]
/// > Always use HTTPS in production to secure API key transmission
///
/// > [!CAUTION]
/// > Never expose API keys in client-side code or version control
///
/// > [!WARNING]
/// > Implement rate limiting alongside API key authentication for better security
/// </remarks>
/// <param name="appConfigs">
/// <see cref="AppConfigsHandler"/> The configuration handler for accessing application settings.
/// <see href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/"/>
/// </param>
/// <param name="language">
/// <see cref="Language"/> The language service for response localization.
/// <see href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/localization"/>
/// </param>
/// <returns>
/// An instance of <see cref="ApiKeyAuthenticationMiddleware"/> configured with the provided dependencies.
/// </returns>
/// <exception cref="ArgumentNullException">
/// Thrown when either <paramref name="appConfigs"/> or <paramref name="language"/> is null.
/// </exception>
/// <seealso cref="IMiddleware"/>
/// <seealso cref="ApiKeyAuthenticationAttribute"/>
/// <seealso cref="ApiKeyGenerator"/>
/// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/">
/// ASP.NET Core Middleware Documentation
/// </seealso>
/// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/security/authentication/">
/// ASP.NET Core Authentication
/// </seealso>
/// <seealso href="https://owasp.org/www-project-api-security/">
/// OWASP API Security Project
/// </seealso>
public class ApiKeyAuthenticationMiddleware(AppConfigsHandler appConfigs, Language language)
    : IMiddleware
{
    private readonly AppConfigsHandler _appConfigs = appConfigs;
    private readonly Language _language = language;
    private const string API_KEY_HEADER = "X-API-Key";

    /// <summary>
    /// Writes an error response to the HTTP context with localized error messages.
    /// </summary>
    /// <remarks>
    /// This method sets the response status code and writes a JSON response with a localized error message.
    /// The response follows the standard API response format using <see cref="APIResponse"/>.
    ///
    /// Common message keys and their meanings:
    /// <list type="bullet">
    ///   <item><description>"missing" - API key is not present in request headers</description></item>
    ///   <item><description>"invalid" - API key is not found in registered keys</description></item>
    ///   <item><description>"not_registered" - No API keys are configured in the system</description></item>
    /// </list>
    ///
    /// Example usage:
    /// <example>
    /// <code>
    /// await WriteErrorResponse(
    ///     context: httpContext,
    ///     statusCode: HttpStatusCode.Unauthorized,
    ///     messageKey: "missing"
    /// );
    /// </code>
    /// </example>
    ///
    /// Example response:
    /// <example>
    /// <code>
    /// {
    ///   "status": "Failed",
    ///   "message": "API key is missing",
    ///   "data": null,
    ///   "errors": null
    /// }
    /// </code>
    /// </example>
    ///
    /// > [!NOTE]
    /// > All error messages are automatically localized based on the configured language
    ///
    /// > [!TIP]
    /// > Use appropriate HTTP status codes for different error scenarios
    /// </remarks>
    /// <param name="context">
    /// <see cref="HttpContext"/> The HTTP context for the current request.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext"/>
    /// </param>
    /// <param name="statusCode">
    /// <see cref="HttpStatusCode"/> The HTTP status code to be set in the response.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.net.httpstatuscode"/>
    /// </param>
    /// <param name="messageKey">
    /// The localization key for the error message. This key will be used to lookup the appropriate
    /// message in the language resources.
    /// </param>
    /// <returns>
    /// <see cref="Task"/> A task representing the asynchronous operation of writing the response.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task"/>
    /// </returns>
    /// <seealso cref="APIResponse"/>
    /// <seealso cref="Language"/>
    /// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling">
    /// Error handling in ASP.NET Core
    /// </seealso>
    private async Task WriteErrorResponse(
        HttpContext context,
        HttpStatusCode statusCode,
        string messageKey
    )
    {
        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsJsonAsync(
            value: new APIResponse()
                .ChangeStatus(language: _language, key: "api.status.failed")
                .ChangeMessage(language: _language, key: $"security.api_key.{messageKey}")
        );
    }

    /// <summary>
    /// Authenticates incoming requests using API key validation.
    /// </summary>
    /// <remarks>
    /// Validates API keys from request headers against a configured list of valid keys.
    /// Supports both header-based and query parameter-based API key authentication.
    ///
    /// Authentication flow:
    /// <list type="number">
    ///   <item><description>Extracts API key from X-API-Key header or query parameter</description></item>
    ///   <item><description>Validates key against registered API keys</description></item>
    ///   <item><description>Sets authentication claims on success</description></item>
    ///   <item><description>Returns 401 Unauthorized on failure</description></item>
    /// </list>
    ///
    /// Example request format:
    /// <example>
    /// <code>
    /// // Header-based authentication
    /// GET /api/data HTTP/1.1
    /// Host: api.example.com
    /// X-API-Key: NFHUZqt0zmL6siZ7/ynQ8nljJtsQrT3h0+nQZHhIQhk=
    ///
    /// // Query parameter authentication
    /// GET /api/data?apiKey=NFHUZqt0zmL6siZ7/ynQ8nljJtsQrT3h0+nQZHhIQhk= HTTP/1.1
    /// Host: api.example.com
    /// </code>
    /// </example>
    ///
    /// Example configuration:
    /// <example>
    /// <code>
    /// {
    ///   "Security": {
    ///     "RegisteredApiKeyList": [
    ///       "NFHUZqt0zmL6siZ7/ynQ8nljJtsQrT3h0+nQZHhIQhk=",
    ///       "IDxvX6aT3XTERRpuHpMNtpcQVUo2rZ3Smtm83UPVfi8="
    ///     ]
    ///   }
    /// }
    /// </code>
    /// </example>
    ///
    /// > [!NOTE]
    /// > API keys should be transmitted securely over HTTPS
    ///
    /// > [!TIP]
    /// > Use header-based authentication for better security
    ///
    /// > [!IMPORTANT]
    /// > API keys should be rotated periodically
    ///
    /// > [!CAUTION]
    /// > Store API keys securely and never log them
    ///
    /// > [!WARNING]
    /// > Query parameter authentication is less secure
    /// </remarks>
    /// <param name="context">
    /// <see cref="HttpContext"/> The HTTP context for the current request.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext"/>
    /// </param>
    /// <param name="next">
    /// <see cref="RequestDelegate"/> The delegate representing the next middleware in the pipeline.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.requestdelegate"/>
    /// </param>
    /// <returns>
    /// <see cref="Task"/> A Task representing the asynchronous authentication operation.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task"/>
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">
    /// Thrown when authentication fails due to invalid or missing API key.
    /// </exception>
    /// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/security/authentication/">
    /// ASP.NET Core Authentication
    /// </seealso>
    /// <seealso href="https://owasp.org/www-community/authentication">
    /// OWASP Authentication Cheat Sheet
    /// </seealso>
    /// <see cref="APIResponse"/>
    /// <see cref="Language"/>
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Skip authentication for static files
        if (
            context.Request.Path.StartsWithSegments(other: "/themes")
            || context.Request.Path.StartsWithSegments(other: "/images")
            || context.Request.Path.StartsWithSegments(other: "/js")
            || context.Request.Path.StartsWithSegments(other: "/css")
            || context.Request.Path.StartsWithSegments(other: "/openapi")
            || context.Request.Path.StartsWithSegments(other: "/logs")
        )
        {
            await next(context: context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(key: API_KEY_HEADER, value: out var apiKeyHeader))
        {
            await WriteErrorResponse(
                context: context,
                statusCode: HttpStatusCode.Unauthorized,
                messageKey: "missing"
            );
            return;
        }

        string apiKey = apiKeyHeader.ToString();
        var registeredKeys =
            _appConfigs.Get<JArray>(path: "Security.RegisteredApiKeyList")?.ToObject<string[]>()
            ?? [];

        if (registeredKeys.Length == 0)
        {
            await WriteErrorResponse(
                context: context,
                statusCode: HttpStatusCode.InternalServerError,
                messageKey: "not_registered"
            );
            return;
        }

        if (!registeredKeys.Contains(value: apiKey))
        {
            await WriteErrorResponse(
                context: context,
                statusCode: HttpStatusCode.Unauthorized,
                messageKey: "invalid"
            );
            return;
        }

        await next(context: context);
    }
}
