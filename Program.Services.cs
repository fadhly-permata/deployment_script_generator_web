using IDC.Utilities.Models.API;
using ScriptDeployerWeb.Utilities.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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
            .Services.AddControllers(options =>
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
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(
                    name: "CorsPolicy",
                    policy =>
                    {
                        policy
                            .WithOrigins(
                                _appConfigs.Get<string[]>(path: "Security.Cors.AllowedHosts")
                                    ?? ["*"]
                            )
                            .WithHeaders(
                                _appConfigs.Get<string[]>(path: "Security.Cors.AllowedHeaders")
                                    ?? ["*"]
                            )
                            .WithMethods(
                                _appConfigs.Get<string[]>(path: "Security.Cors.AllowedMethods")
                                    ?? ["*"]
                            );
                    }
                );
            });
        }
    }
}
