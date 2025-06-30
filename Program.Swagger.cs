using System.Reflection;
using ScriptDeployerWeb.Utilities.DI;
using ScriptDeployerWeb.Utilities.Middlewares.Swagger;
using ScriptDeployerWeb.Utilities.Models;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;

internal partial class Program
{
    /// <summary>
    /// Configures Swagger/OpenAPI documentation generation for the application
    /// </summary>
    /// <param name="builder"><see cref="WebApplicationBuilder"/> instance for configuring application services</param>
    /// <returns>void</returns>
    /// <remarks>
    /// Configures comprehensive Swagger documentation with advanced features and customization options.
    ///
    /// Configuration Structure:
    /// <example>
    /// <code>
    /// {
    ///   "SwaggerConfig": {
    ///     "UI": {
    ///       "Enable": true,
    ///       "SortEndpoints": true,
    ///       "Theme": "/themes/theme-monokai-dark.css"
    ///     },
    ///     "OpenApiInfo": {
    ///       "Title": "API Documentation",
    ///       "Version": "v2",
    ///       "Description": "Comprehensive API documentation",
    ///       "TermsOfService": "/openapi/terms.html",
    ///       "Contact": {
    ///         "Name": "Support Team",
    ///         "Email": "support@example.com",
    ///         "Url": "https://support.example.com"
    ///       },
    ///       "License": {
    ///         "Name": "Proprietary License",
    ///         "Url": "/openapi/license.html"
    ///       }
    ///     },
    ///     "Security": {
    ///       "ApiKey": {
    ///         "Name": "X-API-Key",
    ///         "In": "header"
    ///       }
    ///     }
    ///   }
    /// }
    /// </code>
    /// </example>
    ///
    /// Implementation Example:
    /// <example>
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// ConfigureSwagger(builder: builder);
    /// var app = builder.Build();
    /// </code>
    /// </example>
    ///
    /// > [!IMPORTANT]
    /// > XML documentation must be enabled in project settings for full documentation support
    ///
    /// > [!NOTE]
    /// > Endpoints are automatically grouped into Main and Demo categories
    ///
    /// > [!TIP]
    /// > Use tags to organize endpoints into logical groups
    ///
    /// > [!CAUTION]
    /// > Sensitive information should not be included in API documentation
    ///
    /// > [!WARNING]
    /// > Disable Swagger UI in production unless specifically required
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when configuration is invalid or XML documentation is missing</exception>
    /// <seealso cref="DefaultGroupDocFilter"/>
    /// <seealso cref="DocumentSortDocFilter"/>
    /// <seealso cref="ConfigureSwaggerUI"/>
    /// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger">Microsoft Swagger Documentation</seealso>
    /// <seealso href="https://swagger.io/specification/">OpenAPI Specification</seealso>
    /// <seealso href="https://github.com/domaindrivendev/Swashbuckle.AspNetCore">Swashbuckle Documentation</seealso>
    private static void ConfigureSwagger(WebApplicationBuilder builder)
    {
        if (_appConfigs.Get<bool>(path: "SwaggerConfig.UI.Enable") == false)
            return;

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            var openApiInfo = new OpenApiInfo
            {
                Title = _appConfigs.Get<string>(path: "SwaggerConfig.OpenApiInfo.Title")!,
                Version = _appConfigs.Get<string>(path: "SwaggerConfig.OpenApiInfo.Version")!,
                Description = _appConfigs.Get<string>(
                    path: "SwaggerConfig.OpenApiInfo.Description"
                )!,
                TermsOfService = new Uri(
                    _appConfigs.Get<string>(path: "SwaggerConfig.OpenApiInfo.TermsOfService")!,
                    UriKind.Relative
                ),
                Contact = new OpenApiContact
                {
                    Name = _appConfigs.Get<string>(path: "SwaggerConfig.OpenApiInfo.Contact.Name")!,
                    Email = _appConfigs.Get<string>(
                        path: "SwaggerConfig.OpenApiInfo.Contact.Email"
                    )!,
                    Url = new Uri(
                        _appConfigs.Get<string>(path: "SwaggerConfig.OpenApiInfo.Contact.Url")!
                    ),
                },
                License = new OpenApiLicense
                {
                    Name = _appConfigs.Get<string>(path: "SwaggerConfig.OpenApiInfo.License.Name")!,
                    Url = new Uri(
                        _appConfigs.Get<string>(path: "SwaggerConfig.OpenApiInfo.License.Url")!,
                        UriKind.Relative
                    ),
                },
            };

            // Tambahkan resolver untuk menangani konflik action
            options.ResolveConflictingActions(apiDescriptions =>
            {
                // Prioritaskan controller dari namespace IDC.Template
                return apiDescriptions.FirstOrDefault(api =>
                        api.ActionDescriptor.DisplayName?.Contains(
                            value: _appConfigs.Get<string>(
                                path: "AppName",
                                defaultValue: "IDC.Template"
                            )
                        ) == true
                    ) ?? apiDescriptions.First();
            });

            options.SwaggerDoc(name: "Main", info: openApiInfo);
            options.SwaggerDoc(
                name: "Demo",
                info: new OpenApiInfo
                {
                    Title = "Demo API",
                    Version = openApiInfo.Version,
                    Description = openApiInfo.Description,
                    TermsOfService = openApiInfo.TermsOfService,
                    Contact = openApiInfo.Contact,
                    License = openApiInfo.License,
                }
            );

            // Konfigurasi untuk mengelompokkan berdasarkan Tags
            options.TagActionsBy(api =>
                [
                    .. api
                        .ActionDescriptor.EndpointMetadata.OfType<TagsAttribute>()
                        .SelectMany(attr => attr.Tags)
                        .Distinct(),
                ]
            );

            // Urutkan Tags
            options.OrderActionsBy(apiDesc => apiDesc.GroupName);

            options.AddSecurityDefinition(
                "ApiKey",
                new OpenApiSecurityScheme
                {
                    Description = "API Key authentication using the 'X-API-Key' header",
                    Type = SecuritySchemeType.ApiKey,
                    Name = "X-API-Key",
                    In = ParameterLocation.Header,
                    Scheme = "ApiKeyScheme",
                }
            );

            options.AddSecurityRequirement(
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "ApiKey",
                            },
                        },
                        Array.Empty<string>()
                    },
                }
            );

            options.DocInclusionPredicate(
                (docName, api) =>
                {
                    // Exclude endpoints from MongoDB reference DLLs
                    if (api.RelativePath != null && ExcludeAPIPath(api.RelativePath))
                        return false;

                    if (docName == "Demo")
                        return api.RelativePath?.ToLower().Contains("api/demo/") == true
                            || api.GroupName?.Equals("Demo", StringComparison.OrdinalIgnoreCase)
                                == true;

                    if (docName == "Main")
                        return api.GroupName?.Equals("Main", StringComparison.OrdinalIgnoreCase)
                                == true
                            || api.GroupName == null;

                    return true;
                }
            );

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);

            options.DocumentFilter<DefaultGroupDocFilter>();
            if (_appConfigs.Get<bool>(path: "SwaggerConfig.UI.SortEndpoints"))
                options.DocumentFilter<DocumentSortDocFilter>();
        });
    }

    /// <summary>
    /// Configures Swagger UI middleware for API documentation visualization
    /// </summary>
    /// <param name="app">The web application instance for middleware configuration</param>
    /// <returns>void</returns>
    /// <remarks>
    /// Initializes and configures Swagger UI middleware with comprehensive API documentation features.
    ///
    /// Features:
    /// - Main API documentation endpoint
    /// - Demo/testing API documentation
    /// - External API endpoints integration
    /// - Customizable UI theming
    /// - Responsive layout
    ///
    /// Configuration Structure:
    /// <example>
    /// <code>
    /// {
    ///   "SwaggerConfig": {
    ///     "UI": {
    ///       "Enable": true,
    ///       "Theme": "/themes/custom-theme.css"
    ///     }
    ///   },
    ///   "SwaggerList": [
    ///     {
    ///       "Name": "External Service",
    ///       "URL": "https://api.external.com/swagger/v1/swagger.json"
    ///     }
    ///   ]
    /// }
    /// </code>
    /// </example>
    ///
    /// Implementation Example:
    /// <example>
    /// <code>
    /// var app = builder.Build();
    /// ConfigureSwaggerUI(app: app);
    /// app.Run();
    /// </code>
    /// </example>
    ///
    /// > [!IMPORTANT]
    /// > Swagger UI is only enabled when SwaggerConfig.UI.Enable is set to true
    ///
    /// > [!NOTE]
    /// > All endpoints are automatically grouped and sorted for better organization
    ///
    /// > [!TIP]
    /// > Use SwaggerList configuration to integrate external API documentation
    ///
    /// > [!CAUTION]
    /// > Ensure proper security measures when exposing API documentation in production
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when configuration is invalid or missing required settings</exception>
    /// <seealso cref="SwaggerEndpoint"/>
    /// <seealso cref="ConfigureMainEndpoint"/>
    /// <seealso cref="ConfigureDemoEndpoint"/>
    /// <seealso cref="ConfigureAdditionalEndpoints"/>
    /// <seealso href="https://swagger.io/tools/swagger-ui/">Swagger UI Documentation</seealso>
    /// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger">Microsoft Swagger Integration Guide</seealso>
    private static void ConfigureSwaggerUI(WebApplication app)
    {
        if (!_appConfigs.Get<bool>(path: "SwaggerConfig.UI.Enable"))
            return;

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            // Main endpoints
            ConfigureMainEndpoint(options, _appConfigs);

            // Demo endpoints
            ConfigureDemoEndpoint(options);

            // Additional endpoints from SwaggerList
            ConfigureAdditionalEndpoints(options, app, _appConfigs);

            ConfigureSwaggerUIStyle(options);
        });
    }

    /// <summary>
    /// Configures main API endpoint in Swagger UI
    /// </summary>
    /// <param name="options"><see cref="SwaggerUIOptions"/> instance for configuring the Swagger UI display settings</param>
    /// <param name="appConfigs"><see cref="AppConfigsHandler"/> instance for accessing application configuration values</param>
    /// <returns>void</returns>
    /// <remarks>
    /// Sets up the main API documentation endpoint with customizable configuration from appconfigs.jsonc.
    /// This method configures the primary API documentation endpoint that serves as the default view in Swagger UI.
    ///
    /// Example configuration in appconfigs.jsonc:
    /// <example>
    /// <code>
    /// {
    ///   "AppName": "My API Service",
    ///   "SwaggerConfig": {
    ///     "UI": {
    ///       "Enable": true
    ///     }
    ///   }
    /// }
    /// </code>
    /// </example>
    ///
    /// Example usage:
    /// <example>
    /// <code>
    /// app.UseSwaggerUI(options =>
    /// {
    ///     ConfigureMainEndpoint(
    ///         options: options,
    ///         appConfigs: configHandler
    ///     );
    /// });
    /// </code>
    /// </example>
    ///
    /// > [!NOTE]
    /// > The endpoint URL is fixed to "/swagger/Main/swagger.json"
    ///
    /// > [!TIP]
    /// > Configure AppName in appconfigs.jsonc to customize the display name
    ///
    /// > [!IMPORTANT]
    /// > Ensure SwaggerConfig.UI.Enable is set to true for the endpoint to be available
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when options or appConfigs is null</exception>
    /// <seealso cref="ConfigureSwaggerUI"/>
    /// <seealso cref="SwaggerUIOptions"/>
    /// <seealso href="https://swagger.io/docs/specification/basic-structure/">Swagger Basic Structure</seealso>
    private static void ConfigureMainEndpoint(
        SwaggerUIOptions options,
        AppConfigsHandler appConfigs
    )
    {
        options.SwaggerEndpoint(
            url: "/swagger/Main/swagger.json",
            name: appConfigs.Get(path: "AppName", defaultValue: "IDC Template API")
        );
    }

    /// <summary>
    /// Configures demo API endpoint in Swagger UI for testing and demonstration purposes
    /// </summary>
    /// <param name="options">
    /// <see cref="SwaggerUIOptions"/> instance for configuring Swagger UI display
    /// </param>
    /// <returns>void</returns>
    /// <remarks>
    /// Sets up a dedicated endpoint for demo/testing API documentation with predefined configuration.
    /// This endpoint is separate from the main API documentation to isolate demonstration features.
    ///
    /// The demo endpoint:
    /// - Uses a fixed URL path "/swagger/Demo/swagger.json"
    /// - Has a predefined name "IDC Template Demo API"
    /// - Is automatically filtered in the UI to only show demo-related endpoints
    ///
    /// Example usage:
    /// <example>
    /// <code>
    /// app.UseSwaggerUI(options =>
    /// {
    ///     ConfigureDemoEndpoint(options: options);
    /// });
    /// </code>
    /// </example>
    ///
    /// > [!NOTE]
    /// > Demo endpoints are typically prefixed with "/api/demo/" in the URL path
    ///
    /// > [!TIP]
    /// > Use this endpoint for testing new features or demonstrating API capabilities
    ///
    /// > [!IMPORTANT]
    /// > Ensure demo endpoints are properly secured in production environments
    /// </remarks>
    /// <seealso cref="ConfigureSwaggerUI"/>
    /// <seealso cref="ConfigureMainEndpoint"/>
    /// <seealso href="https://swagger.io/docs/specification/paths-and-operations/">
    /// Swagger Paths and Operations Documentation
    /// </seealso>
    private static void ConfigureDemoEndpoint(SwaggerUIOptions options)
    {
        options.SwaggerEndpoint(url: "/swagger/Demo/swagger.json", name: "IDC Template Demo API");
    }

    /// <summary>
    /// Configures additional API endpoints in Swagger UI from external sources defined in configuration
    /// </summary>
    /// <param name="options">SwaggerUI configuration options instance for customizing endpoint display</param>
    /// <param name="app">Web application instance containing configuration and services</param>
    /// <param name="appConfigs">Application configuration handler for accessing settings</param>
    /// <remarks>
    /// Adds external API documentation endpoints from SwaggerList configuration while filtering out duplicates.
    /// Supports dynamic loading of multiple API documentations from different sources.
    ///
    /// Example configuration in appconfigs.jsonc:
    /// <example>
    /// <code>
    /// {
    ///   "SwaggerList": [
    ///     {
    ///       "Name": "Partner API",
    ///       "URL": "https://api.partner.com/swagger/v1/swagger.json"
    ///     },
    ///     {
    ///       "Name": "Legacy API",
    ///       "URL": "https://legacy.internal/docs/swagger.json"
    ///     }
    ///   ]
    /// }
    /// </code>
    /// </example>
    ///
    /// Example usage in Program.cs:
    /// <example>
    /// <code>
    /// app.UseSwaggerUI(options =>
    /// {
    ///     ConfigureAdditionalEndpoints(
    ///         options: options,
    ///         app: app,
    ///         appConfigs: configHandler
    ///     );
    /// });
    /// </code>
    /// </example>
    ///
    /// > [!IMPORTANT]
    /// > Endpoints must have unique names to avoid conflicts in the UI
    ///
    /// > [!NOTE]
    /// > Main and Demo API endpoints are automatically excluded to prevent duplication
    ///
    /// > [!TIP]
    /// > Use meaningful names in SwaggerList to easily identify different API sources
    /// </remarks>
    /// <seealso cref="SwaggerEndpoint"/>
    /// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle">
    /// ASP.NET Core Swagger Documentation
    /// </seealso>
    private static void ConfigureAdditionalEndpoints(
        SwaggerUIOptions options,
        WebApplication app,
        AppConfigsHandler appConfigs
    )
    {
        var swaggerList = app
            .Configuration.GetSection("SwaggerList")
            .Get<List<SwaggerEndpoint>>()
            ?.Where(endpoint =>
                endpoint.Name != appConfigs.Get(path: "AppName", defaultValue: "IDC Template API")
                && endpoint.Name != "IDC Template Demo API"
            )
            .OrderBy(endpoint => endpoint.Name);

        if (swaggerList != null)
        {
            foreach (var endpoint in swaggerList)
            {
                options.SwaggerEndpoint(url: endpoint.URL, name: endpoint.Name);
            }
        }
    }

    /// <summary>
    /// Configures Swagger UI styling and customization options for the application.
    /// </summary>
    /// <param name="options">The SwaggerUI configuration options instance to customize UI appearance and behavior</param>
    /// <remarks>
    /// Applies styling customizations to Swagger UI including:
    /// - Custom document title with application name
    /// - Theme configuration via stylesheet injection
    /// - Custom CSS styling overrides
    /// - Dynamic theme switching capability
    ///
    /// Example configuration in appconfigs.jsonc:
    /// <code>
    /// {
    ///   "SwaggerConfig": {
    ///     "UI": {
    ///       "Theme": "/themes/theme-monokai-dark.css"
    ///     }
    ///   }
    /// }
    /// </code>
    ///
    /// Example usage:
    /// <example>
    /// <code>
    /// app.UseSwaggerUI(options =>
    /// {
    ///     ConfigureSwaggerUIStyle(options: options);
    /// });
    /// </code>
    /// </example>
    ///
    /// > [!TIP]
    /// > Configure custom themes in SwaggerConfig.UI.Theme setting
    ///
    /// > [!NOTE]
    /// > Custom CSS and JavaScript files must be placed in wwwroot directory
    ///
    /// > [!IMPORTANT]
    /// > Theme switcher requires swagger-theme-switcher.js to be loaded
    /// </remarks>
    /// <seealso cref="SwaggerUIOptions"/>
    /// <seealso href="https://swagger.io/docs/open-source-tools/swagger-ui/usage/configuration/">Swagger UI Configuration</seealso>
    /// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle">ASP.NET Core Swagger Documentation</seealso>
    private static void ConfigureSwaggerUIStyle(SwaggerUIOptions options)
    {
        options.DocumentTitle =
            $"[SUI] {_appConfigs.Get(path: "AppName", defaultValue: "IDC Template API")}";

        options.InjectStylesheet(
            _appConfigs.Get(
                path: "SwaggerConfig.UI.Theme",
                defaultValue: "/themes/theme-monokai-dark.css"
            )!
        );
        options.InjectStylesheet("/_content/IDC.Template/css/swagger-custom.css");

        options.HeadContent =
            @"
                <link rel='stylesheet' type='text/css' href='/css/swagger-custom.css' />
            ";

        options.InjectJavascript("/js/swagger-theme-switcher.js");
    }

    /// <summary>
    /// Determines if an API path should be excluded from Swagger documentation
    /// </summary>
    /// <param name="path">The API path to check</param>
    /// <returns>True if the path should be excluded, otherwise false</returns>
    /// <remarks>
    /// Checks if a path matches any of the excluded paths defined in configuration.
    ///
    /// Example configuration:
    /// <example>
    /// <code>
    /// {
    ///   "SwaggerConfig": {
    ///     "ExcludedPaths": [
    ///       "api/Mongo/",
    ///       "api/Internal/",
    ///       "api/Legacy/"
    ///     ]
    ///   }
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    private static bool ExcludeAPIPath(string path)
    {
        var excludedPaths = _appConfigs.Get<List<string>>(
            path: "SwaggerConfig.ExcludedPaths",
            defaultValue: []
        );

        return excludedPaths?.Any(excluded =>
                path.StartsWith(excluded, StringComparison.OrdinalIgnoreCase)
            ) ?? false;
    }
}
