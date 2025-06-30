using ScriptDeployerWeb.Utilities.DI;
using IDC.Utilities;

/// <summary>
/// Core program class containing dependency injection configurations
/// </summary>
/// <remarks>
/// This class handles the core DI setup including:
/// - Application configuration management
/// - Settings initialization
/// - Language/localization setup
///
/// Example usage:
/// <code>
/// var builder = WebApplication.CreateBuilder(args);
/// Program.ConfigureAppConfigs(builder);
/// Program.ConfigureAppSettings(builder);
/// Program.ConfigureLanguage(builder);
/// </code>
/// </remarks>
/// <seealso cref="AppConfigsHandler"/>
/// <seealso cref="AppSettingsHandler"/>
/// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection">Dependency injection in .NET</seealso>
internal partial class Program
{
    /// <summary>
    /// Configures and registers application configuration handler as a singleton service
    /// </summary>
    /// <param name="builder">The web application builder instance</param>
    /// <returns>Configured <see cref="AppConfigsHandler"/> instance</returns>
    /// <remarks>
    /// Loads application configurations from appconfigs.jsonc and registers them in the DI container.
    ///
    /// Example configuration file (appconfigs.jsonc):
    /// <code>
    /// {
    ///   "AppName": "MyApp",
    ///   "Language": "en",
    ///   "Logging": {
    ///     "LogLevel": {
    ///       "Default": "Information"
    ///     }
    ///   }
    /// }
    /// </code>
    /// </remarks>
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration">Configuration in .NET</seealso>
    private static AppConfigsHandler ConfigureAppConfigs(WebApplicationBuilder builder)
    {
        var appConfigs = AppConfigsHandler.Load();
        builder.Services.AddSingleton(_ => appConfigs);
        return appConfigs;
    }

    /// <summary>
    /// Configures and registers application settings handler as a singleton service
    /// </summary>
    /// <param name="builder">The web application builder instance</param>
    /// <returns>Configured <see cref="AppSettingsHandler"/> instance</returns>
    /// <remarks>
    /// Loads application settings from appsettings.json and registers them in the DI container.
    ///
    /// Example settings file (appsettings.json):
    /// <code>
    /// {
    ///   "AllowedHosts": "*",
    ///   "SwaggerList": [
    ///     {
    ///       "Name": "API Name",
    ///       "URL": "http://api.example.com/swagger/v1/swagger.json"
    ///     }
    ///   ]
    /// }
    /// </code>
    /// </remarks>
    /// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration">ASP.NET Core Configuration</seealso>
    private static AppSettingsHandler ConfigureAppSettings(WebApplicationBuilder builder)
    {
        var appSettings = AppSettingsHandler.Load();
        builder.Services.AddSingleton(_ => appSettings);
        return appSettings;
    }

    /// <summary>
    /// Configures and registers language service as a singleton
    /// </summary>
    /// <param name="builder">The web application builder instance</param>
    /// <remarks>
    /// Initializes language service using messages.json and configures default language from app configs.
    ///
    /// Example messages file (messages.json):
    /// <code>
    /// {
    ///   "en": {
    ///     "welcome": "Welcome to the application",
    ///     "error": "An error occurred"
    ///   },
    ///   "id": {
    ///     "welcome": "Selamat datang di aplikasi",
    ///     "error": "Terjadi kesalahan"
    ///   }
    /// }
    /// </code>
    ///
    /// > [!IMPORTANT]
    /// > The messages.json file must be located in the wwwroot directory
    /// </remarks>
    /// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/localization">Globalization and localization in ASP.NET Core</seealso>
    private static void ConfigureLanguage(WebApplicationBuilder builder)
    {
        _language = new Language(
            jsonPath: Path.Combine(
                path1: Directory.GetCurrentDirectory(),
                path2: "wwwroot/messages.json"
            ),
            defaultLanguage: _appConfigs.Get(path: "Language", defaultValue: "en")
        );

        builder.Services.AddSingleton(_ => _language);
    }
}
