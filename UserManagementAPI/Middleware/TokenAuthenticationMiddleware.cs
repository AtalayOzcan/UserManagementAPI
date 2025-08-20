using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace UserManagementApi.Middleware;

public sealed class TokenAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenAuthenticationMiddleware> _logger;
    private readonly string? _expectedToken;

    public TokenAuthenticationMiddleware(
        RequestDelegate next,
        ILogger<TokenAuthenticationMiddleware> logger,
        IConfiguration config)
    {
        _next = next;
        _logger = logger;
        // appsettings.json → "Auth:Token": "my-secret-token"
        _expectedToken = config["Auth:Token"];
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Swagger'ı engellemek istemiyorsan şu satırı bırak:
        if ((context.Request.Path.Value ?? string.Empty).StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("Authorization", out var auth))
        {
            await WriteUnauthorizedAsync(context);
            return;
        }

        var value = auth.ToString();
        // "Bearer <token>" formatı
        var parts = value.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 || !parts[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase))
        {
            await WriteUnauthorizedAsync(context);
            return;
        }

        var token = parts[1];

        if (!ValidateToken(token))
        {
            await WriteUnauthorizedAsync(context);
            return;
        }

        // Geçerli → devam
        await _next(context);
    }

    private bool ValidateToken(string token)
    {
        // 1) appsettings.json üzerinden beklenen token tanımlıysa onu kullan
        if (!string.IsNullOrEmpty(_expectedToken))
            return token == _expectedToken;

        // 2) Aksi halde demo amaçlı sabit bir token kabul et
        return token == "my-secret-token";
    }

    private static async Task WriteUnauthorizedAsync(HttpContext context)
    {
        if (context.Response.HasStarted) return;

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json; charset=utf-8";
        await context.Response.WriteAsync("""{ "error": "Unauthorized" }""");
    }
}
