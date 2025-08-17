# Changelog

## [1.0.9] - 2025-04-29
### Changed
- Refactor dependency injection setup for improved service registration and maintainability
- Simplified and optimized DI configuration in `Program.DI.cs` and `Program.DI.Services.cs`
- Updated `IDC.Template.csproj` to remove MongoDB.Driver package reference

### Removed
- Removed MongoDB DI configuration and related code from DI setup

### Files Affected
- `IDC.Template.csproj`
- `Program.DI.Services.cs`
- `Program.DI.cs`

## [1.0.8] - 2025-04-29
### Changed
- Updated resource paths from `IDC.UDMongo` to `IDC.Template` (themes, CSS, JS, `Program.Swagger.cs`, etc.)

### Added
- Added `.gitignore` file to exclude build folders, dependencies, logs, and `copilot-guidelines.txt`

### Removed
- Deleted `Controllers/MongoController.cs` (MongoDB controller endpoints removed)

### Files Affected
- `.gitignore`
- `Controllers/DemoController.ApiKey.cs`
- `Controllers/DemoController.Cache.cs`
- `Controllers/DemoController.Language.cs`
- `Controllers/DemoController.SQLiteMemory.cs`
- `Controllers/DemoController.SystemLogging.cs`
- `Controllers/DemoController.cs`
- `Controllers/MongoController.cs`
- `wwwroot/themes/*`
- `wwwroot/css/swagger-custom.css`
- `wwwroot/js/swagger-theme-switcher.js`
- `Program.Swagger.cs`

## [1.0.7] - 2025-04-28
### Added

### Files Affected
- `CHANGELOG.md`
- `README.md`

## [1.0.6] - 2025-04-28
### Added
	- `DemoController.ApiKey.cs`: API key management endpoints
	- `DemoController.Cache.cs`: cache management endpoints
	- `DemoController.Language.cs`: language/message endpoints
	- `DemoController.SQLiteMemory.cs`: SQLite in-memory database endpoints
	- `DemoController.SystemLogging.cs`: system logging endpoints

### Changed
- Refactor and simplify `DemoController.cs` to focus on demo operations

### Removed
- Deleted `CouchbaseDemoController.cs` (Couchbase demo endpoints removed)
- Deleted `.gitignore` file

### Files Affected
- `Controllers/DemoController.ApiKey.cs`
- `Controllers/DemoController.Cache.cs`
- `Controllers/DemoController.Language.cs`
- `Controllers/DemoController.SQLiteMemory.cs`
- `Controllers/DemoController.SystemLogging.cs`
- `Controllers/DemoController.cs`
- `Controllers/CouchbaseDemoController.cs`
- `.gitignore`

## [1.0.5] - 2024-11-27
### Changed

### Files Affected
- `wwwroot/dependencies/IDX.Utilities.dll`
- `wwwroot/dependencies/IDX.Utilities.pdb`

## [1.0.4] - 2024-11-27
### Changed

### Files Affected
- `README.md`

## [1.0.3] - 2024-11-27
### Changed
- Reorder using directives in `ModelStateInvalidFilters` middleware for clarity

### Fixed
- Improved code readability and maintainability in SQL query and middleware files

### Files Affected
- `Controllers/CouchbaseDemoController.cs`
- `Utilities/Middlewares/ModelStateInvalidFilters.cs`

## [1.0.2] - 2024-11-27
### Changed
- Changed default log file path to wwwroot/Logs/Logs.txt
- Added minimum log level and force write to file settings
- Renamed AppName to "idc.template" in config and code
- Enhanced error handler to show stack trace in development mode
- Added default date format constants in GeneralConstant
- Improved ReadConfigurationValue method for null safety and empty string handling
- Added Visual Studio solution file (`idc.template.sln`)
- Updated IDX.Utilities.dll and IDX.Utilities.pdb dependencies

### Added
- New endpoints in DemoController for sample, cache, and HTTP client
- Added endpoints in DemoController for PostgreSQL (raw query, store procedure, scalar, auto paging)

### Fixed
- Fixed Swagger UI theme CSS processing


## [1.0.1] - 2024-11-27
### Added
- CouchbaseDemoController: API endpoints for Couchbase N1QL query, insert, update, delete
- DemoController: API endpoints for cache, HTTP client, model validation, PostgreSQL operations

## [1.0.0] - 2024-11-26
### Added
- Initial release of IDC.Template .NET 8.0 Web API project template
- Modular dependency injection and configuration-driven middleware
- Dynamic API endpoint generator
- Feature-based controller organization
- Local dependency to IDC.Utilities
- Runtime config via appconfigs.jsonc (with comments)
- Swagger UI with theme switching
- Build scripts for Windows, Linux, MacOS
- Automatic config persistence
- Comprehensive XML documentation pattern
