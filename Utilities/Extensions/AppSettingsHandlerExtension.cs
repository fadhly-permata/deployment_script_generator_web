using ScriptDeployerWeb.Utilities.DI;

namespace ScriptDeployerWeb.Utilities.Extensions;

internal static class AppSettingsHandlerExtension
{
    internal static string GetUriByName(
        this AppSettingsHandler appSettings,
        string name,
        string? path = null
    )
    {
        var uri =
            appSettings.Get<string>(path: $"APISettings.{name}")
            ?? throw new Exception(message: @$"API URL for ""{name}"" not found.");

        return uri.UriBuilder(path: path);
    }
}
