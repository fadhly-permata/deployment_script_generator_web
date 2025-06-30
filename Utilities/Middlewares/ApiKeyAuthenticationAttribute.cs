using Microsoft.AspNetCore.Mvc;

namespace ScriptDeployerWeb.Utilities.Middlewares;

/// <summary>
/// Attribute for enforcing API key authentication on controllers and actions.
/// </summary>
/// <remarks>
/// This attribute enforces API key authentication on controllers and actions by validating the API key
/// provided in the request header against registered keys in configuration.
///
/// Features:
/// - Controller-level or method-level authentication
/// - Automatic API key validation
/// - Integration with middleware pipeline
/// - Support for multiple API keys
/// - Configurable via appconfigs.jsonc
///
/// Example configuration in appconfigs.jsonc:
/// <code>
/// {
///   "Security": {
///     "RegisteredApiKeyList": [
///       "NFHUZqt0zmL6siZ7/ynQ8nljJtsQrT3h0+nQZHhIQhk=",
///       "IDxvX6aT3XTERRpuHpMNtpcQVUo2rZ3Smtm83UPVfi8="
///     ]
///   }
/// }
/// </code>
///
/// Example usage:
/// <code>
/// // Protect entire controller
/// [ApiKeyAuthentication]
/// public class SecureController : ControllerBase
/// {
///     [HttpGet]
///     public IActionResult GetSecureData()
///     {
///         return Ok("Secured data");
///     }
/// }
///
/// // Protect specific endpoints
/// public class MixedController : ControllerBase
/// {
///     [ApiKeyAuthentication]
///     [HttpPost]
///     public IActionResult SecureEndpoint([FromBody] SecureData data)
///     {
///         return Ok("Processed secure data");
///     }
///
///     [HttpGet]
///     public IActionResult PublicEndpoint()
///     {
///         return Ok("Public data");
///     }
/// }
/// </code>
///
/// > [!IMPORTANT]
/// > API keys must be included in the X-API-Key header for authenticated requests
///
/// > [!NOTE]
/// > Authentication is skipped for Swagger UI, static files, and documentation paths
///
/// > [!TIP]
/// > Use controller-level authentication to secure all endpoints within a controller
///
/// > [!CAUTION]
/// > Store API keys securely and rotate them periodically
/// </remarks>
/// <seealso cref="TypeFilterAttribute"/>
/// <seealso cref="ApiKeyAuthenticationMiddleware"/>
/// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/security/authentication/">ASP.NET Core Authentication</seealso>
/// <seealso href="https://datatracker.ietf.org/doc/html/rfc6750">The OAuth 2.0 Authorization Framework: Bearer Token Usage</seealso>
/// <seealso href="https://owasp.org/www-project-api-security/">OWASP API Security Project</seealso>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyAuthenticationAttribute : TypeFilterAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiKeyAuthenticationAttribute"/> class.
    /// </summary>
    /// <remarks>
    /// Creates attribute that injects and uses <see cref="ApiKeyAuthenticationMiddleware"/>
    /// for authentication.
    /// </remarks>
    public ApiKeyAuthenticationAttribute()
        : base(typeof(ApiKeyAuthenticationMiddleware)) { }
}
