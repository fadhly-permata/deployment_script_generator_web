using IDC.DBDeployTools.Utilities.Middlewares;
using IDC.Utilities.Models.API;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace IDC.DBDeployTools;

internal partial class Program
{
    /// <summary>
    /// Configures core services and controllers for the application
    /// </summary>
    /// <param name="builder">The web application builder instance</param>
    /// <remarks>
    /// Configures:
    /// - Controller settings with filters and JSON serialization
    /// - CORS policies based on security configuration
    ///
    /// Example CORS configuration in appconfigs.jsonc:
    /// <code>
    /// {
    ///   "Security": {
    ///     "Cors": {
    ///       "Enabled": true,
    ///       "AllowedHosts": [
    ///         "http://*",
    ///         "https://*"
    ///       ],
    ///       "AllowedHeaders": [
    ///         "X-API-Key",
    ///         "Authorization",
    ///         "Content-Type"
    ///       ],
    ///       "AllowedMethods": [
    ///         "GET",
    ///         "POST"
    ///       ]
    ///     }
    ///   }
    /// }
    /// </code>
    ///
    /// Controller configuration includes:
    /// - Model state validation filter
    /// - Exception handling filter
    /// - JSON content type constraints
    /// - Standard response types (200, 400, 500)
    ///
    /// > [!IMPORTANT]
    /// > All API responses are in JSON format with camelCase property names
    ///
    /// > [!NOTE]
    /// > Reference loops in JSON serialization are ignored by default
    /// </remarks>
    /// <seealso cref="ModelStateInvalidFilters"/>
    /// <seealso cref="ExceptionHandlerFilter"/>
    /// <seealso cref="APIResponseData{T}"/>
    /// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/web-api/">ASP.NET Core Web API</seealso>
    /// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/security/cors">CORS in ASP.NET Core</seealso>
    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder
            .Services.AddControllers(configure: options =>
            {
                const string ContentType = "application/json";

                options.Filters.Add(filterType: typeof(ModelStateInvalidFilters));
                options.Filters.Add(filterType: typeof(ExceptionHandlerFilter));
                options.Filters.Add(item: new ConsumesAttribute(contentType: ContentType));
                options.Filters.Add(item: new ProducesAttribute(contentType: ContentType));
                options.Filters.Add(item: new ProducesResponseTypeAttribute(statusCode: 200));
                options.Filters.Add(
                    item: new ProducesResponseTypeAttribute(
                        type: typeof(APIResponseData<List<string>?>),
                        statusCode: StatusCodes.Status400BadRequest
                    )
                );
                options.Filters.Add(
                    item: new ProducesResponseTypeAttribute(
                        type: typeof(APIResponseData<List<string>?>),
                        statusCode: StatusCodes.Status500InternalServerError
                    )
                );
            })
            .AddNewtonsoftJson(setupAction: options =>
            {
                options.SerializerSettings.ContractResolver =
                    new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

        // Add CORS policy
        if (_appConfigs.Get<bool>(path: "Security.Cors.Enabled"))
        {
            builder.Services.AddCors(setupAction: options =>
            {
                options.AddPolicy(
                    name: "CorsPolicy",
                    configurePolicy: policy =>
                    {
                        var allowedHosts =
                            _appConfigs.Get<string[]>(path: "Security.Cors.AllowedHosts") ?? [];

                        // Allow any origin if any host pattern is a full wildcard
                        if (
                            allowedHosts.Any(predicate: host =>
                                host.Trim() == "*"
                                || host.Trim()
                                    .Equals(
                                        value: "http://*",
                                        comparisonType: StringComparison.OrdinalIgnoreCase
                                    )
                                || host.Trim()
                                    .Equals(
                                        value: "https://*",
                                        comparisonType: StringComparison.OrdinalIgnoreCase
                                    )
                            )
                        )
                            policy.AllowAnyOrigin();
                        else if (allowedHosts.Any(predicate: host => host.Contains(value: '*')))
                            policy.WithOrigins(
                                origins:
                                [
                                    .. allowedHosts.Where(predicate: host =>
                                        !host.Contains(value: '*')
                                        && Uri.TryCreate(
                                            uriString: host,
                                            uriKind: UriKind.Absolute,
                                            result: out _
                                        )
                                    ),
                                ]
                            );
                        else
                            policy.WithOrigins(origins: allowedHosts);

                        policy
                            .WithHeaders(
                                headers: _appConfigs.Get<string[]>(
                                    path: "Security.Cors.AllowedHeaders"
                                ) ?? ["*"]
                            )
                            .WithMethods(
                                methods: _appConfigs.Get<string[]>(
                                    path: "Security.Cors.AllowedMethods"
                                )
                                    ??
                                    [
                                        "GET",
                                        "POST",
                                        "PUT",
                                        "DELETE",
                                        "OPTIONS",
                                        "HEAD",
                                        "PATCH",
                                        "TRACE",
                                        "CONNECT",
                                    ]
                            );
                    }
                );
            });
        }
    }
}
