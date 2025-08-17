using IDC.Utilities;
using IDC.Utilities.Models.API;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IDC.DBDeployTools.Utilities.Middlewares;

/// <summary>
/// Filter that validates the model state and returns appropriate error responses when validation fails.
/// </summary>
/// <remarks>
/// This filter automatically validates incoming request models and generates standardized error responses.
/// It integrates with ASP.NET Core's model validation system and provides localized error messages.
///
/// Features:
/// - Automatic model state validation
/// - Localized error messages
/// - Standardized error response format
/// - Collection of all validation errors
///
/// > [!IMPORTANT]
/// > Register this filter globally to ensure consistent validation across all controllers
///
/// > [!NOTE]
/// > Error messages are retrieved from the language service using the error message key
///
/// > [!TIP]
/// > Use data annotations in your models to define validation rules
///
/// Example validation response:
/// <code>
/// {
///   "status": "failed",
///   "message": "Request validation failed",
///   "data": [
///     "The Name field is required",
///     "Email must be in valid format",
///     "Age must be between 18 and 100"
///   ]
/// }
/// </code>
///
/// Example model with validation:
/// <code>
/// public class UserModel
/// {
///     [Required(ErrorMessage = "validation.name.required")]
///     public string Name { get; set; }
///
///     [EmailAddress(ErrorMessage = "validation.email.format")]
///     public string Email { get; set; }
///
///     [Range(18, 100, ErrorMessage = "validation.age.range")]
///     public int Age { get; set; }
/// }
/// </code>
/// </remarks>
/// <param name="language">The language service for message localization</param>
/// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation">Model validation in ASP.NET Core MVC</seealso>
/// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/filters">Filters in ASP.NET Core</seealso>
/// <see cref="Language"/>
/// <see cref="APIResponseData{T}"/>
public class ModelStateInvalidFilters(Language language) : IActionFilter
{
    /// <summary>
    /// Executes before the action method and validates the model state.
    /// </summary>
    /// <remarks>
    /// This method performs the following:
    /// 1. Checks if the model state is valid
    /// 2. If invalid, collects all validation errors
    /// 3. Translates error messages using the language service
    /// 4. Returns a BadRequestObjectResult with detailed error information
    ///
    /// > [!NOTE]
    /// > Each error message key is looked up in the language service
    ///
    /// > [!TIP]
    /// > Group related validation errors by model property
    ///
    /// Error Response Structure:
    /// - status: Always "failed" for validation errors
    /// - message: Localized general validation error message
    /// - data: Array of specific validation error messages
    ///
    /// Example implementation in Startup.cs:
    /// <code>
    /// services.AddControllers(options =>
    /// {
    ///     options.Filters.Add(new ModelStateInvalidFilters(
    ///         new Language("en")
    ///     ));
    /// });
    /// </code>
    /// </remarks>
    /// <param name="context">The context for action execution</param>
    /// <exception cref="InvalidOperationException">Thrown when language service fails to retrieve messages</exception>
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.modelbinding.modelstationary">ModelStateDictionary Class</see>
    /// <see href="https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation">Model Validation</see>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
            context.Result = new BadRequestObjectResult(
                error: new APIResponseData<List<string>?>()
                    .ChangeStatus(language: language, key: "api.status.failed")
                    .ChangeMessage(language: language, key: "api.message.request_validation_error")
                    .ChangeData(
                        [
                            .. context
                                .ModelState.Values.SelectMany(selector: e => e.Errors)
                                .Select(selector: e => language.GetMessage(path: e.ErrorMessage)),
                        ]
                    )
            );
    }

    /// <summary>
    /// Executes after the action method but before the result execution.
    /// </summary>
    /// <remarks>
    /// This method is called after an action has executed but before the result has been processed.
    /// The current implementation is intentionally empty as post-action validation is not required.
    ///
    /// Common scenarios where you might want to implement this method:
    /// <list type="bullet">
    ///   <item><description>Post-processing of action results</description></item>
    ///   <item><description>Modifying the response based on execution outcome</description></item>
    ///   <item><description>Logging or metrics collection</description></item>
    /// </list>
    ///
    /// Example implementation for response modification:
    /// <example>
    /// <code>
    /// public void OnActionExecuted(ActionExecutedContext context)
    /// {
    ///     if (context.Result is ObjectResult result)
    ///     {
    ///         // Modify the response object
    ///         if (result.Value is APIResponseData&lt;object&gt; response)
    ///         {
    ///             response.AddMetadata("ExecutionTime", DateTime.UtcNow);
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    ///
    /// > [!NOTE]
    /// > Required by the <see cref="IActionFilter"/> interface implementation
    ///
    /// > [!TIP]
    /// > Override this method when you need to perform post-action processing
    /// </remarks>
    /// <param name="context">
    /// <see cref="ActionExecutedContext"/> The context for the executed action.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.filters.actionexecutedcontext"/>
    /// </param>
    /// <seealso cref="IActionFilter"/>
    /// <seealso cref="ActionExecutedContext"/>
    /// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/filters">
    /// ASP.NET Core Action Filters
    /// </seealso>
    public void OnActionExecuted(ActionExecutedContext context) { }
}
