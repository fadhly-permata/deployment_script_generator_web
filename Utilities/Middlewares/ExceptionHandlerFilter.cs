using IDC.Utilities;
using IDC.Utilities.Models.API;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IDC.DBDeployTools.Utilities.Middlewares;

/// <summary>
/// Filter that handles exceptions and returns appropriate error responses based on the exception type.
/// </summary>
/// <remarks>
/// This filter implements IExceptionFilter to catch unhandled exceptions and convert them to standardized API responses.
/// It handles various exception types and returns appropriate responses with localized error messages.
/// In production environments, only safe error messages are returned to prevent security vulnerabilities.
///
/// Features:
/// - Automatic exception type to HTTP status code mapping
/// - Localized error messages using Language service
/// - Environment-aware stack trace inclusion
/// - Standardized API response format
/// - Secure error handling in production
/// - System logging integration
///
/// Example implementation:
/// <example>
/// <code>
/// // In Program.cs or Startup.cs
/// builder.Services.AddControllers(options =>
/// {
///     options.Filters.Add&lt;ExceptionHandlerFilter&gt;();
/// });
///
/// // Example controller action that might throw an exception
/// [HttpGet("users/{id}")]
/// public async Task&lt;IActionResult&gt; GetUser(int id)
/// {
///     var user = await _userService.GetByIdAsync(id: id);
///     if (user is null)
///         throw new KeyNotFoundException($"User with ID {id} not found");
///     return Ok(user);
/// }
///
/// // Example response when exception occurs
/// {
///     "status": "Failed",
///     "message": "The requested resource was not found",
///     "data": ["User with ID 123 not found"],
///     "stackTrace": "at UserService.GetByIdAsync..." // Debug only
/// }
/// </code>
/// </example>
///
/// > [!IMPORTANT]
/// > This filter should be registered globally to ensure consistent error handling across the application
///
/// > [!NOTE]
/// > Stack traces are only included in debug environments for security purposes
///
/// > [!WARNING]
/// > Ensure sensitive information is not exposed in error messages in production
///
/// > [!TIP]
/// > Use specific exception types for more granular error handling
///
/// > [!CAUTION]
/// > Always validate input parameters before processing to prevent unnecessary exceptions
/// </remarks>
/// <param name="language">
/// <see cref="Language"/> Service for message localization.
/// <see href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/localization"/>
/// </param>
/// <param name="systemLogging">
/// <see cref="SystemLogging"/> Service for error logging and tracking.
/// <see href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging"/>
/// </param>
/// <seealso cref="IExceptionFilter"/>
/// <seealso cref="APIResponseData{T}"/>
/// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/filters">
/// ASP.NET Core Filters
/// </seealso>
/// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/web-api/handle-errors">
/// Handle errors in ASP.NET Core web APIs
/// </seealso>
/// <seealso href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/error-handling">
/// Error handling in ASP.NET Core
/// </seealso>
public class ExceptionHandlerFilter(Language language, SystemLogging systemLogging)
    : IExceptionFilter
{
    /// <summary>
    /// Handles exceptions that occur during request processing and returns appropriate error responses.
    /// </summary>
    /// <remarks>
    /// Processes unhandled exceptions and generates standardized error responses based on exception type.
    /// Each exception type maps to a specific HTTP status code and error message format.
    ///
    /// Exception type mappings:
    /// <list type="table">
    ///     <listheader>
    ///         <term>Exception Type</term>
    ///         <description>HTTP Status Code</description>
    ///     </listheader>
    ///     <item>
    ///         <term><see cref="BadHttpRequestException"/></term>
    ///         <description>400 Bad Request</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="UnauthorizedAccessException"/></term>
    ///         <description>401 Unauthorized</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="KeyNotFoundException"/></term>
    ///         <description>404 Not Found</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="InvalidOperationException"/></term>
    ///         <description>400 Bad Request</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="ArgumentException"/></term>
    ///         <description>400 Bad Request</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="NotImplementedException"/></term>
    ///         <description>501 Not Implemented</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="TimeoutException"/></term>
    ///         <description>408 Request Timeout</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="OperationCanceledException"/></term>
    ///         <description>409 Conflict</description>
    ///     </item>
    ///     <item>
    ///         <term>All other exceptions</term>
    ///         <description>500 Internal Server Error</description>
    ///     </item>
    /// </list>
    ///
    /// Example response format:
    /// <example>
    /// <code>
    /// // 404 Not Found Response
    /// {
    ///   "status": "Failed",
    ///   "message": "Resource not found",
    ///   "data": [
    ///     "Could not find user with ID: 123"
    ///   ],
    ///   "stackTrace": "at UserService.GetUser(int id)..." // Only in debug mode
    /// }
    ///
    /// // 400 Bad Request Response
    /// {
    ///   "status": "Failed",
    ///   "message": "Invalid request parameters",
    ///   "data": [
    ///     "Username must be between 3 and 50 characters"
    ///   ]
    /// }
    /// </code>
    /// </example>
    ///
    /// > [!IMPORTANT]
    /// > Stack traces are only included in debug environments for security
    ///
    /// > [!NOTE]
    /// > All error messages are automatically localized based on the configured language
    ///
    /// > [!WARNING]
    /// > Sensitive information is automatically redacted from error messages in production
    ///
    /// > [!TIP]
    /// > Use specific exception types for better error handling granularity
    /// </remarks>
    /// <param name="context">
    /// <see cref="ExceptionContext"/> The context containing exception details and HTTP context.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.filters.exceptioncontext"/>
    /// </param>
    /// <seealso cref="IExceptionFilter"/>
    /// <seealso cref="APIResponseData{T}"/>
    /// <seealso cref="Language"/>
    /// <seealso cref="SystemLogging"/>
    /// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling">
    /// ASP.NET Core Error Handling
    /// </seealso>
    /// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/web-api/handle-errors">
    /// Handle errors in ASP.NET Core web APIs
    /// </seealso>
    public void OnException(ExceptionContext context)
    {
        var response = new APIResponseData<List<string>?>()
            .ChangeStatus(language: language, key: "api.status.failed")
            .ChangeMessage(
                exception: context.Exception,
                logging: systemLogging,
                includeStackTrace: Commons.IsDebugEnvironment()
            );

        context.Result = context.Exception switch
        {
            BadHttpRequestException => new BadRequestObjectResult(error: response),
            UnauthorizedAccessException => new UnauthorizedObjectResult(value: response),
            KeyNotFoundException => new NotFoundObjectResult(value: response),
            InvalidOperationException => new BadRequestObjectResult(error: response),
            ArgumentException => new BadRequestObjectResult(error: response),
            NotImplementedException => new StatusCodeResult(
                statusCode: StatusCodes.Status501NotImplemented
            ),
            TimeoutException => new StatusCodeResult(
                statusCode: StatusCodes.Status408RequestTimeout
            ),
            OperationCanceledException => new StatusCodeResult(
                statusCode: StatusCodes.Status409Conflict
            ),
            _ => new ObjectResult(value: response)
            {
                StatusCode = StatusCodes.Status500InternalServerError,
            },
        };
    }
}
