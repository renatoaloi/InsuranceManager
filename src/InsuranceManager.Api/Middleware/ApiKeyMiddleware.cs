using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace InsuranceManager.Api.Middleware;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeaderName = "X-API-Key";

    public ApiKeyMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
    {
        // Skip Swagger endpoints for development convenience
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        // Check if API Key header exists
        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = "API Key is missing." });
            return;
        }

        // Get configured API Key
        var apiKey = configuration.GetValue<string>("ApiKey");
        if (string.IsNullOrEmpty(apiKey))
        {
            // API Key not configured - reject all requests
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = "API Key not configured." });
            return;
        }

        // Validate API Key using constant-time comparison
        if (!string.Equals(apiKey, extractedApiKey.ToString(), StringComparison.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = "Invalid API Key." });
            return;
        }

        await _next(context);
    }
}

public static class ApiKeyMiddlewareExtensions
{
    public static IApplicationBuilder UseApiKeyAuthentication(this IApplicationBuilder app)
        => app.UseMiddleware<ApiKeyMiddleware>();
}