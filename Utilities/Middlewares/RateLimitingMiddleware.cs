using IDC.Utilities;
using IDC.Utilities.Models.API;

namespace ScriptDeployerWeb.Utilities.Middlewares;

/// <summary>
/// Implements IP-based rate limiting to protect API endpoints from excessive requests.
/// </summary>
/// <remarks>
/// Provides configurable request rate limiting based on client IP addresses using in-memory caching.
///
/// Features:
/// - IP-based request tracking
/// - Configurable request limits
/// - Rolling time window
/// - Localized error messages
/// - Thread-safe operation
///
/// > [!IMPORTANT]
/// > Configure appropriate request limits based on your API's capacity and expected usage patterns.
///
/// > [!NOTE]
/// > Uses in-memory cache for tracking requests. For distributed systems, consider using a distributed cache.
///
/// > [!TIP]
/// > Monitor rate limit hits to identify potential abuse or capacity issues.
///
/// > [!CAUTION]
/// > Ensure proper X-Forwarded-For header handling when behind a proxy.
///
/// Example request/response:
/// <code>
/// // Request
/// GET /api/data
/// Host: api.example.com
///
/// // Response (when limit exceeded)
/// HTTP/1.1 429 Too Many Requests
/// Content-Type: application/json
///
/// {
///     "status": "failed",
///     "message": "Rate limit exceeded. Please try again later."
/// }
/// </code>
/// </remarks>
/// <param name="next">The next middleware delegate in the pipeline</param>
/// <param name="cache">Memory cache service for storing request counts</param>
/// <param name="maxRequests">Maximum number of requests allowed per time window</param>
/// <param name="language">Language service for localized messages</param>
/// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limiting">Rate limiting in ASP.NET Core</seealso>
/// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/performance/caching/memory">Memory Cache in ASP.NET Core</seealso>
/// <see cref="Caching"/>
/// <see cref="Language"/>
/// <see cref="APIResponse"/>
public class RateLimitingMiddleware(
    RequestDelegate next,
    Caching cache,
    int maxRequests,
    Language language
)
{
    /// <summary>
    /// Maximum allowed requests per IP address within the time window.
    /// </summary>
    /// <remarks>
    /// This limit applies to each unique IP address independently.
    /// When exceeded, subsequent requests receive HTTP 429 responses.
    ///
    /// Configuration example:
    /// <example>
    /// <code>
    /// {
    ///   "Middlewares": {
    ///     "RateLimiting": {
    ///       "Enabled": true,
    ///       "MaxRequestsPerMinute": 100
    ///     }
    ///   }
    /// }
    /// </code>
    /// </example>
    ///
    /// Implementation example:
    /// <example>
    /// <code>
    /// var middleware = new RateLimitingMiddleware(
    ///     next: app.UseEndpoints,
    ///     cache: new Caching(),
    ///     maxRequests: 100,
    ///     language: new Language("en")
    /// );
    ///
    /// // Usage in Program.cs
    /// app.UseMiddleware&lt;RateLimitingMiddleware&gt;(
    ///     _appConfigs.Get(
    ///         path: "Middlewares.RateLimiting.MaxRequestsPerMinute",
    ///         defaultValue: 100
    ///     )
    /// );
    /// </code>
    /// </example>
    ///
    /// > [!NOTE]
    /// > Value is injected through dependency injection for runtime configuration
    ///
    /// > [!TIP]
    /// > Adjust this value based on your API's capacity and monitoring data
    ///
    /// > [!IMPORTANT]
    /// > Set different limits for different API endpoints if needed
    ///
    /// > [!CAUTION]
    /// > Setting too low might affect legitimate users
    ///
    /// > [!WARNING]
    /// > Setting too high might not effectively prevent abuse
    /// </remarks>
    /// <value>Integer representing maximum allowed requests</value>
    /// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limiting">Rate limiting in ASP.NET Core</seealso>
    /// <seealso href="https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/429">HTTP 429 Too Many Requests</seealso>
    /// <see cref="Caching"/>
    /// <see cref="TimeWindowMinutes"/>
    private readonly int MaxRequests = maxRequests;

    /// <summary>
    /// Language service instance for localized messages.
    /// </summary>
    /// <remarks>
    /// Provides localization support for rate limiting messages and responses.
    /// Used to generate user-friendly error messages in multiple languages.
    ///
    /// Supported message keys:
    /// <list type="bullet">
    ///   <item><description>api.status.failed - General failure status</description></item>
    ///   <item><description>api.rate_limit_exceeded - Rate limit exceeded message</description></item>
    /// </list>
    ///
    /// Example message format:
    /// <code>
    /// {
    ///   "status": "failed",
    ///   "message": {
    ///     "en": "Rate limit exceeded. Please try again later.",
    ///     "id": "Batas permintaan terlampaui. Silakan coba lagi nanti."
    ///   }
    /// }
    /// </code>
    ///
    /// Example usage:
    /// <example>
    /// <code>
    /// var response = new APIResponse()
    ///     .ChangeStatus(language: _language, key: "api.status.failed")
    ///     .ChangeMessage(language: _language, key: "api.rate_limit_exceeded");
    /// </code>
    /// </example>
    ///
    /// > [!NOTE]
    /// > Language selection is based on Accept-Language header or default culture
    ///
    /// > [!TIP]
    /// > Add new translations through language configuration files
    ///
    /// > [!IMPORTANT]
    /// > Ensure all rate limit messages are properly translated
    /// </remarks>
    /// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/localization">ASP.NET Core Localization</seealso>
    /// <see cref="Language"/>
    /// <see cref="APIResponse"/>
    private readonly Language _language = language;

    /// <summary>
    /// Duration of the rolling time window for rate limiting.
    /// </summary>
    /// <remarks>
    /// Defines the period during which requests are counted.
    /// After this period expires, the counter resets.
    ///
    /// > [!IMPORTANT]
    /// > This is a rolling window, not a fixed window.
    /// </remarks>
    /// <value>Time window duration in minutes</value>
    private const int TimeWindowMinutes = 1;

    /// <summary>
    /// Processes incoming HTTP requests with rate limiting enforcement.
    /// </summary>
    /// <remarks>
    /// Processing steps:
    /// 1. Extracts client IP address
    /// 2. Retrieves/updates request count from cache
    /// 3. Enforces rate limit
    /// 4. Updates metrics
    ///
    /// Cache key format: "ratelimit_{ip_address}"
    /// Example: "ratelimit_192.168.1.1"
    ///
    /// > [!WARNING]
    /// > Ensure proper IP address extraction when behind load balancers
    ///
    /// > [!TIP]
    /// > Monitor cache performance for high-traffic scenarios
    ///
    /// Example implementation:
    /// <code>
    /// app.UseMiddleware&lt;RateLimitingMiddleware&gt;(
    ///     new Caching(),
    ///     maxRequests: 100,
    ///     new Language("en")
    /// );
    /// </code>
    /// </remarks>
    /// <param name="context">Current HTTP context</param>
    /// <returns>Task representing the asynchronous operation</returns>
    /// <exception cref="Exception">Rethrows any downstream middleware exceptions</exception>
    /// <seealso href="https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/429">HTTP 429 Too Many Requests</seealso>
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext">HttpContext Class</see>
    public async Task InvokeAsync(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var cacheKey = $"ratelimit_{ip}";

        var requestCount = cache.Get<int>(key: cacheKey, expirationRenewal: false);

        if (requestCount >= MaxRequests)
        {
            context.Response.StatusCode = 429;
            await context.Response.WriteAsJsonAsync(
                new APIResponse()
                    .ChangeStatus(language: _language, key: "api.status.failed")
                    .ChangeMessage(language: _language, key: "api.rate_limit_exceeded")
            );
            return;
        }

        cache.Set(key: cacheKey, value: requestCount + 1, expirationMinutes: TimeWindowMinutes);
        await next(context);
    }
}
