namespace IDC.DBDeployTools.Utilities.Middlewares;

/// <summary>
/// Middleware for adding security headers to HTTP responses.
/// </summary>
/// <remarks>
/// Adds configurable security headers to HTTP responses to enhance application security.
/// Can be configured to apply headers selectively based on HTTP/HTTPS and endpoint paths.
///
/// Supported security headers:
/// <list type="bullet">
///     <item><description>X-Frame-Options</description></item>
///     <item><description>X-Content-Type-Options</description></item>
///     <item><description>X-XSS-Protection</description></item>
///     <item><description>Referrer-Policy</description></item>
///     <item><description>Content-Security-Policy</description></item>
///     <item><description>Permissions-Policy</description></item>
/// </list>
///
/// Example configurations:
/// <example>
/// <code>
/// var app = builder.Build();
///
/// app.UseMiddleware&lt;SecurityHeadersMiddleware&gt;(
///     enableForHttp: true,
///     enableForHttps: true,
///     enableForAllEndpoints: true, // Applies to all endpoints
///     options: new Dictionary&lt;string, string&gt;
///     {
///         ["X-Frame-Options"] = "DENY",
///         ["X-Content-Type-Options"] = "nosniff",
///         ["X-XSS-Protection"] = "1; mode=block",
///         ["Referrer-Policy"] = "strict-origin-when-cross-origin",
///         ["Content-Security-Policy"] = "default-src 'self'",
///         ["Permissions-Policy"] = "geolocation=(), camera=()"
///     }
/// );
/// </code>
///
/// Example with API-only endpoints:
/// <code>
/// app.UseMiddleware&lt;SecurityHeadersMiddleware&gt;(
///     enableForHttp: false,
///     enableForHttps: true,
///     enableForAllEndpoints: false, // Only applies to /api/* paths
///     options: new Dictionary&lt;string, string&gt;
///     {
///         ["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains"
///     }
/// );
/// </code>
/// </example>
///
/// > [!IMPORTANT]
/// > Security headers should be configured appropriately based on your application's requirements
///
/// > [!NOTE]
/// > When enableForAllEndpoints is false, headers are only applied to /api/* endpoints
///
/// > [!TIP]
/// > Use Content-Security-Policy to prevent XSS attacks
///
/// > [!WARNING]
/// > Incorrect security header configuration may break application functionality
///
/// > [!CAUTION]
/// > Some headers may not work as expected when served over HTTP
/// </remarks>
/// <param name="next">
/// <see cref="RequestDelegate"/> The next middleware in the pipeline.
/// <see href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.requestdelegate"/>
/// </param>
/// <param name="enableForHttp">
/// Determines if headers should be applied to HTTP requests.
/// </param>
/// <param name="enableForHttps">
/// Determines if headers should be applied to HTTPS requests.
/// </param>
/// <param name="enableForAllEndpoints">
/// Determines if headers should be applied to all endpoints or just /api/* endpoints.
/// </param>
/// <param name="options">
/// <see cref="IDictionary{TKey, TValue}"/> Dictionary of security headers and their values.
/// </param>
/// <seealso href="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers#security">
/// Security Headers Reference
/// </seealso>
/// <seealso href="https://owasp.org/www-project-secure-headers/">
/// OWASP Secure Headers Project
/// </seealso>
/// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl">
/// Enforce HTTPS in ASP.NET Core
/// </seealso>
public class SecurityHeadersMiddleware(
    RequestDelegate next,
    bool enableForHttp,
    bool enableForHttps,
    bool enableForAllEndpoints,
    IDictionary<string, string> options
)
{
    /// <summary>
    /// The next middleware delegate in the pipeline
    /// </summary>
    /// <see cref="RequestDelegate"/>
    private readonly RequestDelegate _next = next;

    /// <summary>
    /// Determines if security headers should be applied to HTTP requests
    /// </summary>
    private readonly bool _enableForHttp = enableForHttp;

    /// <summary>
    /// Determines if security headers should be applied to HTTPS requests
    /// </summary>
    private readonly bool _enableForHttps = enableForHttps;

    /// <summary>
    /// When false, headers are only added to /api/* paths
    /// </summary>
    private readonly bool _enableForAllEndpoints = enableForAllEndpoints;

    /// <summary>
    /// Dictionary of security header names and values to apply
    /// </summary>
    /// <remarks>
    /// Common security headers include:
    /// - X-Frame-Options
    /// - X-Content-Type-Options
    /// - X-XSS-Protection
    /// - Referrer-Policy
    /// - Content-Security-Policy
    /// - Permissions-Policy
    /// </remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers#security">MDN Security Headers Documentation</see>
    private readonly IDictionary<string, string> _options = options;

    /// <summary>
    /// Processes the HTTP request and adds configured security headers.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> instance representing the current HTTP request and response.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous middleware operation.</returns>
    /// <remarks>
    /// Evaluates the request context against configured conditions before applying security headers.
    /// Headers are added to the response before executing the next middleware in the pipeline.
    ///
    /// Example request format:
    /// <code>
    /// {
    ///   "headers": {
    ///     "X-Frame-Options": "DENY",
    ///     "X-Content-Type-Options": "nosniff",
    ///     "X-XSS-Protection": "1; mode=block",
    ///     "Referrer-Policy": "strict-origin-when-cross-origin",
    ///     "Content-Security-Policy": "default-src 'self'",
    ///     "Permissions-Policy": "geolocation=(), camera=()"
    ///   }
    /// }
    /// </code>
    ///
    /// Example usage:
    /// <example>
    /// <code>
    /// public class Startup
    /// {
    ///     public void Configure(IApplicationBuilder app)
    ///     {
    ///         app.UseMiddleware&lt;SecurityHeadersMiddleware&gt;(
    ///             enableForHttp: false,
    ///             enableForHttps: true,
    ///             enableForAllEndpoints: false,
    ///             options: new Dictionary&lt;string, string&gt;
    ///             {
    ///                 ["X-Frame-Options"] = "DENY",
    ///                 ["X-Content-Type-Options"] = "nosniff"
    ///             }
    ///         );
    ///     }
    /// }
    /// </code>
    /// </example>
    ///
    /// > [!NOTE]
    /// > Headers are only applied if the request matches the configured protocol and endpoint conditions
    ///
    /// > [!TIP]
    /// > Place this middleware early in the pipeline to ensure headers are added before response generation
    ///
    /// > [!IMPORTANT]
    /// > Security headers should be carefully configured based on your application's requirements
    ///
    /// > [!CAUTION]
    /// > Incorrect security header configuration may break application functionality
    /// </remarks>
    /// <seealso href="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers#security">MDN Security Headers Documentation</seealso>
    /// <seealso href="https://owasp.org/www-project-secure-headers/">OWASP Secure Headers Project</seealso>
    /// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/">ASP.NET Core Middleware</seealso>
    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldApplyHeaders(context: context))
            foreach (var option in _options)
                context.Response.Headers[option.Key] = option.Value;

        await _next(context: context);
    }

    /// <summary>
    /// Determines if security headers should be applied to the current request.
    /// </summary>
    /// <param name="context"><see cref="HttpContext"/> The HTTP context to evaluate.</param>
    /// <returns><see cref="bool"/> True if headers should be applied, false otherwise.</returns>
    /// <remarks>
    /// Evaluates the request against configured conditions to determine if security headers should be applied.
    /// The evaluation includes protocol checks (HTTP/HTTPS) and endpoint path validation.
    ///
    /// The method checks two main conditions:
    /// <list type="bullet">
    ///   <item><description>Request protocol (HTTP/HTTPS) against enabled protocols</description></item>
    ///   <item><description>Request path against endpoint configuration</description></item>
    /// </list>
    ///
    /// Example request format:
    /// <code>
    /// {
    ///   "request": {
    ///     "protocol": "HTTPS",
    ///     "path": "/api/users",
    ///     "headers": {
    ///       "Host": "api.example.com"
    ///     }
    ///   }
    /// }
    /// </code>
    ///
    /// Example usage:
    /// <example>
    /// <code>
    /// public class SecurityMiddleware
    /// {
    ///     public async Task InvokeAsync(HttpContext context)
    ///     {
    ///         if (ShouldApplyHeaders(context))
    ///         {
    ///             context.Response.Headers["X-Frame-Options"] = "DENY";
    ///             context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ///         }
    ///         await _next(context);
    ///     }
    /// }
    /// </code>
    /// </example>
    ///
    /// > [!NOTE]
    /// > The method uses configured values from middleware initialization
    ///
    /// > [!TIP]
    /// > Configure enableForAllEndpoints=false to apply headers only to API endpoints
    ///
    /// > [!IMPORTANT]
    /// > HTTPS validation is performed separately from endpoint path validation
    ///
    /// > [!CAUTION]
    /// > Ensure proper configuration to avoid unintended header application
    /// </remarks>
    /// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/">ASP.NET Core Middleware</seealso>
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.pathstring.startswithsegments">PathString.StartsWithSegments Method</seealso>
    /// <seealso href="https://owasp.org/www-project-secure-headers/">OWASP Secure Headers Project</seealso>
    private bool ShouldApplyHeaders(HttpContext context) =>
        (_enableForAllEndpoints || context.Request.Path.StartsWithSegments(other: "/api"))
        && (context.Request.IsHttps ? _enableForHttps : _enableForHttp);
}
