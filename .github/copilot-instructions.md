# Copilot Guidelines for IDC.DBDeployTools Project

## Project Architecture Overview
This is a .NET 8.0 web API template with modular dependency injection, configuration-driven middleware, and extensive customization features.

### Key Components
- **Configuration Handlers**: `AppConfigsHandler` and `AppSettingsHandler` manage JSON configurations with dot notation paths (`_config.PropGet("Security.Cors.Enabled")`)
- **Partial Program Classes**: Main setup split across `Program.cs`, `Program.DI.cs`, `Program.Middlewares.cs`, `Program.Services.cs`, etc.
- **Controller Pattern**: Split controllers by feature (e.g., `DemoController.Cache.cs`, `DemoController.ApiKey.cs`)
- **IDC.Utilities Dependency**: External utility library referenced as local DLL at `D:\- Works\SCM\idc.utility\bin\Release\net8.0\IDC.Utilities.dll`

### Configuration Files
- `appconfigs.jsonc`: Runtime configuration with comments support, used by `AppConfigsHandler`
- `appsettings.json`: Standard ASP.NET settings, used by `AppSettingsHandler`  
- `endpoint_generator.jsonc`: Dynamic API endpoint generation definitions

## Coding Standards
- Always add argument names when calling methods: `config.Get(path: "app.name", defaultValue: "default")`
- Implement nullable and null safety throughout
- Use simplified collection initialization and collection expressions
- Functions should return the class type to enable method chaining
- Controllers use primary constructors: `public class DemoController(SystemLogging systemLogging, Language language)`
- Jangan menggunakan kurung kurawal "{}" untuk perintah yang hanya satu baris.
- Tidak perlu deklarasi variable, jika digunakan hanya sekali langsung gunakan saja isiannya.

## Documentation Standards
- Use English with formal tone for XML documentation
- Include comprehensive sections: Summary, Remarks (with examples), Parameters, Returns, Exceptions
- Generate documentation for private/internal methods too
- Use DocFX-compatible XML with `<example>`, `<code>` tags
- Limit lines to 100 characters maximum
- Use DocFX alerts: `> [!NOTE]`, `> [!TIP]`, `> [!IMPORTANT]`, `> [!CAUTION]`, `> [!WARNING]`
- Always include working code examples in `<remarks>` sections

## Project-Specific Patterns
### Configuration Access
```csharp
// Use dot notation for nested config access
var isEnabled = _appConfigs.Get<bool>(path: "Security.Cors.Enabled");
var maxItems = _appConfigs.Get(path: "app.settings.maxItems", defaultValue: 100);
```

### Middleware Configuration
- Middleware order matters: Request Logging → Rate Limiting → Response Compression → Security Headers → API Key Auth
- All middleware is conditionally enabled via `appconfigs.jsonc`
- API Key Authentication excludes paths: Swagger UI, CSS, JS, themes, images

### Controller Organization
- Split controllers by feature area into separate partial files
- Use `[ApiExplorerSettings(GroupName = "Demo")]` for Swagger grouping
- Primary constructor injection pattern throughout

### Dependency Injection Setup
- All DI configuration in `Program.DI.cs` via `SetupDI()` method
- Language, logging, caching, databases configured separately
- Scoped registration for middleware: `builder.Services.AddScoped<ApiKeyAuthenticationMiddleware>()`

## Development Workflow
- Build scripts in `.vscode/` for cross-platform: `build.ps1`, `build.sh`, `build.bat`
- Swagger UI with theme switching at runtime
- Configuration changes persist automatically to `appconfigs.jsonc`
- Dispose pattern implementation required for configuration handlers

## Method Variants
- Create async versions with callback parameters and cancellation tokens
- Do not modify existing synchronous methods when adding async variants

## Communication
- Explanations in Bahasa Indonesia when needed
- Code speaks for itself - minimal explanations required
