using System.Diagnostics;
using IDC.DBDeployTools.Utilities;
using IDC.DBDeployTools.Utilities.DI;
using IDC.DBDeployTools.Utilities.Helpers;
using IDC.Utilities;
using IDC.Utilities.Models.API;
using Microsoft.AspNetCore.Mvc;

namespace IDC.DBDeployTools.Controllers;

/// <summary>
/// Controller for managing demo operations
/// </summary>
/// <remarks>
/// Provides endpoints for system logging and other demo functionalities
/// </remarks>
/// <example>
/// <code>
/// var controller = new DemoController(new SystemLogging());
/// controller.LogInfo(message: "Test message");
/// </code>
/// </example>
[Route("api/demo/DemoAppManagement")]
[ApiController]
[ApiExplorerSettings(GroupName = "Demo")]
public class Demo(SystemLogging systemLogging, Language language, AppConfigsHandler appConfigs)
    : ControllerBase
{
    private const string CON_API_STATUS_FAILED = "api.status.failed";

    /// <summary>
    /// Restarts the current application process
    /// </summary>
    /// <remarks>
    /// This endpoint initiates a graceful shutdown and restart of the application.
    /// The process is restarted with the same executable path as the current process.
    ///
    /// > [!IMPORTANT]
    /// > This operation will cause temporary service interruption.
    ///
    /// > [!CAUTION]
    /// > Ensure all critical operations are completed before calling this endpoint.
    ///
    /// Example usage:
    /// <code>
    /// var response = await httpClient.PostAsync("api/Demo/RestartApps", null);
    /// var result = await response.Content.ReadFromJsonAsync&lt;APIResponseData&lt;string&gt;&gt;();
    /// // result.Data will contain "Restarting application..."
    /// </code>
    /// </remarks>
    /// <returns>
    /// <see cref="APIResponseData{T}"/> where T is <see cref="string"/>
    /// containing confirmation message if successful, or error details if failed
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown when unable to determine current process filename</exception>
    /// <exception cref="System.ComponentModel.Win32Exception">Thrown when the process cannot be started</exception>
    [Tags(tags: "Apps Management"), HttpPost(template: "RestartApps")]
    public APIResponseData<string> RestartApps()
    {
        try
        {
            using var _ = Process.Start(
                startInfo: new ProcessStartInfo
                {
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    FileName =
                        (Process.GetCurrentProcess().MainModule?.FileName)
                        ?? throw new InvalidOperationException(
                            message: "Cannot get current process filename"
                        ),
                }
            );

            Environment.Exit(exitCode: 0);
            return new APIResponseData<string>().ChangeData(data: "Restarting application...");
        }
        catch (Exception ex)
        {
            return new APIResponseData<string>()
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
    [Tags(tags: "Apps Management"), HttpGet(template: "Logs/Files")]
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

            if (!Directory.Exists(path: fullPath))
                throw new DirectoryNotFoundException(
                    message: string.Format(
                        format: language.GetMessage(path: "logging.directory_not_found"),
                        arg0: fullPath
                    )
                );

            return new APIResponseData<List<object>>()
                .ChangeStatus(language: language, key: "api.status.success")
                .ChangeData(
                    data:
                    [
                        .. Directory
                            .GetFiles(path: fullPath, searchPattern: "logs-*.txt")
                            .Select(selector: f => new FileInfo(fileName: f))
                            .Select(selector: f =>
                                SystemLoggingLogic.CreateFileInfo(
                                    file: f,
                                    requestScheme: Request.Scheme,
                                    requestHost: Request.Host.Value
                                )
                            )
                            .OrderByDescending(keySelector: f => ((dynamic)f).Modified),
                    ]
                );
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
    [Tags(tags: "Apps Management"), HttpGet(template: "Logs/Read")]
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

            if (!Directory.Exists(path: fullPath))
                throw new DirectoryNotFoundException(
                    message: string.Format(
                        format: language.GetMessage(path: "logging.directory_not_found"),
                        arg0: fullPath
                    )
                );

            return new APIResponseData<List<object>>()
                .ChangeStatus(language: language, key: "api.status.success")
                .ChangeData(
                    data:
                    [
                        (
                            SystemLoggingLogic.GroupLogEntries(
                                logEntries: SystemLoggingLogic.GetLogEntries(
                                    logFiles: SystemLoggingLogic.GetLogFilesByDateRange(
                                        fullPath: fullPath,
                                        startTime: startTime,
                                        endTime: endTime
                                    ),
                                    startTime: startTime,
                                    endTime: endTime
                                )
                            )
                        ),
                    ]
                );
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
