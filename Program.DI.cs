using IDC.DBDeployTools.Utilities.Middlewares;
using IDC.Utilities;
using IDC.Utilities.Comm.Http;

namespace IDC.DBDeployTools;

internal partial class Program
{
    /// <summary>
    /// Sets up dependency injection for the application services
    /// </summary>
    /// <param name="builder">The web application builder instance</param>
    /// <remarks>
    /// Configures and registers all required services in the dependency injection container:
    /// - Language/localization services
    /// - Swagger documentation
    /// - System logging
    /// - Caching mechanism
    /// - SQLite database
    /// - MongoDB database
    /// - API key authentication
    ///
    /// Example usage:
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// SetupDI(builder);
    /// var app = builder.Build();
    /// </code>
    ///
    /// > [!NOTE]
    /// > The order of configuration is important as some services may depend on others
    ///
    /// > [!IMPORTANT]
    /// > ApiKeyAuthenticationMiddleware is registered as scoped service to ensure proper request lifecycle management
    /// </remarks>
    /// <seealso cref="ApiKeyAuthenticationMiddleware"/>
    /// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection">Dependency injection in ASP.NET Core</seealso>
    /// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/security/authentication/">Authentication in ASP.NET Core</seealso>
    private static void SetupDI(WebApplicationBuilder builder)
    {
        ConfigureLanguage(builder: builder);
        ConfigureSwagger(builder: builder);
        ConfigureSystemLogging(builder: builder);
        ConfigureCaching(builder: builder);
        ConfigurePGSQL(builder: builder);
        ConfigureSQLite(builder: builder);
        ConfigureMongoDB(builder: builder);

        // Add this in your service configuration
        builder.Services.AddScoped<ApiKeyAuthenticationMiddleware>();

        // Add HttpClientUtility
        builder.Services.AddHttpClientUtility(
            systemLogging: builder
                .Services.BuildServiceProvider()
                .GetRequiredService<SystemLogging>()
        );
    }
}
