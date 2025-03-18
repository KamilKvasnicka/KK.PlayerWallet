using System.Diagnostics;

namespace KK.PlayerWalletApi.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path}");

            await _next(context); 

            stopwatch.Stop();
            _logger.LogInformation($"Response: {context.Response.StatusCode} ({stopwatch.ElapsedMilliseconds}ms)");
        }
    }

    public static class RequestLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}
