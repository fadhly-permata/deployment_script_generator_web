using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ScriptDeployerWeb.Utilities.Middlewares.Swagger;

/// <summary>
/// Filter to set default group for Swagger/OpenAPI endpoints that don't have any group specified.
/// </summary>
/// <remarks>
/// This filter implements IDocumentFilter to modify the OpenAPI document and assigns endpoints to appropriate groups.
/// If an endpoint doesn't have a group specified, it will be assigned to the 'Main' group by default.
/// Demo endpoints are those under the '/api/demo/' path.
///
/// Features:
/// - Automatic endpoint grouping
/// - Support for custom tags
/// - Path-based categorization
/// - Group separation for demo and main APIs
///
/// Configuration in appconfigs.jsonc:
/// <code>
/// {
///   "SwaggerConfig": {
///     "UI": {
///       "Enable": true,
///       "SortEndpoints": true
///     },
///     "OpenApiInfo": {
///       "Title": "API - Universal Data for MongoDB",
///       "Version": "v2"
///     }
///   }
/// }
/// </code>
///
/// Example Implementation:
/// <code>
/// // Controller without explicit group
/// [ApiController]
/// public class DataController
/// {
///     [HttpGet("api/demo/data")]  // Will be grouped as "Demo"
///     public async Task&lt;IActionResult&gt; GetDemoData()
///     {
///         return Ok("Demo data");
///     }
///
///     [HttpGet("api/data")]      // Will be grouped as "Main"
///     public async Task&lt;IActionResult&gt; GetMainData()
///     {
///         return Ok("Main data");
///     }
/// }
///
/// // Controller with explicit group
/// [ApiController]
/// [Tags("CustomGroup")]
/// public class CustomController
/// {
///     [HttpGet]                  // Will keep "CustomGroup" tag
///     public async Task&lt;IActionResult&gt; Get()
///     {
///         return Ok("Custom group data");
///     }
/// }
/// </code>
///
/// > [!NOTE]
/// > Endpoints are automatically categorized based on their path prefix
///
/// > [!TIP]
/// > Use [Tags] attribute to override automatic grouping
///
/// > [!IMPORTANT]
/// > Groups are used for UI organization only and don't affect API functionality
///
/// > [!CAUTION]
/// > Ensure consistent grouping strategy across your API endpoints
/// </remarks>
/// <example>
/// <code>
/// builder.Services.AddSwaggerGen(options =>
/// {
///     options.DocumentFilter&lt;DefaultGroupDocFilter&gt;();
/// });
/// </code>
/// </example>
/// <seealso cref="IDocumentFilter"/>
/// <seealso cref="OpenApiDocument"/>
/// <seealso cref="DocumentSortDocFilter"/>
/// <seealso href="https://swagger.io/docs/specification/grouping-operations-with-tags/">Swagger Tags Documentation</seealso>
/// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger">
/// ASP.NET Core Swagger Documentation
/// </seealso>
public class DefaultGroupDocFilter : IDocumentFilter
{
    /// <summary>
    /// Applies default grouping to the Swagger/OpenAPI document operations.
    /// </summary>
    /// <param name="swaggerDoc">The OpenAPI document to modify. Contains all API endpoints and their metadata.</param>
    /// <param name="context">The document filter context containing the API model and schema information.</param>
    /// <remarks>
    /// Groups endpoints based on their path and tags for better organization in Swagger UI.
    ///
    /// Processing Logic:
    /// - Preserves existing tags if already defined
    /// - Auto-categorizes untagged endpoints based on path
    /// - Sorts and deduplicates final tag list
    /// - Separates demo and main endpoints
    ///
    /// Configuration Example:
    /// <code>
    /// {
    ///   "SwaggerConfig": {
    ///     "UI": {
    ///       "Enable": true,
    ///       "SortEndpoints": true
    ///     }
    ///   }
    /// }
    /// </code>
    ///
    /// Example Request/Response:
    /// <code>
    /// // Original endpoint without tag
    /// [HttpGet("/api/demo/users")]
    /// public IActionResult GetUsers() { ... }
    ///
    /// // After processing:
    /// {
    ///   "paths": {
    ///     "/api/demo/users": {
    ///       "get": {
    ///         "tags": ["Demo"],
    ///         ...
    ///       }
    ///     }
    ///   }
    /// }
    /// </code>
    ///
    /// > [!NOTE]
    /// > Endpoints under /api/demo/ are automatically tagged as "Demo"
    ///
    /// > [!TIP]
    /// > Use explicit tags in controllers to override automatic grouping
    ///
    /// > [!IMPORTANT]
    /// > Tag changes affect Swagger UI organization only, not API behavior
    /// </remarks>
    /// <example>
    /// <code>
    /// // Usage in Startup.cs
    /// builder.Services.AddSwaggerGen(options =>
    /// {
    ///     options.DocumentFilter&lt;DefaultGroupDocFilter&gt;();
    ///     options.SwaggerDoc("v1", new OpenApiInfo { Title = "API Documentation" });
    /// });
    ///
    /// // Controller with explicit tag
    /// [ApiController]
    /// [Tags("CustomGroup")]
    /// public class UserController
    /// {
    ///     [HttpGet]
    ///     public IActionResult Get() { ... }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="IDocumentFilter"/>
    /// <seealso cref="OpenApiDocument"/>
    /// <seealso href="https://swagger.io/docs/specification/grouping-operations-with-tags/">Swagger Tags Documentation</seealso>
    /// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger">ASP.NET Core Swagger Documentation</seealso>
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var paths = swaggerDoc.Paths.ToDictionary(
            keySelector: x => x.Key,
            elementSelector: x => x.Value
        );
        swaggerDoc.Paths.Clear();

        var isDemoDoc = swaggerDoc.Info.Title.Contains(
            value: "Demo",
            comparisonType: StringComparison.OrdinalIgnoreCase
        );

        foreach (var path in paths)
        {
            var isDemoEndpoint = path.Key.StartsWith(
                value: "/api/demo/",
                comparisonType: StringComparison.OrdinalIgnoreCase
            );

            if (isDemoDoc == isDemoEndpoint)
            {
                swaggerDoc.Paths.Add(key: path.Key, value: path.Value);

                foreach (var operation in path.Value.Operations)
                {
                    if (!operation.Value.Tags.Any())
                    {
                        operation.Value.Tags =
                        [
                            new OpenApiTag { Name = isDemoDoc ? "Demo" : "Main" },
                        ];
                    }
                }
            }
        }

        swaggerDoc.Tags =
        [
            .. swaggerDoc
                .Paths.SelectMany(selector: p =>
                    p.Value.Operations.SelectMany(selector: o => o.Value.Tags)
                )
                .DistinctBy(keySelector: t => t.Name)
                .OrderBy(keySelector: t => t.Name),
        ];
    }
}
