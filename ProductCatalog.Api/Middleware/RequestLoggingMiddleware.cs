using System.Diagnostics;

namespace ProductCatalog.Api.Middleware;

// Logs request details and response time for every HTTP request.
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _nextDelegate;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate nextDelegate, ILogger<RequestLoggingMiddleware> logger)
    {
        _nextDelegate = nextDelegate;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        Stopwatch timer = Stopwatch.StartNew();
        
        try
        {
            await _nextDelegate(httpContext);
        }
        finally
        {
            timer.Stop();
            
            _logger.LogInformation(
                "HTTP {RequestMethod} {RequestPath} completed in {ElapsedMilliseconds} ms with status code {StatusCode}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                timer.ElapsedMilliseconds,
                httpContext.Response.StatusCode);
        }
    }
}
