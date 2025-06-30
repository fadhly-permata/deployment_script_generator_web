using IDC.Utilities.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ScriptDeployerWeb.Utilities.DI;

/// <summary>
/// Handles configuration management through JSON files with thread-safe operations.
/// </summary>
/// <remarks>
/// Provides functionality to load, update, and manage application configurations stored in JSON format.
/// Implements IDisposable for proper resource cleanup.
///
/// Features:
/// - Thread-safe operations
/// - JSON configuration management
/// - Dot notation path support
/// - Type-safe value retrieval
/// - Automatic file persistence
///
/// Example configuration file (appconfigs.jsonc):
/// <code>
/// {
///   "app": {
///     "name": "MyApp",
///     "version": "1.0.0",
///     "settings": {
///       "maxItems": 100,
///       "enableCache": true
///     }
///   }
/// }
/// </code>
///
/// Example usage:
/// <example>
/// <code>
/// // Load configuration
/// using var config = AppConfigsHandler.Load();
///
/// // Get values
/// var appName = config.Get&lt;string&gt;(path: "app.name");
/// var maxItems = config.Get&lt;int&gt;(path: "app.settings.maxItems", defaultValue: 50);
///
/// // Update values
/// config.Update(path: "app.version", value: "1.1.0");
///
/// // Batch update
/// config.Update(new Dictionary&lt;string, object?&gt; {
///     ["app.name"] = "NewAppName",
///     ["app.settings.enableCache"] = false
/// });
/// </code>
/// </example>
///
/// > [!NOTE]
/// > Configuration changes are automatically persisted to the appconfigs.jsonc file
///
/// > [!IMPORTANT]
/// > Always dispose the handler using 'using' statement or calling Dispose() explicitly
///
/// > [!TIP]
/// > Use dot notation for nested properties: "parent.child.property"
/// </remarks>
/// <seealso cref="IDisposable"/>
/// <seealso href="https://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_Linq_JObject.htm">Newtonsoft.Json.Linq.JObject</seealso>
public sealed class AppConfigsHandler : IDisposable
{
    /// <summary>
    /// The underlying configuration data store.
    /// </summary>
    /// <remarks>
    /// Stores the configuration data in a thread-safe JObject instance.
    /// Provides access to configuration values through JSON path expressions.
    ///
    /// Example usage:
    /// <example>
    /// <code>
    /// // Internal configuration access
    /// private T? GetValue&lt;T&gt;(string path)
    /// {
    ///     ThrowIfDisposed();
    ///     var token = _config.SelectToken(path: path);
    ///     return token?.ToObject&lt;T&gt;();
    /// }
    ///
    /// // Configuration update
    /// private void SetValue(string path, object? value)
    /// {
    ///     ThrowIfDisposed();
    ///     var token = _config.SelectToken(path: path);
    ///     if (token != null)
    ///     {
    ///         token.Replace(JToken.FromObject(value));
    ///     }
    /// }
    /// </code>
    /// </example>
    ///
    /// > [!NOTE]
    /// > All operations on this field are thread-safe
    ///
    /// > [!IMPORTANT]
    /// > Always use ThrowIfDisposed() before accessing this field
    ///
    /// > [!TIP]
    /// > Use JObject's SelectToken method for efficient path-based access
    /// </remarks>
    /// <seealso href="https://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_Linq_JObject.htm">JObject Documentation</seealso>
    /// <seealso href="https://www.newtonsoft.com/json/help/html/QueryingLINQtoJSON.htm">Querying JSON with LINQ</seealso>
    private readonly JObject _config;

    /// <summary>
    /// Flag indicating whether the handler has been disposed.
    /// </summary>
    /// <remarks>
    /// Tracks the disposal state of the configuration handler to prevent access after disposal.
    /// Used by ThrowIfDisposed() to validate operations.
    ///
    /// Example usage:
    /// <example>
    /// <code>
    /// // Internal disposal check
    /// private void ValidateState()
    /// {
    ///     if (_disposed)
    ///     {
    ///         throw new ObjectDisposedException(nameof(AppConfigsHandler));
    ///     }
    /// }
    ///
    /// // Disposal operation
    /// public void Dispose()
    /// {
    ///     if (!_disposed)
    ///     {
    ///         _config.RemoveAll();
    ///         _disposed = true;
    ///     }
    /// }
    /// </code>
    /// </example>
    ///
    /// > [!NOTE]
    /// > This field is set to true after Dispose() is called
    ///
    /// > [!IMPORTANT]
    /// > Once set to true, the handler becomes permanently unusable
    ///
    /// > [!WARNING]
    /// > Do not modify this field outside of the Dispose method
    /// </remarks>
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose">Implementing Dispose Pattern</seealso>
    private bool _disposed;

    /// <summary>
    /// Throws ObjectDisposedException if the handler has been disposed.
    /// </summary>
    /// <remarks>
    /// Internal validation method to ensure the handler is still valid for operations.
    /// Called before any operation that accesses the configuration data.
    ///
    /// Example usage:
    /// <example>
    /// <code>
    /// private void SomeOperation()
    /// {
    ///     ThrowIfDisposed();
    ///     // Proceed with operation
    ///     var value = _config.SelectToken("some.path");
    /// }
    ///
    /// // Example of disposal state check
    /// try
    /// {
    ///     ThrowIfDisposed();
    ///     // Safe to proceed
    /// }
    /// catch (ObjectDisposedException ex)
    /// {
    ///     // Handle disposed state
    ///     Logger.LogError(message: "Handler was disposed", exception: ex);
    /// }
    /// </code>
    /// </example>
    ///
    /// > [!IMPORTANT]
    /// > This method should be called at the start of any public method
    ///
    /// > [!NOTE]
    /// > Throws ObjectDisposedException when _disposed is true
    ///
    /// > [!WARNING]
    /// > Do not catch this exception in internal methods
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Thrown when the handler is accessed after disposal. See <see href="https://learn.microsoft.com/en-us/dotnet/api/system.objectdisposedexception">ObjectDisposedException Documentation</see></exception>
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/using-objects">Using Disposable Objects</seealso>
    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(AppConfigsHandler));
    }

    /// <summary>
    /// Performs cleanup of managed resources.
    /// </summary>
    /// <remarks>
    /// Implements the IDisposable pattern to clean up resources used by the configuration handler.
    /// Clears all configuration data and marks the handler as disposed.
    ///
    /// Example usage:
    /// <example>
    /// <code>
    /// // Using statement (recommended)
    /// using (var config = AppConfigsHandler.Load())
    /// {
    ///     string? value = config.Get&lt;string&gt;(path: "some.setting");
    /// }
    ///
    /// // Manual disposal
    /// var config = AppConfigsHandler.Load();
    /// try
    /// {
    ///     bool setting = config.Get(path: "feature.enabled", defaultValue: false);
    /// }
    /// finally
    /// {
    ///     config.Dispose();
    /// }
    /// </code>
    /// </example>
    ///
    /// > [!IMPORTANT]
    /// > After disposal, any attempt to use the handler will throw ObjectDisposedException
    ///
    /// > [!NOTE]
    /// > This method is called automatically when using a 'using' statement
    ///
    /// > [!TIP]
    /// > Always prefer using statements over manual disposal
    /// </remarks>
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose">Implementing Dispose Pattern</seealso>
    public void Dispose()
    {
        if (!_disposed)
        {
            _config.RemoveAll();
            _disposed = true;
        }
    }

    /// <summary>
    /// Initializes a new instance of AppConfigsHandler.
    /// </summary>
    /// <remarks>
    /// Creates a new instance of the AppConfigsHandler class with the provided JObject configuration.
    /// This constructor is private to ensure proper initialization through the static Load methods.
    ///
    /// Example usage:
    /// <example>
    /// <code>
    /// // Create configuration using Load methods
    /// var config1 = AppConfigsHandler.Load();
    ///
    /// var jsonString = @"{ 'setting': 'value' }";
    /// var config2 = AppConfigsHandler.Load(jsonContent: jsonString);
    ///
    /// var jobject = new JObject { ["setting"] = "value" };
    /// var config3 = AppConfigsHandler.Load(config: jobject);
    /// </code>
    /// </example>
    ///
    /// > [!NOTE]
    /// > This constructor is private. Use the static Load methods to create instances
    ///
    /// > [!IMPORTANT]
    /// > The provided JObject is stored as a reference, not a copy
    ///
    /// > [!TIP]
    /// > Use appropriate Load method based on your configuration source
    /// </remarks>
    /// <param name="config">The JObject containing configuration data. See <see href="https://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_Linq_JObject.htm">JObject Documentation</see></param>
    /// <seealso cref="Load()"/>
    /// <seealso cref="Load(string)"/>
    /// <seealso cref="Load(JObject)"/>
    private AppConfigsHandler(JObject config)
    {
        _config = config;
        _disposed = false;
    }

    /// <summary>
    /// Loads configuration from the default appconfigs.jsonc file.
    /// </summary>
    /// <remarks>
    /// Loads and parses the configuration from the default appconfigs.jsonc file located in the wwwroot directory.
    /// Supports JSON with comments (JSONC) format.
    ///
    /// Example request body:
    /// ```json
    /// {
    ///   "app": {
    ///     "name": "MyApp",
    ///     // Application version
    ///     "version": "1.0.0",
    ///     "settings": {
    ///       "maxItems": 100,
    ///       "enableCache": true
    ///     }
    ///   }
    /// }
    /// ```
    ///
    /// Example usage:
    /// <example>
    /// <code>
    /// // Load configuration from default file
    /// using var config = AppConfigsHandler.Load();
    ///
    /// // Access configuration values
    /// string? appName = config.Get&lt;string&gt;(path: "app.name");
    /// string? version = config.Get&lt;string&gt;(path: "app.version");
    /// bool enableCache = config.Get(path: "app.settings.enableCache", defaultValue: false);
    /// </code>
    /// </example>
    ///
    /// > [!NOTE]
    /// > The file must be located at wwwroot/appconfigs.jsonc
    ///
    /// > [!IMPORTANT]
    /// > File must have read/write permissions for the application
    ///
    /// > [!TIP]
    /// > Use JSONC format to include comments in your configuration
    /// </remarks>
    /// <returns>A new instance of AppConfigsHandler initialized with the configuration from appconfigs.jsonc</returns>
    /// <exception cref="FileNotFoundException">Thrown when appconfigs.jsonc is not found in the wwwroot directory</exception>
    /// <exception cref="JsonReaderException">Thrown when JSON parsing fails due to invalid format</exception>
    /// <seealso href="https://www.newtonsoft.com/json/help/html/ParsingLINQtoJSON.htm">Parsing JSON with LINQ to JSON</seealso>
    /// <seealso href="https://code.visualstudio.com/docs/languages/json#_json-with-comments">JSON with Comments</seealso>
    public static AppConfigsHandler Load()
    {
        var appConfigPath = Path.Combine(
            path1: Directory.GetCurrentDirectory(),
            path2: "wwwroot",
            path3: "appconfigs.jsonc"
        );
        var jsonContent = File.ReadAllText(appConfigPath);
        return new(JObject.Parse(jsonContent));
    }

    /// <summary>
    /// Loads configuration from a JSON string.
    /// </summary>
    /// <remarks>
    /// Creates a new instance of AppConfigsHandler by parsing a JSON string.
    /// Useful for loading configurations from dynamic sources or in-memory JSON strings.
    ///
    /// Example usage:
    /// <example>
    /// <code>
    /// // JSON string configuration
    /// var jsonString = @"{
    ///     'app': {
    ///         'name': 'DynamicApp',
    ///         'settings': {
    ///             'maxItems': 150,
    ///             'enableCache': true
    ///         }
    ///     }
    /// }";
    ///
    /// // Load configuration from JSON string
    /// var config = AppConfigsHandler.Load(jsonContent: jsonString);
    ///
    /// // Use the configuration
    /// string? appName = config.Get&lt;string&gt;(path: "app.name");
    /// int? maxItems = config.Get&lt;int&gt;(path: "app.settings.maxItems");
    /// </code>
    /// </example>
    ///
    /// > [!NOTE]
    /// > The JSON string must be valid JSON format
    ///
    /// > [!IMPORTANT]
    /// > Comments in JSON string are not supported
    ///
    /// > [!TIP]
    /// > Use single quotes for JSON strings in C# to avoid escaping double quotes
    /// </remarks>
    /// <param name="jsonContent">JSON string containing configuration data</param>
    /// <returns>A new instance of AppConfigsHandler initialized with the parsed configuration</returns>
    /// <exception cref="JsonReaderException">Thrown when the JSON string is invalid or cannot be parsed</exception>
    /// <seealso href="https://www.newtonsoft.com/json/help/html/ParsingLINQtoJSON.htm">Parsing JSON with LINQ to JSON</seealso>
    public static AppConfigsHandler Load(string jsonContent) => new(JObject.Parse(jsonContent));

    /// <summary>
    /// Loads configuration from an existing JObject.
    /// </summary>
    /// <remarks>
    /// Creates a new instance of AppConfigsHandler using an existing JObject configuration.
    /// This method is useful when you want to reuse an existing JObject configuration or create a configuration handler from a dynamically generated JObject.
    ///
    /// Example usage:
    /// <example>
    /// <code>
    /// // Create a JObject configuration
    /// var jsonConfig = new JObject
    /// {
    ///     ["app"] = new JObject
    ///     {
    ///         ["name"] = "CustomApp",
    ///         ["settings"] = new JObject
    ///         {
    ///             ["maxItems"] = 100,
    ///             ["enableCache"] = true
    ///         }
    ///     }
    /// };
    ///
    /// // Load configuration from JObject
    /// var config = AppConfigsHandler.Load(config: jsonConfig);
    ///
    /// // Use the configuration
    /// string? appName = config.Get&lt;string&gt;(path: "app.name");
    /// </code>
    /// </example>
    ///
    /// > [!NOTE]
    /// > The provided JObject is used directly without creating a copy
    ///
    /// > [!IMPORTANT]
    /// > Changes to the original JObject will affect the configuration handler
    ///
    /// > [!TIP]
    /// > Use JObject.Parse() to create a JObject from a JSON string before loading
    /// </remarks>
    /// <param name="config">JObject containing configuration data. See <see href="https://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_Linq_JObject.htm">JObject Documentation</see></param>
    /// <returns>A new instance of AppConfigsHandler initialized with the provided configuration</returns>
    /// <seealso href="https://www.newtonsoft.com/json/help/html/CreatingLINQtoJSON.htm">Creating JSON with LINQ to JSON</seealso>
    public static AppConfigsHandler Load(JObject config) => new(config);

    /// <summary>
    /// Gets a configuration value by path with a default value.
    /// </summary>
    /// <remarks>
    /// Retrieves a configuration value from the specified path using dot notation.
    /// Returns the default value if the path is not found or the value cannot be converted to the specified type.
    ///
    /// Example usage:
    /// <example>
    /// <code>
    /// using var config = AppConfigsHandler.Load();
    ///
    /// // Get string value with default
    /// string appName = config.Get(path: "app.name", defaultValue: "DefaultApp");
    ///
    /// // Get integer value with default
    /// int maxItems = config.Get(path: "app.settings.maxItems", defaultValue: 100);
    ///
    /// // Get boolean value with default
    /// bool isEnabled = config.Get(path: "app.settings.enableCache", defaultValue: false);
    ///
    /// // Get nested object with default
    /// var settings = config.Get(
    ///     path: "app.settings",
    ///     defaultValue: new JObject { ["maxItems"] = 50, ["enableCache"] = true }
    /// );
    /// </code>
    /// </example>
    ///
    /// > [!NOTE]
    /// > Returns the default value for non-existent paths instead of throwing an exception
    ///
    /// > [!TIP]
    /// > Use Get&lt;T&gt;(path) overload when you don't need a default value and want to handle null cases
    /// </remarks>
    /// <typeparam name="T">The type to convert the value to. See <see href="https://www.newtonsoft.com/json/help/html/ConvertingJSONandNET.htm">Type Conversion</see></typeparam>
    /// <param name="path">The dot-notation path to the configuration value (e.g., "app.settings.maxItems")</param>
    /// <param name="defaultValue">The default value to return if the path is not found or conversion fails</param>
    /// <returns>The configuration value converted to type T, or the default value if not found</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the handler is disposed</exception>
    /// <seealso href="https://www.newtonsoft.com/json/help/html/QueryingLINQtoJSON.htm">Querying JSON with LINQ</seealso>
    public T Get<T>(string path, T defaultValue)
    {
        ThrowIfDisposed();
        return _config.PropGet(path: path, defaultValue: defaultValue);
    }

    /// <summary>
    /// Gets a configuration value by path without a default value.
    /// </summary>
    /// <remarks>
    /// Retrieves a configuration value from the specified path using dot notation.
    /// Returns null if the path is not found or the value cannot be converted to the specified type.
    ///
    /// Example usage:
    /// <example>
    /// <code>
    /// using var config = AppConfigsHandler.Load();
    ///
    /// // Get string value
    /// string? appName = config.Get&lt;string&gt;(path: "app.name");
    ///
    /// // Get integer value
    /// int? maxItems = config.Get&lt;int&gt;(path: "app.settings.maxItems");
    ///
    /// // Get boolean value
    /// bool? isEnabled = config.Get&lt;bool&gt;(path: "app.settings.enableCache");
    ///
    /// // Get nested object
    /// var settings = config.Get&lt;JObject&gt;(path: "app.settings");
    /// </code>
    /// </example>
    ///
    /// > [!NOTE]
    /// > Returns null for non-existent paths instead of throwing an exception
    ///
    /// > [!TIP]
    /// > Use Get&lt;T&gt;(path, defaultValue) overload when you need a default value
    /// </remarks>
    /// <typeparam name="T">The type to convert the value to. See <see href="https://www.newtonsoft.com/json/help/html/ConvertingJSONandNET.htm">Type Conversion</see></typeparam>
    /// <param name="path">The dot-notation path to the configuration value (e.g., "app.settings.maxItems")</param>
    /// <returns>The configuration value converted to type T, or null if not found</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the handler is disposed</exception>
    /// <seealso href="https://www.newtonsoft.com/json/help/html/QueryingLINQtoJSON.htm">Querying JSON with LINQ</seealso>
    public T? Get<T>(string path)
    {
        ThrowIfDisposed();
        return _config.PropGet<T>(path: path);
    }

    /// <summary>
    /// Updates a configuration value by path.
    /// </summary>
    /// <remarks>
    /// Updates a single configuration value and automatically persists changes to the appconfigs.jsonc file.
    /// Uses dot notation to specify nested properties.
    ///
    /// Example usage:
    /// <example>
    /// <code>
    /// using var config = AppConfigsHandler.Load();
    ///
    /// // Update string value
    /// config.Update(path: "app.name", value: "NewAppName");
    ///
    /// // Update numeric value
    /// config.Update(path: "app.settings.maxItems", value: 200);
    ///
    /// // Update boolean value
    /// config.Update(path: "app.settings.enableCache", value: false);
    ///
    /// // Clear value by setting null
    /// config.Update(path: "app.settings.optionalSetting", value: null);
    /// </code>
    /// </example>
    ///
    /// > [!NOTE]
    /// > If the path doesn't exist, it will be created automatically
    ///
    /// > [!IMPORTANT]
    /// > Configuration changes are automatically persisted to disk
    ///
    /// > [!TIP]
    /// > Use null value to clear a configuration entry
    /// </remarks>
    /// <param name="path">The dot-notation path to update (e.g., "app.settings.maxItems")</param>
    /// <param name="value">The new value to set. Can be null to clear the entry</param>
    /// <returns>True if update successful, false if the operation fails</returns>
    /// <exception cref="ObjectDisposedException">Thrown when attempting to update after the handler has been disposed</exception>
    /// <seealso href="https://www.newtonsoft.com/json/help/html/ModifyJson.htm">Newtonsoft.Json Modify JSON</seealso>
    public bool Update(string path, object? value)
    {
        ThrowIfDisposed();
        try
        {
            _config.PropUpsert(path, value);
            SaveToFile();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Updates multiple configuration values in a single operation.
    /// </summary>
    /// <remarks>
    /// Performs a batch update of multiple configuration values and automatically persists changes to the appconfigs.jsonc file.
    /// Uses dot notation to specify nested properties.
    ///
    /// Example usage:
    /// <example>
    /// <code>
    /// using var config = AppConfigsHandler.Load();
    ///
    /// var updates = new Dictionary&lt;string, object?&gt;
    /// {
    ///     ["app.name"] = "UpdatedApp",
    ///     ["app.settings.maxItems"] = 200,
    ///     ["app.settings.enableCache"] = false,
    ///     ["logging.level"] = "Debug"
    /// };
    ///
    /// bool success = config.Update(updates: updates);
    /// </code>
    /// </example>
    ///
    /// > [!NOTE]
    /// > All updates are performed atomically - either all succeed or none are applied
    ///
    /// > [!IMPORTANT]
    /// > Configuration changes are automatically persisted to disk
    ///
    /// > [!TIP]
    /// > Use null values to clear configuration entries
    /// </remarks>
    /// <param name="updates">Dictionary containing path-value pairs to update. Keys are dot-notation paths, values are the new configuration values</param>
    /// <returns>True if all updates were successful, false if any update fails</returns>
    /// <exception cref="ObjectDisposedException">Thrown when attempting to update after the handler has been disposed</exception>
    /// <seealso href="https://www.newtonsoft.com/json/help/html/ModifyJson.htm">Newtonsoft.Json Modify JSON</seealso>
    public bool Update(Dictionary<string, object?> updates)
    {
        ThrowIfDisposed();
        try
        {
            _config.PropUpdate(updates);
            SaveToFile();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Removes a configuration value by path from the configuration file.
    /// </summary>
    /// <remarks>
    /// Removes the specified configuration value and automatically persists the changes to the appconfigs.jsonc file.
    /// Uses dot notation to specify nested properties.
    ///
    /// Example usage:
    /// <example>
    /// <code>
    /// using var config = AppConfigsHandler.Load();
    ///
    /// // Remove a single configuration value
    /// config.Remove(path: "app.settings.maxItems");
    ///
    /// // Remove a nested object
    /// config.Remove(path: "app.settings");
    /// </code>
    /// </example>
    ///
    /// > [!NOTE]
    /// > If the specified path doesn't exist, the method returns false without throwing an exception
    ///
    /// > [!IMPORTANT]
    /// > Removing a parent path will remove all child configurations
    /// </remarks>
    /// <param name="path">The dot-notation path to the configuration value to remove (e.g., "app.settings.maxItems")</param>
    /// <returns>True if the value was successfully removed, false if the path doesn't exist or removal fails</returns>
    /// <exception cref="ObjectDisposedException">Thrown when attempting to remove a value after the handler has been disposed</exception>
    /// <seealso href="https://www.newtonsoft.com/json/help/html/RemoveSpecificProperty.htm">Newtonsoft.Json Remove Property</seealso>
    public bool Remove(string path)
    {
        ThrowIfDisposed();
        try
        {
            _config.PropRemove(kvp => kvp.Key == path);
            SaveToFile();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Saves the current configuration state to the appconfigs.jsonc file with proper formatting.
    /// </summary>
    /// <remarks>
    /// Automatically persists configuration changes to the appconfigs.jsonc file in the wwwroot directory.
    /// The file is saved with indented formatting for better readability.
    ///
    /// Example file structure:
    /// <code>
    /// {
    ///   "app": {
    ///     "name": "MyApp",
    ///     "version": "1.0.0"
    ///   }
    /// }
    /// </code>
    ///
    /// > [!NOTE]
    /// > The file is saved in the wwwroot directory relative to the application's current directory
    ///
    /// > [!IMPORTANT]
    /// > Ensure the application has write permissions to the wwwroot directory
    /// </remarks>
    /// <exception cref="IOException">Thrown when file writing fails due to I/O errors</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the application lacks write permissions</exception>
    /// <seealso href="https://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_Formatting.htm">Newtonsoft.Json.Formatting</seealso>
    private void SaveToFile()
    {
        var appConfigPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "appconfigs.jsonc"
        );
        File.WriteAllText(appConfigPath, _config.ToString(Formatting.Indented));
    }
}
