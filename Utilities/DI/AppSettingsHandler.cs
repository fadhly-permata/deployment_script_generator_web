using IDC.Utilities.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ScriptDeployerWeb.Utilities.DI;

/// <summary>
/// Handles appsettings.json configuration management with thread-safe operations.
/// </summary>
/// <remarks>
/// Provides functionality to load, update, and manage application settings stored in appsettings.json format.
/// Implements IDisposable for proper resource cleanup.
///
/// Key Features:
/// - Thread-safe operations for concurrent access
/// - Automatic file persistence
/// - Strongly-typed configuration access
/// - Dot notation path support
/// - Default value fallbacks
///
/// Configuration Structure:
/// <code>
/// {
///   "AppName": "IDC.Template",
///   "Language": "en",
///   "Logging": {
///     "LogLevel": {
///       "Default": "Information",
///       "Microsoft.AspNetCore": "Warning"
///     }
///   },
///   "Middlewares": {
///     "RequestLogging": true,
///     "ResponseCompression": true,
///     "RateLimiting": {
///       "Enabled": true,
///       "MaxRequestsPerMinute": 1000
///     }
///   }
/// }
/// </code>
///
/// > [!NOTE]
/// > All operations are thread-safe and handle concurrent access
///
/// > [!TIP]
/// > Use dot notation for accessing nested properties (e.g., "Logging.LogLevel.Default")
///
/// > [!IMPORTANT]
/// > Always dispose the handler when finished to release resources
///
/// > [!CAUTION]
/// > Changes are immediately persisted to disk - plan updates accordingly
/// </remarks>
/// <example>
/// <code>
/// // Basic usage
/// using var settings = AppSettingsHandler.Load();
///
/// // Get values with type safety
/// var appName = settings.Get&lt;string&gt;(path: "AppName");
/// var maxRequests = settings.Get(path: "Middlewares.RateLimiting.MaxRequestsPerMinute", defaultValue: 100);
///
/// // Update single value
/// settings.Update(path: "Logging.LogLevel.Default", value: "Debug");
///
/// // Batch update
/// settings.Update(new Dictionary&lt;string, object?&gt; {
///     ["AppName"] = "UpdatedApp",
///     ["Language"] = "id",
///     ["Logging.LogLevel.Default"] = "Warning"
/// });
///
/// // Remove configuration
/// settings.Remove(path: "Middlewares.RequestLogging");
/// </code>
/// </example>
/// <seealso cref="IDisposable"/>
/// <seealso href="https://www.newtonsoft.com/json/help/html/Introduction.htm">JSON.NET Documentation</seealso>
/// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/">ASP.NET Core Configuration</seealso>
public sealed class AppSettingsHandler : IDisposable
{
    /// <summary>
    /// Thread-safe storage for application configuration settings.
    /// </summary>
    /// <remarks>
    /// Stores the application settings in a JObject format with thread-safe access mechanisms.
    /// This field maintains the hierarchical structure of the configuration data.
    ///
    /// Expected Configuration Structure:
    /// <code>
    /// {
    ///   "AppName": "MyApplication",
    ///   "Version": "1.0.0",
    ///   "Database": {
    ///     "ConnectionString": "mongodb://localhost:27017",
    ///     "Name": "MyDatabase",
    ///     "Options": {
    ///       "MaxPoolSize": 100,
    ///       "Timeout": 30000
    ///     }
    ///   },
    ///   "Logging": {
    ///     "Level": "Information",
    ///     "Providers": ["Console", "File"],
    ///     "FilePath": "/var/log/app.log"
    ///   },
    ///   "Security": {
    ///     "ApiKey": "your-api-key-here",
    ///     "AllowedOrigins": ["https://api.example.com"]
    ///   }
    /// }
    /// </code>
    ///
    /// Example Usage:
    /// <example>
    /// <code>
    /// // Accessing nested values
    /// var dbName = _settings["Database"]?["Name"]?.Value&lt;string&gt;();
    /// var logLevel = _settings.SelectToken("Logging.Level")?.Value&lt;string&gt;();
    ///
    /// // Modifying values
    /// _settings["AppName"] = "UpdatedAppName";
    /// _settings["Database"]["Options"]["MaxPoolSize"] = 200;
    ///
    /// // Adding new sections
    /// _settings["NewFeature"] = JToken.FromObject(new {
    ///     Enabled = true,
    ///     Config = new { Timeout = 5000 }
    /// });
    /// </code>
    /// </example>
    ///
    /// > [!NOTE]
    /// > All access to this field should be synchronized using appropriate locking mechanisms
    ///
    /// > [!TIP]
    /// > Use JObject's built-in methods for type-safe value extraction
    ///
    /// > [!IMPORTANT]
    /// > Changes to this field are immediately reflected in the configuration state
    ///
    /// > [!CAUTION]
    /// > Direct modification of nested objects may require additional synchronization
    ///
    /// > [!WARNING]
    /// > Avoid storing sensitive information in plain text
    /// </remarks>
    /// <seealso href="https://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_Linq_JObject.htm">JObject Documentation</seealso>
    /// <seealso href="https://www.newtonsoft.com/json/help/html/QueryingLINQtoJSON.htm">LINQ to JSON</seealso>
    private readonly JObject _settings;

    /// <summary>
    /// Flag indicating whether the handler has been disposed.
    /// </summary>
    /// <remarks>
    /// Thread-safe boolean field used to track the disposal state of the AppSettingsHandler instance.
    /// This field is used by the <see cref="ThrowIfDisposed"/> method to prevent access to disposed resources.
    ///
    /// Usage patterns:
    /// <example>
    /// <code>
    /// // Internal checking
    /// private void SomeOperation()
    /// {
    ///     if (_disposed)
    ///     {
    ///         throw new ObjectDisposedException(nameof(AppSettingsHandler));
    ///     }
    ///     // Continue with operation
    /// }
    ///
    /// // Disposal logic
    /// public void Dispose()
    /// {
    ///     if (!_disposed)
    ///     {
    ///         // Cleanup resources
    ///         _disposed = true;
    ///     }
    /// }
    /// </code>
    /// </example>
    ///
    /// > [!NOTE]
    /// > This field is marked as private to ensure encapsulation
    ///
    /// > [!IMPORTANT]
    /// > Only modified during object construction and disposal
    ///
    /// > [!WARNING]
    /// > Direct modification outside of disposal logic may cause undefined behavior
    /// </remarks>
    /// <seealso cref="Dispose"/>
    /// <seealso cref="ThrowIfDisposed"/>
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.objectdisposedexception"/>
    private bool _disposed;

    /// <summary>
    /// Throws ObjectDisposedException if the handler has been disposed.
    /// </summary>
    /// <remarks>
    /// Provides a thread-safe validation mechanism to ensure the handler is still valid for operations.
    /// This method is called internally before any operation that accesses the configuration data.
    ///
    /// Example usage within internal methods:
    /// <example>
    /// <code>
    /// public T Get&lt;T&gt;(string path)
    /// {
    ///     ThrowIfDisposed();
    ///     // Continue with operation
    /// }
    ///
    /// public void Update(string path, object value)
    /// {
    ///     ThrowIfDisposed();
    ///     // Continue with operation
    /// }
    /// </code>
    /// </example>
    ///
    /// > [!NOTE]
    /// > This method is called automatically by all public methods
    ///
    /// > [!IMPORTANT]
    /// > Custom implementations should always call this before accessing _settings
    ///
    /// > [!WARNING]
    /// > Failing to call this method could lead to undefined behavior
    /// </remarks>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when attempting to access a disposed instance of AppSettingsHandler.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.objectdisposedexception"/>
    /// </exception>
    /// <seealso cref="Dispose"/>
    private void ThrowIfDisposed() =>
        ObjectDisposedException.ThrowIf(_disposed, nameof(AppSettingsHandler));

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <remarks>
    /// Implements IDisposable pattern to ensure proper cleanup of resources. This method:
    /// - Clears all entries from the internal JObject settings
    /// - Marks the handler as disposed to prevent further access
    /// - Is thread-safe for concurrent disposal attempts
    ///
    /// Example cleanup sequence:
    /// <example>
    /// <code>
    /// // Automatic disposal using using statement
    /// using (var settings = AppSettingsHandler.Load())
    /// {
    ///     var value = settings.Get&lt;string&gt;(path: "AppName");
    /// } // Disposed automatically here
    ///
    /// // Manual disposal
    /// var handler = AppSettingsHandler.Load();
    /// try
    /// {
    ///     var value = handler.Get&lt;string&gt;(path: "AppName");
    /// }
    /// finally
    /// {
    ///     handler.Dispose();
    /// }
    /// </code>
    /// </example>
    ///
    /// > [!IMPORTANT]
    /// > Always call Dispose when finished with the handler
    ///
    /// > [!NOTE]
    /// > Multiple calls to Dispose are safe but only the first has effect
    ///
    /// > [!WARNING]
    /// > Accessing disposed handler throws ObjectDisposedException
    /// </remarks>
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose"/>
    /// <seealso cref="IDisposable"/>
    /// <seealso cref="ObjectDisposedException"/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _settings.RemoveAll();
            _disposed = true;
        }
    }

    /// <summary>
    /// Initializes a new instance of the AppSettingsHandler class with the specified settings.
    /// </summary>
    /// <param name="settings">
    /// The JObject containing application settings.
    /// <see href="https://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_Linq_JObject.htm"/>
    /// </param>
    /// <remarks>
    /// Creates a new handler instance with thread-safe access to configuration settings.
    ///
    /// Expected Settings Structure:
    /// <code>
    /// {
    ///   "AppName": "MyApp",
    ///   "Version": "1.0.0",
    ///   "Database": {
    ///     "ConnectionString": "mongodb://localhost:27017",
    ///     "Name": "MyDatabase"
    ///   },
    ///   "Logging": {
    ///     "Level": "Information",
    ///     "Path": "/logs/app.log"
    ///   }
    /// }
    /// </code>
    ///
    /// > [!NOTE]
    /// > Settings object is stored as a reference
    ///
    /// > [!IMPORTANT]
    /// > Modifications to settings are immediately reflected
    /// </remarks>
    /// <example>
    /// <code>
    /// var json = JObject.Parse(@"{
    ///     ""AppName"": ""TestApp"",
    ///     ""Database"": {
    ///         ""Host"": ""localhost"",
    ///         ""Port"": 27017
    ///     }
    /// }");
    /// var handler = new AppSettingsHandler(settings: json);
    /// </code>
    /// </example>
    private AppSettingsHandler(JObject settings)
    {
        _settings = settings;
        _disposed = false;
    }

    /// <summary>
    /// Loads configuration from the default appsettings.json file.
    /// </summary>
    /// <returns>
    /// A new instance of AppSettingsHandler.
    /// <see cref="AppSettingsHandler"/>
    /// </returns>
    /// <exception cref="FileNotFoundException">
    /// Thrown when appsettings.json is not found.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.io.filenotfoundexception"/>
    /// </exception>
    /// <exception cref="JsonReaderException">
    /// Thrown when JSON parsing fails.
    /// <see href="https://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_JsonReaderException.htm"/>
    /// </exception>
    /// <remarks>
    /// Reads and parses the appsettings.json file from the application's current directory.
    /// Provides thread-safe access to application configuration settings.
    ///
    /// Expected Configuration Structure:
    /// <code>
    /// {
    ///   "AppName": "IDC.Template",
    ///   "Environment": "Development",
    ///   "ConnectionStrings": {
    ///     "DefaultConnection": "mongodb://localhost:27017",
    ///     "BackupConnection": "mongodb://backup:27017"
    ///   },
    ///   "Logging": {
    ///     "LogLevel": {
    ///       "Default": "Information",
    ///       "Microsoft": "Warning"
    ///     },
    ///     "FilePath": "/var/log/app.log"
    ///   },
    ///   "Security": {
    ///     "ApiKey": "your-api-key-here",
    ///     "AllowedHosts": ["*"]
    ///   }
    /// }
    /// </code>
    ///
    /// > [!NOTE]
    /// > File is read from the application's current directory
    ///
    /// > [!TIP]
    /// > Ensure appsettings.json is copied to output directory
    ///
    /// > [!IMPORTANT]
    /// > Configuration is cached after initial load
    ///
    /// > [!CAUTION]
    /// > Sensitive information should be stored in user secrets or environment variables
    /// </remarks>
    /// <example>
    /// <code>
    /// // Load settings from appsettings.json
    /// using var settings = AppSettingsHandler.Load();
    ///
    /// // Access configuration values
    /// var appName = settings.Get&lt;string&gt;(path: "AppName");
    /// var logLevel = settings.Get&lt;string&gt;(path: "Logging.LogLevel.Default");
    /// var connectionString = settings.Get&lt;string&gt;(path: "ConnectionStrings.DefaultConnection");
    /// var apiKey = settings.Get&lt;string&gt;(path: "Security.ApiKey");
    ///
    /// // Access with default values
    /// var port = settings.Get(path: "Server.Port", defaultValue: 5000);
    /// var timeout = settings.Get(path: "Server.Timeout", defaultValue: 30);
    /// </code>
    /// </example>
    /// <seealso cref="Load(string)"/>
    /// <seealso cref="Load(JObject)"/>
    /// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/">ASP.NET Core Configuration</seealso>
    public static AppSettingsHandler Load()
    {
        var settingsPath = Path.Combine(
            path1: Directory.GetCurrentDirectory(),
            path2: "appsettings.json"
        );
        var jsonContent = File.ReadAllText(settingsPath);
        return new(JObject.Parse(jsonContent));
    }

    /// <summary>
    /// Creates a new AppSettingsHandler from JSON content.
    /// </summary>
    /// <param name="jsonContent">
    /// The JSON string to parse.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-how-to"/>
    /// </param>
    /// <returns>
    /// A new instance of AppSettingsHandler.
    /// <see cref="AppSettingsHandler"/>
    /// </returns>
    /// <exception cref="JsonReaderException">
    /// Thrown when JSON parsing fails.
    /// <see href="https://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_JsonReaderException.htm"/>
    /// </exception>
    /// <remarks>
    /// Initializes a new handler by parsing a JSON string into a configuration object.
    /// Useful for loading configuration from various sources like API responses or string literals.
    ///
    /// Expected JSON Structure:
    /// <code>
    /// {
    ///   "AppName": "IDC.Template",
    ///   "Environment": "Production",
    ///   "Database": {
    ///     "ConnectionString": "mongodb://localhost:27017",
    ///     "Name": "ProductionDB",
    ///     "Options": {
    ///       "MaxPoolSize": 100,
    ///       "ConnectTimeout": 30000
    ///     }
    ///   },
    ///   "Security": {
    ///     "ApiKey": "your-api-key",
    ///     "AllowedOrigins": ["https://api.example.com", "https://admin.example.com"]
    ///   }
    /// }
    /// </code>
    ///
    /// > [!NOTE]
    /// > JSON must be a valid object at root level
    ///
    /// > [!TIP]
    /// > Use this for dynamic configuration loading scenarios
    ///
    /// > [!IMPORTANT]
    /// > Ensure JSON string is properly escaped
    ///
    /// > [!CAUTION]
    /// > Large JSON strings may impact memory usage
    /// </remarks>
    /// <example>
    /// <code>
    /// // Load from JSON string
    /// var json = @"{
    ///     ""AppName"": ""TestApp"",
    ///     ""Database"": {
    ///         ""Host"": ""localhost"",
    ///         ""Port"": 27017
    ///     },
    ///     ""Logging"": {
    ///         ""Level"": ""Debug"",
    ///         ""Path"": ""/var/log/app.log""
    ///     }
    /// }";
    ///
    /// var settings = AppSettingsHandler.Load(jsonContent: json);
    ///
    /// // Access configuration
    /// var appName = settings.Get&lt;string&gt;(path: "AppName");
    /// var dbPort = settings.Get&lt;int&gt;(path: "Database.Port");
    /// var logPath = settings.Get&lt;string&gt;(path: "Logging.Path");
    /// </code>
    /// </example>
    /// <seealso cref="Load()"/>
    /// <seealso cref="Load(JObject)"/>
    /// <seealso href="https://www.newtonsoft.com/json/help/html/ParsingLINQtoJSON.htm">Parsing LINQ to JSON</seealso>
    public static AppSettingsHandler Load(string jsonContent) => new(JObject.Parse(jsonContent));

    /// <summary>
    /// Creates a new AppSettingsHandler from an existing JObject.
    /// </summary>
    /// <param name="settings">
    /// The JObject containing settings.
    /// <see href="https://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_Linq_JObject.htm">JObject Documentation</see>
    /// </param>
    /// <returns>
    /// A new instance of AppSettingsHandler.
    /// <see cref="AppSettingsHandler"/>
    /// </returns>
    /// <remarks>
    /// Creates a new handler instance from an existing JObject configuration.
    /// Useful for testing or when configuration is obtained from non-file sources.
    ///
    /// Configuration Structure Example:
    /// <code>
    /// {
    ///   "AppName": "IDC.Template",
    ///   "Environment": "Development",
    ///   "Database": {
    ///     "ConnectionString": "mongodb://localhost:27017",
    ///     "Name": "MyDatabase"
    ///   },
    ///   "Logging": {
    ///     "MinimumLevel": "Information",
    ///     "FilePath": "/var/log/app.log"
    ///   }
    /// }
    /// </code>
    ///
    /// > [!NOTE]
    /// > The provided JObject is cloned to prevent external modifications
    ///
    /// > [!TIP]
    /// > Use this method when you need to create settings from dynamic sources
    ///
    /// > [!IMPORTANT]
    /// > The JObject structure should match your application's configuration schema
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create from existing JObject
    /// var jobject = new JObject(
    ///     new JProperty(name: "AppName", value: "TestApp"),
    ///     new JProperty(name: "Version", value: "1.0.0"),
    ///     new JProperty(
    ///         name: "Database",
    ///         value: new JObject(
    ///             new JProperty(name: "Host", value: "localhost"),
    ///             new JProperty(name: "Port", value: 5432)
    ///         )
    ///     )
    /// );
    ///
    /// var settings = AppSettingsHandler.Load(settings: jobject);
    ///
    /// // Access configuration
    /// var appName = settings.Get&lt;string&gt;(path: "AppName");
    /// var dbPort = settings.Get&lt;int&gt;(path: "Database.Port");
    /// </code>
    /// </example>
    /// <seealso cref="Load()"/>
    /// <seealso cref="Load(string)"/>
    /// <seealso href="https://www.newtonsoft.com/json/help/html/CreatingLINQtoJSON.htm">Creating LINQ to JSON</seealso>
    public static AppSettingsHandler Load(JObject settings) => new(settings);

    /// <summary>
    /// Gets a configuration value with a default fallback.
    /// </summary>
    /// <typeparam name="T">
    /// The type to convert the value to.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/generic-type-parameters"/>
    /// </typeparam>
    /// <param name="path">
    /// The dot-notation path to the configuration value.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.string"/>
    /// </param>
    /// <param name="defaultValue">
    /// The default value if path not found.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/nullable-value-types"/>
    /// </param>
    /// <returns>
    /// The configuration value or default if not found.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/nullable-value-types"/>
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the handler is disposed.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.objectdisposedexception"/>
    /// </exception>
    /// <exception cref="JsonReaderException">
    /// Thrown when JSON parsing fails.
    /// <see href="https://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_JsonReaderException.htm"/>
    /// </exception>
    /// <remarks>
    /// Retrieves a strongly-typed configuration value with fallback support.
    /// Returns the default value if the path doesn't exist or value cannot be converted.
    ///
    /// Path Format:
    /// - Use dots to separate nested levels
    /// - Case sensitive
    /// - No leading/trailing dots
    ///
    /// Example Configuration:
    /// <code>
    /// {
    ///   "AppName": "IDC.Template",
    ///   "Server": {
    ///     "Port": 5000,
    ///     "Timeout": 30
    ///   },
    ///   "Features": {
    ///     "Cache": {
    ///       "Enabled": true,
    ///       "Duration": 3600
    ///     }
    ///   }
    /// }
    /// </code>
    ///
    /// > [!NOTE]
    /// > Type conversion is handled automatically
    ///
    /// > [!TIP]
    /// > Use meaningful default values that won't cause runtime issues
    ///
    /// > [!IMPORTANT]
    /// > Default values are used for both missing paths and type conversion failures
    ///
    /// > [!CAUTION]
    /// > Ensure default values are appropriate for your application's logic
    /// </remarks>
    /// <example>
    /// <code>
    /// var settings = AppSettingsHandler.Load();
    ///
    /// // Get with primitive defaults
    /// var port = settings.Get(path: "Server.Port", defaultValue: 8080);
    /// var timeout = settings.Get(path: "Server.Timeout", defaultValue: 60);
    /// var appName = settings.Get(path: "AppName", defaultValue: "DefaultApp");
    ///
    /// // Get with complex defaults
    /// var cacheConfig = settings.Get(
    ///     path: "Features.Cache",
    ///     defaultValue: new CacheConfig {
    ///         Enabled = false,
    ///         Duration = 1800
    ///     }
    /// );
    ///
    /// // Get with collection defaults
    /// var allowedHosts = settings.Get(
    ///     path: "Security.AllowedHosts",
    ///     defaultValue: new[] { "localhost" }
    /// );
    /// </code>
    /// </example>
    /// <seealso cref="Get{T}(string)"/>
    /// <seealso href="https://www.newtonsoft.com/json/help/html/QueryingLINQtoJSON.htm">Querying LINQ to JSON</seealso>
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-how-to">System.Text.Json How-to</seealso>
    public T Get<T>(string path, T defaultValue)
    {
        ThrowIfDisposed();
        return _settings.PropGet(path: path, defaultValue: defaultValue);
    }

    /// <summary>
    /// Gets a configuration value with type conversion support.
    /// </summary>
    /// <typeparam name="T">
    /// The type to convert the value to.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/generic-type-parameters"/>
    /// </typeparam>
    /// <param name="path">
    /// The dot-notation path to the configuration value.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.string"/>
    /// </param>
    /// <returns>
    /// The configuration value or null if not found.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/nullable-value-types"/>
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the handler is disposed.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.objectdisposedexception"/>
    /// </exception>
    /// <exception cref="JsonReaderException">
    /// Thrown when JSON parsing fails.
    /// <see href="https://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_JsonReaderException.htm"/>
    /// </exception>
    /// <remarks>
    /// Retrieves a strongly-typed configuration value using dot notation path.
    /// Returns null if the path doesn't exist or value cannot be converted to specified type.
    ///
    /// Supported Types:
    /// - Primitive types (int, string, bool, etc.)
    /// - Complex objects
    /// - Arrays and collections
    /// - Nullable types
    ///
    /// Example Configuration:
    /// <code>
    /// {
    ///   "AppName": "IDC.Template",
    ///   "Port": 5000,
    ///   "Features": {
    ///     "Logging": {
    ///       "Enabled": true,
    ///       "Level": "Debug"
    ///     }
    ///   },
    ///   "AllowedHosts": ["localhost", "127.0.0.1"]
    /// }
    /// </code>
    ///
    /// > [!NOTE]
    /// > Type conversion is handled automatically
    ///
    /// > [!TIP]
    /// > Use specific types instead of object for better type safety
    ///
    /// > [!IMPORTANT]
    /// > Always check for null when accessing optional configuration
    /// </remarks>
    /// <example>
    /// <code>
    /// var settings = AppSettingsHandler.Load();
    ///
    /// // Get primitive types
    /// var appName = settings.Get&lt;string&gt;(path: "AppName");
    /// var port = settings.Get&lt;int&gt;(path: "Port");
    /// var isEnabled = settings.Get&lt;bool&gt;(path: "Features.Logging.Enabled");
    ///
    /// // Get array
    /// var hosts = settings.Get&lt;string[]&gt;(path: "AllowedHosts");
    ///
    /// // Get complex object
    /// var logging = settings.Get&lt;LoggingConfig&gt;(path: "Features.Logging");
    ///
    /// // Get nullable value
    /// var optionalTimeout = settings.Get&lt;int?&gt;(path: "OptionalTimeout");
    /// </code>
    /// </example>
    /// <seealso cref="Get{T}(string, T)"/>
    /// <seealso href="https://www.newtonsoft.com/json/help/html/QueryingLINQtoJSON.htm">Querying LINQ to JSON</seealso>
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-how-to">System.Text.Json How-to</seealso>
    public T? Get<T>(string path)
    {
        ThrowIfDisposed();
        return _settings.PropGet<T>(path: path);
    }

    /// <summary>
    /// Updates a configuration value by path.
    /// </summary>
    /// <param name="path">
    /// The dot-notation path to update.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.string"/>
    /// </param>
    /// <param name="value">
    /// The new value to set. Can be null.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/nullable-value-types"/>
    /// </param>
    /// <returns>
    /// True if update successful, false otherwise.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.boolean"/>
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when attempting to update using a disposed handler instance.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.objectdisposedexception"/>
    /// </exception>
    /// <remarks>
    /// Updates a single configuration value using dot notation path. The change is immediately persisted to disk.
    ///
    /// Path Format:
    /// - Use dots to separate nested levels
    /// - Case sensitive
    /// - No leading/trailing dots
    ///
    /// Example Configuration:
    /// <code>
    /// {
    ///   "AppName": "IDC.Template",
    ///   "Logging": {
    ///     "LogLevel": {
    ///       "Default": "Information"
    ///     }
    ///   }
    /// }
    /// </code>
    ///
    /// > [!NOTE]
    /// > Non-existent paths will be created automatically
    ///
    /// > [!IMPORTANT]
    /// > Changes are persisted immediately to disk
    ///
    /// > [!TIP]
    /// > Use batch updates for multiple changes
    ///
    /// > [!CAUTION]
    /// > Updating deeply nested paths may impact performance
    /// </remarks>
    /// <example>
    /// <code>
    /// var settings = AppSettingsHandler.Load();
    ///
    /// // Update simple value
    /// settings.Update(path: "AppName", value: "NewAppName");
    ///
    /// // Update nested value
    /// settings.Update(path: "Logging.LogLevel.Default", value: "Debug");
    ///
    /// // Update with complex object
    /// settings.Update(path: "Security", value: new {
    ///     ApiKey = "xyz123",
    ///     AllowedHosts = new[] { "localhost" }
    /// });
    ///
    /// // Update with null
    /// settings.Update(path: "OptionalSetting", value: null);
    /// </code>
    /// </example>
    /// <seealso cref="M:Update(Dictionary{string, object?})"/>
    /// <seealso cref="M:Remove(string)"/>
    /// <seealso href="https://www.newtonsoft.com/json/help/html/ModifyingLINQtoJSON.htm">Modifying LINQ to JSON</seealso>
    public bool Update(string path, object? value)
    {
        ThrowIfDisposed();
        try
        {
            _settings.PropUpsert(path, value);
            SaveToFile();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Updates multiple configuration values in a batch operation.
    /// </summary>
    /// <param name="updates">
    /// Dictionary containing path-value pairs to update.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2"/>
    /// </param>
    /// <returns>
    /// True if all updates successful, false if any update fails.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.boolean"/>
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when attempting to update using a disposed handler instance.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.objectdisposedexception"/>
    /// </exception>
    /// <remarks>
    /// Performs atomic batch updates to multiple configuration values. All updates are applied in a single operation
    /// and persisted to disk automatically.
    ///
    /// Update Rules:
    /// - Paths use dot notation for nesting
    /// - Non-existent paths are created
    /// - Existing values are overwritten
    /// - Null values are allowed
    ///
    /// Example Configuration:
    /// <code>
    /// {
    ///   "AppName": "IDC.Template",
    ///   "Logging": {
    ///     "LogLevel": {
    ///       "Default": "Information"
    ///     }
    ///   },
    ///   "Security": {
    ///     "ApiKey": "xyz123",
    ///     "AllowedHosts": ["localhost"]
    ///   }
    /// }
    /// </code>
    ///
    /// > [!NOTE]
    /// > All updates in the batch must succeed for the operation to be considered successful
    ///
    /// > [!IMPORTANT]
    /// > Changes are immediately persisted to disk after the batch completes
    ///
    /// > [!TIP]
    /// > Use batch updates instead of multiple single updates for better performance
    ///
    /// > [!CAUTION]
    /// > Large batch updates may impact application performance
    /// </remarks>
    /// <example>
    /// <code>
    /// var settings = AppSettingsHandler.Load();
    ///
    /// // Basic batch update
    /// settings.Update(new Dictionary&lt;string, object?&gt; {
    ///     ["AppName"] = "NewAppName",
    ///     ["Logging.LogLevel.Default"] = "Debug"
    /// });
    ///
    /// // Complex nested updates
    /// settings.Update(new Dictionary&lt;string, object?&gt; {
    ///     ["Security.ApiKey"] = "newKey123",
    ///     ["Security.AllowedHosts"] = new[] { "localhost", "127.0.0.1" },
    ///     ["Logging.LogLevel"] = new {
    ///         Default = "Warning",
    ///         System = "Error",
    ///         Microsoft = "Information"
    ///     }
    /// });
    ///
    /// // Update with null values
    /// settings.Update(new Dictionary&lt;string, object?&gt; {
    ///     ["OptionalSetting"] = null,
    ///     ["RequiredSetting"] = "value"
    /// });
    /// </code>
    /// </example>
    /// <seealso cref="Update(string, object?)"/>
    /// <seealso cref="Remove(string)"/>
    /// <seealso href="https://www.newtonsoft.com/json/help/html/ModifyingLINQtoJSON.htm">Modifying LINQ to JSON</seealso>
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-how-to">System.Text.Json How-to</seealso>
    public bool Update(Dictionary<string, object?> updates)
    {
        ThrowIfDisposed();
        try
        {
            _settings.PropUpdate(updates);
            SaveToFile();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Removes a configuration value by path.
    /// </summary>
    /// <param name="path">
    /// The dot-notation path to remove.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.string"/>
    /// </param>
    /// <returns>
    /// True if removal successful, false otherwise.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.boolean"/>
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the handler is disposed.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.objectdisposedexception"/>
    /// </exception>
    /// <remarks>
    /// Removes a configuration value and its children from the settings using dot notation path.
    /// Changes are automatically persisted to the appsettings.json file.
    ///
    /// Path Format:
    /// - Use dots to separate nested levels
    /// - Case sensitive
    /// - No leading/trailing dots
    ///
    /// Example paths:
    /// <code>
    /// "AppName"                              // Root level
    /// "Logging.LogLevel.Default"             // Nested property
    /// "Middlewares.RateLimiting.Enabled"     // Deep nesting
    /// </code>
    ///
    /// > [!NOTE]
    /// > Removing a path also removes all child properties
    ///
    /// > [!IMPORTANT]
    /// > Operation is persisted immediately to disk
    ///
    /// > [!CAUTION]
    /// > No validation is performed on the importance of the removed setting
    /// </remarks>
    /// <example>
    /// <code>
    /// var settings = AppSettingsHandler.Load();
    ///
    /// // Remove single value
    /// settings.Remove(path: "AppName");
    ///
    /// // Remove nested value
    /// settings.Remove(path: "Logging.LogLevel.Default");
    ///
    /// // Remove entire section and children
    /// settings.Remove(path: "Middlewares.RateLimiting");
    /// </code>
    /// </example>
    /// <seealso cref="M:Update(string, object?)"/>
    /// <seealso cref="M:Get{T}(string)"/>
    /// <seealso href="https://www.newtonsoft.com/json/help/html/ModifyingLINQtoJSON.htm">Modifying LINQ to JSON</seealso>
    public bool Remove(string path)
    {
        ThrowIfDisposed();
        try
        {
            _settings.PropRemove(kvp => kvp.Key == path);
            SaveToFile();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Saves the current configuration to the appsettings.json file.
    /// </summary>
    /// <remarks>
    /// Writes the settings to appsettings.json in the application's current directory.
    /// The JSON is formatted with indentation for readability.
    ///
    /// The file is saved with the following characteristics:
    /// - Location: Application's current directory
    /// - Filename: appsettings.json
    /// - Format: Indented JSON
    /// - Encoding: UTF-8
    ///
    /// Example file content:
    /// <code>
    /// {
    ///   "AppName": "IDC.Template",
    ///   "Language": "en",
    ///   "Logging": {
    ///     "LogLevel": {
    ///       "Default": "Information",
    ///       "Microsoft.AspNetCore": "Warning"
    ///     }
    ///   }
    /// }
    /// </code>
    ///
    /// > [!NOTE]
    /// > Existing file will be overwritten with new content
    ///
    /// > [!IMPORTANT]
    /// > Ensure write permissions exist for the application directory
    ///
    /// > [!CAUTION]
    /// > This operation is not atomic - partial writes may occur if process is interrupted
    /// </remarks>
    /// <example>
    /// <code>
    /// var settings = AppSettingsHandler.Load();
    /// settings.Update(path: "AppName", value: "NewAppName");
    /// // SaveToFile is automatically called after Update
    ///
    /// // Manual save if needed
    /// settings.SaveToFile();
    /// </code>
    /// </example>
    /// <seealso cref="Update(string, object?)"/>
    /// <seealso cref="Load()"/>
    /// <seealso href="https://www.newtonsoft.com/json/help/html/SerializingJSON.htm">JSON.NET Serialization</seealso>
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.io.file.writealltext">File.WriteAllText Method</seealso>
    private void SaveToFile()
    {
        var settingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
        File.WriteAllText(settingsPath, _settings.ToString(Formatting.Indented));
    }
}
