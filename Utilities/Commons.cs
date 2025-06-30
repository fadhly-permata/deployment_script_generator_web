namespace ScriptDeployerWeb.Utilities;

/// <summary>
/// Provides common utility methods and functionality for the application,
/// including environment checks, configuration validation, and utility functions.
/// </summary>
/// <remarks>
/// This static class contains helper methods that can be used across the application,
/// including environment checks, configuration validation, and utility functions.
///
/// Example request format:
/// <code>
/// // Environment check
/// if (Commons.IsDebugEnvironment())
/// {
///     // Enable debug-specific features
///     EnableDetailedLogging();
///     EnableDeveloperExceptions();
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
///         if (Commons.IsDebugEnvironment())
///         {
///             app.UseDeveloperExceptionPage();
///             app.UseSwagger();
///             app.UseSwaggerUI();
///         }
///         else
///         {
///             app.UseExceptionHandler("/Error");
///             app.UseHsts();
///         }
///     }
/// }
/// </code>
/// </example>
///
/// > [!NOTE]
/// > All methods in this class are thread-safe and can be called from any context
///
/// > [!TIP]
/// > Use this class for centralized utility functions that are commonly needed across the application
///
/// > [!IMPORTANT]
/// > Environment checks should be performed early in the application lifecycle
/// </remarks>
/// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/environments">ASP.NET Core Environments</seealso>
/// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/">Configuration in ASP.NET Core</seealso>
public static class Commons
{
    /// <summary>
    /// Checks if the application is running in debug mode.
    /// </summary>
    /// <returns>
    /// <see cref="bool"/> indicating whether the application is running in debug mode.
    /// </returns>
    /// <remarks>
    /// Checks if the ASPNETCORE_ENVIRONMENT variable is set to "Development".
    /// This method is case-insensitive when comparing environment values.
    ///
    /// Example request format:
    /// <code>
    /// {
    ///   "environmentVariables": {
    ///     "ASPNETCORE_ENVIRONMENT": "Development"
    ///   }
    /// }
    /// </code>
    ///
    /// Example usage:
    /// <example>
    /// <code>
    /// public class ErrorHandlingMiddleware
    /// {
    ///     public async Task InvokeAsync(HttpContext context)
    ///     {
    ///         try
    ///         {
    ///             await _next(context);
    ///         }
    ///         catch (Exception ex)
    ///         {
    ///             if (Commons.IsDebugEnvironment())
    ///             {
    ///                 // Show detailed error in development
    ///                 await HandleDevelopmentError(context, ex);
    ///             }
    ///             else
    ///             {
    ///                 // Show generic error in production
    ///                 await HandleProductionError(context);
    ///             }
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    ///
    /// > [!NOTE]
    /// > This method is thread-safe and can be called from any context
    ///
    /// > [!TIP]
    /// > Use this method to conditionally enable development-specific features
    ///
    /// > [!IMPORTANT]
    /// > Always ensure production environments have ASPNETCORE_ENVIRONMENT set appropriately
    /// </remarks>
    /// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/environments">ASP.NET Core Environments</seealso>
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.environment.getenvironmentvariable">Environment.GetEnvironmentVariable Method</seealso>
    public static bool IsDebugEnvironment() =>
        string.Equals(
            a: Environment.GetEnvironmentVariable(variable: "ASPNETCORE_ENVIRONMENT"),
            b: "Development",
            comparisonType: StringComparison.OrdinalIgnoreCase
        );

    /// <summary>
    /// Calculates the duration between two timestamps and returns a formatted string.
    /// </summary>
    /// <param name="startTime">The starting timestamp</param>
    /// <param name="endTime">The ending timestamp</param>
    /// <returns>A formatted string representing the duration between timestamps</returns>
    /// <remarks>
    /// This method calculates the time difference between two timestamps and formats it as a human-readable string
    /// showing days, hours, minutes, and seconds.
    ///
    /// Example usage:
    /// <example>
    /// <code>
    /// var start = DateTime.Parse("2023-01-01 10:00:00");
    /// var end = DateTime.Parse("2023-01-03 14:30:45");
    /// string duration = Commons.GetDurationFromTimestamp(start, end);
    /// // Result: "2 days 4 hours 30 minutes 45 seconds"
    /// </code>
    /// </example>
    ///
    /// > [!NOTE]
    /// > This method is based on the PostgreSQL function workflow.rpt_log_howlongv2
    ///
    /// > [!TIP]
    /// > Use this method for displaying human-readable durations in logs and reports
    /// </remarks>
    /// <seealso href="https://www.postgresql.org/docs/current/functions-datetime.html">PostgreSQL Date/Time Functions</seealso>
    public static string GetDurationFromTimestamp(DateTime startTime, DateTime endTime)
    {
        var timeSpan = endTime - startTime;

        int days = timeSpan.Days;
        int hours = timeSpan.Hours;
        int minutes = timeSpan.Minutes;
        double seconds = Math.Round(timeSpan.Seconds + (timeSpan.Milliseconds / 1000.0), 2);

        var durationParts = new List<string>();

        if (days != 0)
            durationParts.Add($"{days} days");

        if (hours != 0)
            durationParts.Add($"{hours} hours");

        if (minutes != 0)
            durationParts.Add($"{minutes} minutes");

        if (seconds != 0)
            durationParts.Add($"{seconds} seconds");

        return string.Join(" ", durationParts);
    }
}
