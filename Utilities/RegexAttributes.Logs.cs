using System.Text.RegularExpressions;

namespace ScriptDeployerWeb.Utilities;

/// <summary>
/// Contains regex patterns for parsing log entries.
/// </summary>
/// <remarks>
/// Provides compiled regular expressions for splitting and parsing different types of log entries.
///
/// Example request format:
/// <code>
/// // Simple log format
/// [2024-01-01 12:00:00] [INFO] User logged in successfully
/// [2024-01-01 12:01:00] [ERROR] Failed to connect to database
///
/// // Detailed log format with exception
/// [2024-01-01 12:00:00] [ERROR] Type: System.InvalidOperationException
/// Message: Operation cannot be completed
/// StackTrace:
///    --> at ProcessData() in DataProcessor.cs:line 45
///    --> at ExecuteOperation() in Operations.cs:line 23
/// </code>
///
/// Example usage:
/// <example>
/// <code>
/// // Split log file into entries
/// var logContent = File.ReadAllText("app.log");
/// var entries = RegexAttributes.LogEntrySplitter()
///     .Split(logContent)
///     .Where(entry => !string.IsNullOrWhiteSpace(entry));
///
/// // Parse each entry
/// foreach (var entry in entries)
/// {
///     var simpleMatch = RegexAttributes.SimpleLogEntry().Match(entry);
///     if (simpleMatch.Success)
///     {
///         ProcessSimpleLog(simpleMatch);
///         continue;
///     }
///
///     var detailedMatch = RegexAttributes.DetailedLogEntry().Match(entry);
///     if (detailedMatch.Success)
///     {
///         ProcessDetailedLog(detailedMatch);
///     }
/// }
/// </code>
/// </example>
///
/// > [!NOTE]
/// > All regex patterns are compiled for optimal performance
///
/// > [!TIP]
/// > Use LogEntrySplitter() first to break log file into individual entries
///
/// > [!IMPORTANT]
/// > Ensure log entries follow the exact format specifications
///
/// > [!CAUTION]
/// > Large log files should be processed in chunks to manage memory usage
///
/// > [!WARNING]
/// > Invalid log formats may cause parsing failures
/// </remarks>
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex">Regex Class</seealso>
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expressions">Regular Expressions in .NET</seealso>
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference">Regex Language Reference</seealso>
public static partial class RegexAttributes
{
    /// <summary>
    /// Splits log content into individual entries based on timestamp pattern.
    /// </summary>
    /// <remarks>
    /// Pattern matches the start of each log entry by looking for timestamp in format [YYYY-MM-DD HH:mm:ss].
    /// Uses positive lookahead to preserve the timestamp in the split result.
    ///
    /// Example request format:
    /// <code>
    /// [2024-01-01 12:00:00] [INFO] First log entry
    /// [2024-01-01 12:01:00] [ERROR] Second log entry
    /// [2024-01-01 12:02:00] [WARN] Third log entry with
    /// multiple lines of content
    /// [2024-01-01 12:03:00] [DEBUG] Fourth log entry
    /// </code>
    ///
    /// Example usage:
    /// <example>
    /// <code>
    /// var logContent = File.ReadAllText("app.log");
    /// var entries = RegexAttributes.LogEntrySplitter()
    ///     .Split(logContent)
    ///     .Where(entry => !string.IsNullOrWhiteSpace(entry));
    ///
    /// foreach (var entry in entries)
    /// {
    ///     // Process each log entry
    ///     Console.WriteLine($"Processing entry: {entry}");
    /// }
    /// </code>
    /// </example>
    ///
    /// > [!NOTE]
    /// > The pattern preserves the timestamp in each split entry
    ///
    /// > [!TIP]
    /// > Use with SimpleLogEntry() or DetailedLogEntry() to parse individual entries
    ///
    /// > [!IMPORTANT]
    /// > Ensure log entries start with properly formatted timestamps
    /// </remarks>
    /// <returns>A compiled regex pattern for splitting log content into individual entries</returns>
    /// <seealso cref="SimpleLogEntry"/>
    /// <seealso cref="DetailedLogEntry"/>
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.split">Regex.Split Method</seealso>
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expressions">Regular Expressions in .NET</seealso>
    [GeneratedRegex(@"(?=\[\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2}\])", RegexOptions.Singleline)]
    public static partial Regex LogEntrySplitter();

    /// <summary>
    /// Parses simple log entries with timestamp, level and message components.
    /// </summary>
    /// <remarks>
    /// Pattern captures three groups in a single-line log entry format:
    /// - Group 1: Timestamp in [YYYY-MM-DD HH:mm:ss] format
    /// - Group 2: Log level in [LEVEL] format
    /// - Group 3: Message content
    ///
    /// Example request format:
    /// ```
    /// [2024-01-01 12:00:00] [INFO] User logged in successfully
    /// [2024-01-01 12:01:00] [ERROR] Failed to connect to database
    /// [2024-01-01 12:02:00] [WARN] High memory usage detected
    /// ```
    ///
    /// Example usage:
    /// <example>
    /// <code>
    /// var logLine = "[2024-01-01 12:00:00] [INFO] User logged in";
    /// var match = RegexAttributes.SimpleLogEntry().Match(logLine);
    ///
    /// if (match.Success)
    /// {
    ///     var timestamp = match.Groups[1].Value;  // "2024-01-01 12:00:00"
    ///     var level = match.Groups[2].Value;      // "INFO"
    ///     var message = match.Groups[3].Value;    // "User logged in"
    /// }
    /// </code>
    /// </example>
    ///
    /// > [!NOTE]
    /// > The pattern uses start (^) and end ($) anchors to ensure full line matching
    ///
    /// > [!TIP]
    /// > Use non-greedy quantifiers (.*?) to properly handle messages containing brackets
    ///
    /// > [!IMPORTANT]
    /// > Ensure log entries strictly follow the [Timestamp] [Level] Message format
    /// </remarks>
    /// <returns>A compiled regex pattern for parsing simple log entries</returns>
    /// <seealso cref="LogEntrySplitter"/>
    /// <seealso cref="DetailedLogEntry"/>
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex">Regex Class</seealso>
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference">Regular Expression Language Reference</seealso>
    [GeneratedRegex(@"^\[(.*?)\] \[(.*?)\] (.+)$", RegexOptions.Singleline)]
    public static partial Regex SimpleLogEntry();

    /// <summary>
    /// Parses detailed log entries containing exception information with timestamp, level, type, message and stack trace.
    /// </summary>
    /// <remarks>
    /// Extracts structured information from detailed log entries using named capture groups.
    ///
    /// The pattern captures five essential components:
    /// - Timestamp: [YYYY-MM-DD HH:mm:ss]
    /// - Log Level: [ERROR|WARN|INFO|DEBUG]
    /// - Exception Type: Fully qualified type name
    /// - Exception Message: Error description
    /// - Stack Trace: Multi-line call stack with indentation
    ///
    /// Example usage:
    /// <example>
    /// <code>
    /// var logEntry = @"[2024-01-01 12:00:00] [ERROR] Type: System.InvalidOperationException
    ///                  Message: The operation cannot be completed
    ///                  StackTrace:
    ///                     --> at ProcessData() in DataProcessor.cs:line 45
    ///                     --> at ExecuteOperation() in Operations.cs:line 23";
    ///
    /// var match = RegexAttributes.DetailedLogEntry().Match(logEntry);
    /// if (match.Success)
    /// {
    ///     var timestamp = match.Groups[1].Value;    // "2024-01-01 12:00:00"
    ///     var level = match.Groups[2].Value;        // "ERROR"
    ///     var type = match.Groups[3].Value;         // "System.InvalidOperationException"
    ///     var message = match.Groups[4].Value;      // "The operation cannot be completed"
    ///     var stackTrace = match.Groups[5].Value;   // Full stack trace with line breaks
    /// }
    /// </code>
    /// </example>
    ///
    /// > [!NOTE]
    /// > The pattern uses single-line mode to match across line breaks
    ///
    /// > [!TIP]
    /// > Stack trace lines are always prefixed with "   --> " for consistent parsing
    ///
    /// > [!IMPORTANT]
    /// > Ensure log entries follow the exact format for successful matching
    /// </remarks>
    /// <returns>A compiled regex pattern optimized for parsing detailed log entries</returns>
    /// <seealso cref="LogEntrySplitter"/>
    /// <seealso cref="SimpleLogEntry"/>
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.match">Regex.Match Method</seealso>
    [GeneratedRegex(
        @"\[(.*?)\] \[(.*?)\] Type: (.*?)[\r\n]+Message: (.*?)[\r\n]+StackTrace:[\r\n]+((?:   --> .*(?:\r?\n|$))*)",
        RegexOptions.Singleline
    )]
    public static partial Regex DetailedLogEntry();
}
