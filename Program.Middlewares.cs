using ScriptDeployerWeb.Utilities.Middlewares;
using Microsoft.Extensions.FileProviders;

internal partial class Program
{
    /// <summary>
    /// Configures middleware pipeline for the application
    /// </summary>
    /// <param name="app">The web application instance</param>
    /// <remarks>
    /// Configures and orders middleware components based on appconfigs.jsonc settings:
    ///
    /// Example configuration:
    /// <code>
    /// {
    ///   "Middlewares": {
    ///     "RequestLogging": true,
    ///     "RateLimiting": {
    ///       "Enabled": true,
    ///       "MaxRequestsPerMinute": 100
    ///     },
    ///     "ResponseCompression": true,
    ///     "SecurityHeaders": {
    ///       "Enabled": true,
    ///       "EnableForHttp": true,
    ///       "EnableForHttps": true,
    ///       "EnableForAllEndpoints": true,
    ///       "Options": {
    ///         "X-Frame-Options": "DENY",
    ///         "X-Content-Type-Options": "nosniff"
    ///       }
    ///     },
    ///     "ApiKeyAuthentication": true
    ///   }
    /// }
    /// </code>
    ///
    /// > [!IMPORTANT]
    /// > Middleware order is crucial for proper request pipeline execution
    ///
    /// > [!NOTE]
    /// > API Key Authentication excludes Swagger UI, CSS, JS, themes, and image paths
    /// </remarks>
    /// <seealso cref="RequestLoggingMiddleware"/>
    /// <seealso cref="RateLimitingMiddleware"/>
    /// <seealso cref="ResponseCompressionMiddleware"/>
    /// <seealso cref="SecurityHeadersMiddleware"/>
    /// <seealso cref="ApiKeyAuthenticationMiddleware"/>
    /// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/">ASP.NET Core Middleware</seealso>
    private static void ConfigureMiddlewares(WebApplication app)
    {
        if (_appConfigs.Get(path: "Middlewares.RequestLogging", defaultValue: true))
            app.UseMiddleware<RequestLoggingMiddleware>();

        if (_appConfigs.Get(path: "Middlewares.RateLimiting.Enabled", defaultValue: true))
            app.UseMiddleware<RateLimitingMiddleware>(
                _appConfigs.Get(
                    path: "Middlewares.RateLimiting.MaxRequestsPerMinute",
                    defaultValue: 100
                )
            );

        if (_appConfigs.Get(path: "Middlewares.ResponseCompression", defaultValue: true))
            app.UseMiddleware<ResponseCompressionMiddleware>();

        if (_appConfigs.Get(path: "Middlewares.SecurityHeaders.Enabled", defaultValue: true))
            app.UseMiddleware<SecurityHeadersMiddleware>(
                _appConfigs.Get(
                    path: "Middlewares.SecurityHeaders.EnableForHttp",
                    defaultValue: true
                ),
                _appConfigs.Get(
                    path: "Middlewares.SecurityHeaders.EnableForHttps",
                    defaultValue: true
                ),
                _appConfigs.Get(
                    path: "Middlewares.SecurityHeaders.EnableForAllEndpoints",
                    defaultValue: true
                ),
                _appConfigs.Get(
                    path: "Middlewares.SecurityHeaders.Options",
                    defaultValue: new Dictionary<string, string>()
                )
            );

        if (_appConfigs.Get(path: "Middlewares.ApiKeyAuthentication", defaultValue: true))
        {
            app.UseWhen(
                predicate: context =>
                {
                    var path = context.Request.Path.Value?.ToLower();
                    return path?.StartsWith("/swagger") != true
                        && path?.StartsWith("/css") != true
                        && path?.StartsWith("/js") != true
                        && path?.StartsWith("/themes") != true
                        && path?.StartsWith("/images") != true;
                },
                configuration: appBuilder =>
                {
                    appBuilder.UseMiddleware<ApiKeyAuthenticationMiddleware>();
                }
            );
        }

        app.UseHttpsRedirection();
        ConfigureSwaggerUI(app: app);
        ConfigureStaticFiles(app: app);
        app.UseAuthorization();
        app.MapControllers();
    }

    /// <summary>
    /// Configures static file serving for the application
    /// </summary>
    /// <param name="app">The web application instance</param>
    /// <remarks>
    /// Sets up static file middleware to serve content from the wwwroot directory:
    /// - CSS files
    /// - JavaScript files
    /// - Images
    /// - Other static assets
    ///
    /// Example directory structure:
    /// <code>
    /// wwwroot/
    ///   ├── css/
    ///   ├── js/
    ///   ├── images/
    ///   └── themes/
    /// </code>
    ///
    /// > [!TIP]
    /// > Files are served from the root path (/) of the application
    /// </remarks>
    /// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/static-files">Static files in ASP.NET Core</seealso>
    private static void ConfigureStaticFiles(WebApplication app) =>
        app.UseStaticFiles(
            options: new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    root: Path.Combine(path1: Directory.GetCurrentDirectory(), path2: "wwwroot")
                ),
                RequestPath = "",
            }
        );
}
