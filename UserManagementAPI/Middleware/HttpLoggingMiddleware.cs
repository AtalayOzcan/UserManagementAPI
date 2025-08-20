using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace UserManagementApi.Middleware;

public sealed class HttpLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HttpLoggingMiddleware> _logger;

    public HttpLoggingMiddleware(RequestDelegate next, ILogger<HttpLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method;
        var path = context.Request.Path + context.Request.QueryString;

        await _next(context); // sıradaki middleware/endpoint çalışır

        var status = context.Response.StatusCode;
        _logger.LogInformation("HTTP {Method} {Path} -> {StatusCode}", method, path, status);
    }
}
