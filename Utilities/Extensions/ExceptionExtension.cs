using IDC.Utilities;

namespace IDC.DBDeployTools.Utilities.Extensions;

internal static class ExceptionExtension
{
    public static void AdditionalLoggingAction(
        this Exception exception,
        Language language,
        SystemLogging systemLogging
    )
    {
        // Log the exception using the system logging service
        systemLogging.LogError(message: $"Additional logging for exception: {exception.Message}");

        // TODO: Implement any additional logging actions here
    }
}
