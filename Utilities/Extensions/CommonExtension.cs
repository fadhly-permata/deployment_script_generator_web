namespace IDC.DBDeployTools.Utilities.Extensions;

internal static class CommonExtension
{
    /// <summary>
    /// Builds a new URI string based on the specified base URI and optional components.
    /// </summary>
    /// <remarks>
    /// This method allows you to construct a new URI by replacing specific components of the
    /// original URI, such as the query, fragment, path, scheme, host, or port.
    /// <example>
    /// <code>
    /// string baseUri = "https://example.com/api/resource";
    /// string newUri = baseUri.UriBuilder(
    ///     query: "id=123",
    ///     fragment: "section1",
    ///     path: "/api/other",
    ///     scheme: "https",
    ///     host: "api.example.com",
    ///     port: 8080
    /// );
    /// // Result: "https://api.example.com:8080/api/other?id=123#section1"
    /// </code>
    /// </example>
    /// </remarks>
    /// <param name="uri">The base URI string to start from.</param>
    /// <param name="query">An optional query string to set. If null, retains the original query.</param>
    /// <param name="fragment">An optional fragment to set. If null, retains the original fragment.</param>
    /// <param name="path">An optional path to set. If null, retains the original path.</param>
    /// <param name="scheme">An optional scheme to set (e.g., "http", "https"). If null, retains the original scheme.</param>
    /// <param name="host">An optional host to set. If null, retains the original host.</param>
    /// <param name="port">
    /// An optional port to set. If 0, retains the original port.
    /// </param>
    /// <returns>
    /// A string representing the newly constructed URI with the specified components replaced as provided.
    /// </returns>
    /// <exception cref="System.UriFormatException">
    /// Thrown if the input URI string is not valid.
    /// </exception>
    internal static string UriBuilder(
        this string uri,
        string? query = null,
        string? fragment = null,
        string? path = null,
        string? scheme = null,
        string? host = null,
        int port = 0
    )
    {
        var builder = new UriBuilder(uri);

        builder.Query = query ?? builder.Query;
        builder.Fragment = fragment ?? builder.Fragment;
        builder.Path = path ?? builder.Path;
        builder.Scheme = scheme ?? builder.Scheme;
        builder.Host = host ?? builder.Host;
        builder.Port = port == 0 ? builder.Port : port;

        return builder.ToString();
    }

    internal static string UriBuilder(this string uri, string? path)
    {
        var builder = new UriBuilder(uri);
        builder.Path = ($"{builder.Path}/{path}" ?? builder.Path).Replace(
            oldValue: "//",
            newValue: "/"
        );
        return builder.ToString();
    }

    internal static string MapPathWithUri(this string? path, string uri)
    {
        return string.IsNullOrEmpty(path) ? uri : new UriBuilder(uri) { Path = path }.ToString();
    }
}
