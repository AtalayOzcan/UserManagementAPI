using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace UserManagementApi.Middleware;

public sealed class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unhandled exception for {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await WriteErrorResponseAsync(context, ex);
        }
    }

    private async Task WriteErrorResponseAsync(HttpContext context, Exception ex)
    {
        // Yanıt başladıysa artık gövdeyi değiştiremeyiz; sadece loglayıp çık.
        if (context.Response.HasStarted)
        {
            _logger.LogWarning("Response has already started; cannot write error response for {Path}", context.Request.Path);
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = MapStatusCode(ex);
        context.Response.ContentType = "application/json; charset=utf-8";

        var payload = new
        {
            error = context.Response.StatusCode == (int)HttpStatusCode.InternalServerError
                ? "Internal server error."
                : ex.Message,
            traceId = context.TraceIdentifier
        };

        // İstersen global JsonOptions kullanabilirsin; burada basit tutuyoruz.
        var json = JsonSerializer.Serialize(payload);
        await context.Response.WriteAsync(json);
    }

    // Basit eşleme (ihtiyaca göre genişlet)
    private static int MapStatusCode(Exception ex) => ex switch
    {
        ArgumentException or ArgumentNullException or FormatException => StatusCodes.Status400BadRequest,
        KeyNotFoundException => StatusCodes.Status404NotFound,
        UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
        _ => StatusCodes.Status500InternalServerError
    };
}
