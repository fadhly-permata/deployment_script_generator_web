using IDC.Utilities;

namespace ScriptDeployerWeb.Utilities.Extensions;

internal static class ExceptionExtension
{
    public static void AdditionalLoggingAction(
        this Exception exception,
        Language language,
        SystemLogging systemLogging
    )
    {
        // Log the exception using the system logging service
        systemLogging.LogError(exception: exception);

        // TODO: Implement any additional logging actions here
    }

    public static int? GetErrLine(this Exception ex)
    {
        var st = new System.Diagnostics.StackTrace(e: ex, fNeedFileInfo: true);
        return st.GetFrame(index: st.FrameCount - 1)?.GetFileLineNumber();
    }
}
