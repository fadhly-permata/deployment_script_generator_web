using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ScriptDeployerWeb.Utilities.Middlewares.Swagger;

/// <summary>
/// Filter to sort Swagger/OpenAPI endpoints alphabetically.
/// </summary>
/// <remarks>
/// Implements IDocumentFilter to modify OpenAPI documentation by sorting all endpoints alphabetically by their paths.
/// Provides consistent and organized API documentation presentation in Swagger UI.
///
/// Features:
/// - Alphabetical sorting of API endpoints
/// - Maintains original path-operation mapping
/// - Compatible with grouped endpoints
/// - Preserves HTTP method ordering
///
/// Configuration in appconfigs.jsonc:
/// <code>
/// {
///   "SwaggerConfig": {
///     "UI": {
///       "Enable": true,
///       "SortEndpoints": true  // Enable/disable endpoint sorting
///     }
///   }
/// }
/// </code>
///
/// Example API Structure:
/// <code>
/// // Before sorting:
/// /api/users/create    [POST]
/// /api/auth/login     [POST]
/// /api/users/{id}     [GET]
/// /api/settings       [GET]
///
/// // After sorting:
/// /api/auth/login     [POST]
/// /api/settings       [GET]
/// /api/users/{id}     [GET]
/// /api/users/create   [POST]
/// </code>
///
/// Implementation Example:
/// <code>
/// [ApiController]
/// [Route("api/[controller]")]
/// public class UsersController : ControllerBase
/// {
///     [HttpGet("{id}")]         // Will be sorted as "/api/users/{id}"
///     public async Task&lt;IActionResult&gt; GetUser(string id)
///     {
///         return Ok($"User {id}");
///     }
///
///     [HttpPost("create")]      // Will be sorted as "/api/users/create"
///     public async Task&lt;IActionResult&gt; CreateUser([FromBody] UserDto user)
///     {
///         return Created($"/api/users/{user.Id}", user);
///     }
/// }
/// </code>
///
/// > [!NOTE]
/// > Sorting is applied at the documentation level and doesn't affect API routing
///
/// > [!TIP]
/// > Use consistent path naming conventions for better organization
///
/// > [!IMPORTANT]
/// > Enable sorting through SwaggerConfig.UI.SortEndpoints in configuration
/// </remarks>
/// <example>
/// <code>
/// builder.Services.AddSwaggerGen(options =>
/// {
///     options.DocumentFilter&lt;DocumentSortDocFilter&gt;();
/// });
/// </code>
/// </example>
/// <seealso cref="IDocumentFilter"/>
/// <seealso cref="OpenApiDocument"/>
/// <seealso cref="DefaultGroupDocFilter"/>
/// <seealso href="https://swagger.io/docs/specification/paths-and-operations/">Swagger Paths Documentation</seealso>
/// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle">
/// Getting Started with Swashbuckle
/// </seealso>
public class DocumentSortDocFilter : IDocumentFilter
{
    /// <summary>
    /// Applies alphabetical sorting to the Swagger/OpenAPI document paths.
    /// </summary>
    /// <param name="swaggerDoc">
    /// The OpenAPI document to modify.
    /// <see cref="OpenApiDocument"/>
    /// </param>
    /// <param name="context">
    /// The document filter context.
    /// <see cref="DocumentFilterContext"/>
    /// </param>
    /// <remarks>
    /// Orders all API endpoints alphabetically by their path and reconstructs the Paths collection.
    ///
    /// Processing Steps:
    /// 1. Orders existing paths alphabetically
    /// 2. Creates new dictionary with sorted paths
    /// 3. Clears original paths collection
    /// 4. Adds sorted paths back to document
    ///
    /// Example Request/Response:
    /// <code>
    /// // Input paths
    /// {
    ///   "/api/users": { ... },
    ///   "/api/auth": { ... },
    ///   "/api/data": { ... }
    /// }
    ///
    /// // Output paths (sorted)
    /// {
    ///   "/api/auth": { ... },
    ///   "/api/data": { ... },
    ///   "/api/users": { ... }
    /// }
    /// </code>
    ///
    /// > [!NOTE]
    /// > Sorting is performed on path strings only, HTTP methods remain in original order
    ///
    /// > [!TIP]
    /// > Use consistent path naming patterns for predictable sorting results
    ///
    /// > [!IMPORTANT]
    /// > This operation modifies the OpenApiDocument instance directly
    /// </remarks>
    /// <example>
    /// <code>
    /// var filter = new DocumentSortDocFilter();
    /// var openApiDoc = new OpenApiDocument
    /// {
    ///     Paths = new OpenApiPaths
    ///     {
    ///         ["/api/users"] = new OpenApiPathItem(),
    ///         ["/api/auth"] = new OpenApiPathItem()
    ///     }
    /// };
    /// var context = new DocumentFilterContext(apiDescriptions, schemaGenerator, schemaRepository);
    ///
    /// filter.Apply(swaggerDoc: openApiDoc, context: context);
    /// </code>
    /// </example>
    /// <seealso href="https://swagger.io/docs/specification/paths-and-operations/">
    /// Swagger Paths and Operations
    /// </seealso>
    /// <seealso href="https://github.com/domaindrivendev/Swashbuckle.AspNetCore">
    /// Swashbuckle Documentation
    /// </seealso>
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var paths = swaggerDoc
            .Paths.OrderBy(static x => x.Key)
            .ToDictionary(static x => x.Key, static x => x.Value);

        swaggerDoc.Paths.Clear();
        foreach (var path in paths)
            swaggerDoc.Paths.Add(path.Key, path.Value);
    }
}
