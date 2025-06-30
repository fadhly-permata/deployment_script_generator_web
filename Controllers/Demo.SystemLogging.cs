using System.Data;
using IDC.Utilities;
using IDC.Utilities.Models.API;
using ScriptDeployerWeb.Utilities;
using ScriptDeployerWeb.Utilities.DI;
using ScriptDeployerWeb.Utilities.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace ScriptDeployerWeb.Controllers;

/// <summary>
/// Controller for managing system logging operations
/// </summary>
/// <remarks>
/// Provides endpoints for writing logs at different levels (Info, Warning, Error) and retrieving log information.
/// Supports operations like viewing log files and reading log entries within a specified time range.
/// </remarks>
/// <example>
/// Basic usage:
/// <code>
/// var controller = new DemoControllerSystemLogging(
///     appConfigs: new AppConfigsHandler(),
///     language: new Language(),
///     systemLogging: new SystemLogging()
/// );
/// </code>
/// </example>
/// <param name="appConfigs">Configuration handler for accessing application settings</param>
/// <param name="language">Service for handling language and localization</param>
/// <param name="systemLogging">Service for system-wide logging operations</param>
[Route("api/demo/[controller]")]
[ApiController]
[ApiExplorerSettings(GroupName = "Demo")]
public class DemoSystemLogging(
    AppConfigsHandler appConfigs,
    Language language,
    SystemLogging systemLogging
) : ControllerBase
{
    /// <summary>
    /// Write logs an information message
    /// </summary>
    /// <param name="message">Message to log</param>
    [Tags(tags: "System Logging"), HttpPost(template: "Info")]
    public APIResponse LogInfo([FromBody] string message)
    {
        try
        {
            throw new DataException(s: message);
        }
        catch (Exception ex)
        {
            return new APIResponse()
                .ChangeStatus(language: language, key: "api.status.failed")
                .ChangeMessage(
                    exception: ex,
                    logging: systemLogging,
                    includeStackTrace: Commons.IsDebugEnvironment()
                );
        }
    }

    /// <summary>
    /// Write logs a warning message
    /// </summary>
    /// <param name="message">Message to log</param>
    [Tags(tags: "System Logging"), HttpPost(template: "Warning")]
    public APIResponse LogWarning([FromBody] string message)
    {
        try
        {
            throw new DataException(s: message);
        }
        catch (Exception ex)
        {
            return new APIResponse()
                .ChangeStatus(language: language, key: "api.status.failed")
                .ChangeMessage(
                    exception: ex,
                    logging: systemLogging,
                    includeStackTrace: Commons.IsDebugEnvironment()
                );
        }
    }

    /// <summary>
    /// Write logs an error message
    /// </summary>
    /// <param name="message">Message to log</param>
    [Tags(tags: "System Logging"), HttpPost(template: "Error")]
    public APIResponse LogError([FromBody] string message)
    {
        try
        {
            throw new DataException(s: message);
        }
        catch (Exception ex)
        {
            return new APIResponse()
                .ChangeStatus(language: language, key: "api.status.failed")
                .ChangeMessage(
                    exception: ex,
                    logging: systemLogging,
                    includeStackTrace: Commons.IsDebugEnvironment()
                );
        }
    }

    /// <summary>
    /// Write logs an error with exception details
    /// </summary>
    /// <param name="message">Message to log</param>
    [Tags(tags: "System Logging"), HttpPost(template: "ErrorWithException")]
    public APIResponse LogErrorWithException([FromBody] string message)
    {
        try
        {
            throw new DataException(s: message);
        }
        catch (Exception ex)
        {
            return new APIResponse()
                .ChangeStatus(language: language, key: "api.status.failed")
                .ChangeMessage(
                    exception: ex,
                    logging: systemLogging,
                    includeStackTrace: Commons.IsDebugEnvironment()
                );
        }
    }

    /// <summary>
    /// Gets list of log files from configured directory
    /// </summary>
    /// <returns>List of log files with their details</returns>
    [Tags(tags: "System Logging"), HttpGet(template: "Files")]
    public APIResponseData<List<object>> GetLogFiles()
    {
        try
        {
            var fullPath = SystemLoggingLogic.GetFullLogPath(
                baseDirectory: appConfigs.Get(
                    path: "Logging.baseDirectory",
                    defaultValue: Directory.GetCurrentDirectory()
                ),
                logDirectory: appConfigs.Get(path: "Logging.LogDirectory", defaultValue: "logs")
            );

            if (!Directory.Exists(fullPath))
                throw new DirectoryNotFoundException(
                    message: string.Format(
                        language.GetMessage(path: "logging.directory_not_found"),
                        fullPath
                    )
                );

            var files = Directory
                .GetFiles(fullPath, "logs-*.txt")
                .Select(f => new FileInfo(f))
                .Select(f =>
                    SystemLoggingLogic.CreateFileInfo(
                        file: f,
                        requestScheme: Request.Scheme,
                        requestHost: Request.Host.Value
                    )
                )
                .OrderByDescending(f => ((dynamic)f).Modified)
                .ToList();

            return new APIResponseData<List<object>>()
                .ChangeStatus(language: language, key: "api.status.success")
                .ChangeData(data: files);
        }
        catch (Exception ex)
        {
            return new APIResponseData<List<object>>()
                .ChangeStatus(language: language, key: "api.status.failed")
                .ChangeMessage(
                    exception: ex,
                    logging: systemLogging,
                    includeStackTrace: Commons.IsDebugEnvironment()
                );
        }
    }

    /// <summary>
    /// Gets log entries between specified time range
    /// </summary>
    /// <param name="startTime">Start time in ISO 8601 format</param>
    /// <param name="endTime">End time in ISO 8601 format</param>
    /// <returns>List of log entries within the specified time range</returns>
    [Tags(tags: "System Logging"), HttpGet(template: "Read")]
    public APIResponseData<List<object>> ReadLogs(
        [FromQuery] DateTime startTime,
        [FromQuery] DateTime endTime
    )
    {
        try
        {
            var fullPath = SystemLoggingLogic.GetFullLogPath(
                baseDirectory: appConfigs.Get(
                    path: "Logging.baseDirectory",
                    defaultValue: Directory.GetCurrentDirectory()
                ),
                logDirectory: appConfigs.Get(path: "Logging.LogDirectory", defaultValue: "logs")
            );

            if (!Directory.Exists(fullPath))
                throw new DirectoryNotFoundException(
                    message: string.Format(
                        language.GetMessage(path: "logging.directory_not_found"),
                        fullPath
                    )
                );

            var logFiles = SystemLoggingLogic.GetLogFilesByDateRange(
                fullPath: fullPath,
                startTime: startTime,
                endTime: endTime
            );

            var logEntries = SystemLoggingLogic.GetLogEntries(
                logFiles: logFiles,
                startTime: startTime,
                endTime: endTime
            );

            var groupedEntries = SystemLoggingLogic.GroupLogEntries(logEntries: logEntries);

            return new APIResponseData<List<object>>()
                .ChangeStatus(language: language, key: "api.status.success")
                .ChangeData(data: [groupedEntries]);
        }
        catch (Exception ex)
        {
            return new APIResponseData<List<object>>()
                .ChangeStatus(language: language, key: "api.status.failed")
                .ChangeMessage(
                    exception: ex,
                    logging: systemLogging,
                    includeStackTrace: Commons.IsDebugEnvironment()
                );
        }
    }
}
