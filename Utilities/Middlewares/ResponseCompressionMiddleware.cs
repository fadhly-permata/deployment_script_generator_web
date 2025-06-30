using System.IO.Compression;

namespace ScriptDeployerWeb.Utilities.Middlewares;

/// <summary>
/// Middleware for compressing HTTP responses based on the client's Accept-Encoding header.
/// </summary>
/// <remarks>
/// This middleware examines the client's Accept-Encoding header and applies gzip or deflate compression
/// to the response body when supported. It helps reduce bandwidth usage and improve response times.
///
/// The middleware supports compression for the following content types:
/// - text/*
/// - application/json
/// - application/xml
/// - application/javascript
/// - application/swagger+json
/// - application/swagger-json
///
/// > [!IMPORTANT]
/// > Ensure this middleware is placed before any middleware that generates response bodies.
///
/// > [!NOTE]
/// > The middleware automatically selects the best compression method based on the Accept-Encoding header.
///
/// > [!TIP]
/// > For optimal performance, use gzip as it has broader browser support.
///
/// Example implementation in Program.cs:
/// <code>
/// var builder = WebApplication.CreateBuilder(args);
/// var app = builder.Build();
///
/// // Add compression middleware
/// app.UseMiddleware&lt;ResponseCompressionMiddleware&gt;();
///
/// // Other middleware
/// app.UseRouting();
/// app.UseEndpoints(...);
/// </code>
/// </remarks>
/// <param name="next">The delegate representing the next middleware in the pipeline</param>
/// <seealso href="https://learn.microsoft.com/en-us/aspnet/core/performance/response-compression">Response Compression in ASP.NET Core</seealso>
/// <seealso href="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Accept-Encoding">MDN: Accept-Encoding header</seealso>
public class ResponseCompressionMiddleware(RequestDelegate next)
{
    private static readonly string[] CompressibleTypes =
    [
        "text/",
        "application/json",
        "application/xml",
        "application/javascript",
        "application/swagger+json",
        "application/swagger-json",
    ];

    /// <summary>
    /// Processes individual HTTP requests and applies compression when applicable.
    /// </summary>
    /// <param name="context"><see cref="HttpContext"/> The HTTP context for the current request.</param>
    /// <returns><see cref="Task"/> A Task representing the asynchronous operation.</returns>
    /// <remarks>
    /// Performs compression based on the client's Accept-Encoding header and content type.
    /// Supports both gzip and deflate compression methods.
    ///
    /// Processing steps:
    /// <list type="number">
    ///   <item><description>Examines the Accept-Encoding header</description></item>
    ///   <item><description>Intercepts the response body using MemoryStream</description></item>
    ///   <item><description>Applies compression if content is compressible</description></item>
    ///   <item><description>Handles special status codes (204, 304)</description></item>
    /// </list>
    ///
    /// Example request format:
    /// <code>
    /// {
    ///   "headers": {
    ///     "Accept-Encoding": "gzip, deflate",
    ///     "Content-Type": "application/json"
    ///   },
    ///   "body": {
    ///     "data": "Sample content to be compressed"
    ///   }
    /// }
    /// </code>
    ///
    /// Example usage:
    /// <example>
    /// <code>
    /// app.UseMiddleware&lt;ResponseCompressionMiddleware&gt;();
    ///
    /// // Or with specific configuration
    /// public class CustomCompression : ResponseCompressionMiddleware
    /// {
    ///     public async Task InvokeAsync(HttpContext context)
    ///     {
    ///         if (ShouldCompress(context))
    ///         {
    ///             await base.InvokeAsync(context);
    ///         }
    ///         else
    ///         {
    ///             await _next(context);
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    ///
    /// > [!NOTE]
    /// > Compression is skipped for empty responses or non-compressible content types
    ///
    /// > [!TIP]
    /// > Configure this middleware early in the pipeline for optimal performance
    ///
    /// > [!IMPORTANT]
    /// > Memory usage increases temporarily during compression
    ///
    /// > [!CAUTION]
    /// > Avoid compressing already compressed content (e.g., images)
    ///
    /// > [!WARNING]
    /// > Large response bodies may impact server performance
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Thrown when stream has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when compression fails.</exception>
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.io.compression.gzipstream">GZipStream Class</seealso>
    /// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.io.compression.deflatestream">DeflateStream Class</seealso>
    /// <seealso href="https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Accept-Encoding">MDN: Accept-Encoding Header</seealso>
    public async Task InvokeAsync(HttpContext context)
    {
        var acceptEncoding = context.Request.Headers.AcceptEncoding.ToString().ToLower();

        if (string.IsNullOrEmpty(acceptEncoding))
        {
            await next(context);
            return;
        }

        await using var memoryStream = new MemoryStream();
        var originalBody = context.Response.Body;
        context.Response.Body = memoryStream;

        try
        {
            await next(context);

            if (
                context.Response.StatusCode == StatusCodes.Status204NoContent
                || context.Response.StatusCode == StatusCodes.Status304NotModified
            )
            {
                return;
            }

            var contentType = context.Response.ContentType?.ToLower() ?? string.Empty;
            var shouldCompress = CompressibleTypes.Any(type => contentType.Contains(type));

            if (!shouldCompress || memoryStream.Length == 0)
            {
                context.Response.Body = originalBody;
                memoryStream.Position = 0;
                await memoryStream.CopyToAsync(context.Response.Body);
                return;
            }

            context.Response.Headers.Remove("Content-Length");
            memoryStream.Position = 0;
            context.Response.Body = originalBody;

            if (acceptEncoding.Contains("gzip"))
            {
                context.Response.Headers.ContentEncoding = "gzip";
                await using var compressed = new GZipStream(
                    stream: context.Response.Body,
                    compressionLevel: CompressionLevel.Fastest,
                    leaveOpen: true
                );
                await memoryStream.CopyToAsync(compressed);
            }
            else if (acceptEncoding.Contains("deflate"))
            {
                context.Response.Headers.ContentEncoding = "deflate";
                await using var compressed = new DeflateStream(
                    stream: context.Response.Body,
                    compressionLevel: CompressionLevel.Fastest,
                    leaveOpen: true
                );
                await memoryStream.CopyToAsync(compressed);
            }
            else
            {
                await memoryStream.CopyToAsync(context.Response.Body);
            }
        }
        finally
        {
            context.Response.Body = originalBody;
        }
    }
}
