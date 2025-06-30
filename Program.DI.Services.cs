using IDC.Utilities;
using IDC.Utilities.Data;
using IDC.Utilities.Extensions;
using IDC.Utilities.Models.Data;
using MongoDB.Driver;

internal partial class Program
{
    /// <summary>
    /// Configures and registers system logging service
    /// </summary>
    /// <param name="builder">The web application builder instance</param>
    /// <remarks>
    /// Initializes system logging with configuration from appconfigs.jsonc:
    ///
    /// Example configuration:
    /// <code>
    /// {
    ///   "Logging": {
    ///     "LogDirectory": "logs",
    ///     "OSLogging": true,
    ///     "FileLogging": true,
    ///     "AutoCleanupOldLogs": true,
    ///     "MaxOldlogAge": 30,
    ///     "BaseDirectory": "",
    ///     "IncludeStackTrace": true
    ///   }
    /// }
    /// </code>
    ///
    /// > [!IMPORTANT]
    /// > Registered as a singleton to ensure consistent logging across the application
    /// </remarks>
    /// <seealso cref="SystemLogging"/>
    /// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/">Logging in .NET Core</seealso>
    private static void ConfigureSystemLogging(WebApplicationBuilder builder) =>
        builder.Services.AddSingleton(_ =>
        {
            return new SystemLogging(
                logDirectory: _appConfigs.Get(path: "Logging.LogDirectory", defaultValue: "logs"),
                source: _appConfigs.Get(path: "AppName", defaultValue: "IDC.Template"),
                enableOsLogging: _appConfigs.Get(path: "Logging.OSLogging", defaultValue: true),
                enableFileLogging: _appConfigs.Get(path: "Logging.FileLogging", defaultValue: true),
                autoCleanupOldLogs: _appConfigs.Get(
                    path: "Logging.AutoCleanupOldLogs",
                    defaultValue: true
                ),
                maxOldlogAge: _appConfigs.Get(path: "Logging.MaxOldlogAge", defaultValue: 30),
                baseDirectory: _appConfigs.Get(path: "Logging.BaseDirectory", defaultValue: ""),
                includeStackTrace: _appConfigs.Get(
                    path: "Logging.IncludeStackTrace",
                    defaultValue: true
                )
            );
        });

    /// <summary>
    /// Configures and registers caching service
    /// </summary>
    /// <param name="builder">The web application builder instance</param>
    /// <remarks>
    /// Sets up in-memory caching with configuration from appconfigs.jsonc:
    ///
    /// Example configuration:
    /// <code>
    /// {
    ///   "DependencyInjection": {
    ///     "Caching": {
    ///       "Enable": true,
    ///       "ExpirationInMinutes": 30
    ///     }
    ///   }
    /// }
    /// </code>
    ///
    /// > [!NOTE]
    /// > Caching service is optional and will only be registered if enabled in configuration
    /// </remarks>
    /// <seealso cref="Caching"/>
    /// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/performance/caching/memory">Memory caching in ASP.NET Core</seealso>
    private static void ConfigureCaching(WebApplicationBuilder builder)
    {
        if (_appConfigs.Get(path: "DependencyInjection.Caching.Enable", defaultValue: false))
            builder.Services.AddSingleton(static _ => new Caching(
                defaultExpirationMinutes: _appConfigs.Get(
                    path: "DependencyInjection.Caching.ExpirationInMinutes",
                    defaultValue: 30
                )
            ));
    }

    private static void ConfigurePGSQL(WebApplicationBuilder builder)
    {
        if (_appConfigs.Get(path: "DependencyInjection.PGSQL", defaultValue: false))
            builder.Services.AddScoped(static _ =>
            {
                string pass = _appSettings.Get(
                    path: "configPass.passwordDB",
                    defaultValue: string.Empty
                );
                string key = _appSettings.Get(
                    path: "KeyConvert.EncryptionKey",
                    defaultValue: string.Empty
                );

                if (pass != string.Empty && key != string.Empty)
                    pass = pass.LegacyDecryptor(key);

                var connString = new CommonConnectionString()
                    .FromConnectionString(
                        connectionString: _appSettings.Get(
                            path: $"DbContextSettings.{(_appSettings.Get(path: "DefaultConStrings.WFRunner.PGSQL", defaultValue: "ConnectionString_en"))}",
                            defaultValue: "Host=localhost;Port=5432;Database=mydb;Username=myuser;Password=mypassword"
                        )
                    )
                    .ChangePassword(newPassword: pass)
                    .ToPostgreSQL();

                return new PostgreHelper(
                    connectionString: connString,
                    logging: _appConfigs.Get(path: "Logging.AttachToDIObjects", defaultValue: true)
                        ? (SystemLogging?)_systemLogging
                        : null
                );
            });
    }

    /// <summary>
    /// Configures and registers SQLite database service
    /// </summary>
    /// <param name="builder">The web application builder instance</param>
    /// <remarks>
    /// Initializes SQLite connection with configuration from appsettings.json:
    ///
    /// Example configuration:
    /// <code>
    /// {
    ///   "SqLiteContextSettings": {
    ///     "memory": "Data Source=:memory:;Cache=Private;Mode=Memory",
    ///     "file": "Data Source=database.db;Cache=Shared;Mode=ReadWrite"
    ///   },
    ///   "DefaultConStrings": {
    ///     "SQLite": "memory"
    ///   }
    /// }
    /// </code>
    ///
    /// > [!TIP]
    /// > Use memory mode for testing and temporary data storage
    ///
    /// > [!WARNING]
    /// > Ensure proper file permissions when using file-based SQLite
    /// </remarks>
    /// <seealso cref="SQLiteHelper"/>
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/">SQLite Database Provider</seealso>
    private static void ConfigureSQLite(WebApplicationBuilder builder)
    {
        if (_appConfigs.Get(path: "DependencyInjection.SQLite", defaultValue: false))
            builder.Services.AddScoped(static _ =>
            {
                string defaultConString = _appSettings.Get(
                    path: "DefaultConStrings.WFRunner.SQLite",
                    defaultValue: "memory"
                );

                return defaultConString == "memory"
                    ? new SQLiteHelper(
                        logging: _appConfigs.Get(
                            path: "Logging.AttachToDIObjects",
                            defaultValue: true
                        )
                            ? (SystemLogging?)_systemLogging
                            : null
                    )
                    : new SQLiteHelper(
                        connectionString: new CommonConnectionString()
                            .FromConnectionString(
                                connectionString: _appSettings.Get(
                                    path: $"SqLiteContextSettings.{defaultConString}",
                                    defaultValue: "memory"
                                )
                            )
                            .ToSQLite(),
                        logging: _appConfigs.Get(
                            path: "Logging.AttachToDIObjects",
                            defaultValue: true
                        )
                            ? (SystemLogging?)_systemLogging
                            : null
                    );
            });
    }

    /// <summary>
    /// Configures and registers MongoDB database service
    /// </summary>
    /// <param name="builder">The web application builder instance</param>
    /// <remarks>
    /// Sets up MongoDB connection with configuration from appsettings.json:
    ///
    /// Example configuration:
    /// <code>
    /// {
    ///   "MongoDBSettings": {
    ///     "local": "mongodb://localhost:27017",
    ///     "production": "mongodb://user:password@host:port/database"
    ///   },
    ///   "DefaultConStrings": {
    ///     "MongoDB": "local"
    ///   }
    /// }
    /// </code>
    ///
    /// > [!IMPORTANT]
    /// > Connection timeout is set to 5 seconds by default
    ///
    /// > [!CAUTION]
    /// > Ensure proper security measures when storing connection strings
    /// </remarks>
    /// <seealso cref="IMongoDatabase"/>
    /// <seealso href="https://www.mongodb.com/docs/drivers/csharp/current/">MongoDB .NET Driver</seealso>
    private static void ConfigureMongoDB(WebApplicationBuilder builder)
    {
        if (_appConfigs.Get(path: "DependencyInjection.MongoDB", defaultValue: false))
            builder.Services.AddScoped(static _ =>
            {
                string defaultConString = _appSettings.Get(
                    path: "DefaultConStrings.WFRunner.MongoDB",
                    defaultValue: "local"
                );

                var settings = MongoClientSettings.FromConnectionString(
                    _appSettings.Get(
                        path: $"MongoDBSettings.{defaultConString}",
                        defaultValue: "mongodb://localhost:27017"
                    )
                );
                settings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);
                settings.ConnectTimeout = TimeSpan.FromSeconds(5);
                settings.SocketTimeout = TimeSpan.FromSeconds(5);
                settings.RetryWrites = false;
                // settings.DirectConnection = true;
                settings.DirectConnection = defaultConString != "withReplica";

                return new MongoClient(settings).GetDatabase(settings.ApplicationName ?? "IDC_EN");
            });
    }
}
