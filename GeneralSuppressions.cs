/*
    This file contains global code analysis suppressions for the IDC.DBDeployTools project.

    Summary:
    - Suppresses SonarAnalyzer warnings for specific members and namespaces.
    - Ensures intentional design choices are not flagged by static analysis tools.

    Remarks:
    > [!NOTE]
    These suppressions are applied at the assembly level and affect code analysis results.

    > [!TIP]
    Use this file to centralize and document all suppression attributes for maintainability.
*/


// Suppressions example for specified member
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    category: "SonarAnalyzer",
    checkId: "S1144",
    Scope = "member",
    Target = "~F:IDC.DBDeployTools.Controllers.Demo.CON_API_STATUS_FAILED",
    Justification = "Field may be used for future extensibility or via reflection."
)]

// Suppressions example for specified namespace
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    category: "SonarAnalyzer",
    checkId: "S1192",
    Scope = "namespaceanddescendants",
    Target = "~N:IDC.DBDeployTools",
    Justification = "String literals are intentional for configuration and extensibility."
)]
