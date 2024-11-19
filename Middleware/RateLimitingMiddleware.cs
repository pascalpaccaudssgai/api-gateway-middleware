using System.Collections.Concurrent;
using System.Net;
using ApiGateway.Models;
using Microsoft.Extensions.Options;

namespace ApiGateway.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimitConfig _config;
    private readonly ConcurrentDictionary<string, RateLimitEntry> _rateLimitEntries;

    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger,
        IOptions<RateLimitConfig> config)
    {
        _next = next;
        _logger = logger;
        _config = config.Value;
        _rateLimitEntries = new ConcurrentDictionary<string, RateLimitEntry>();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_config.EnableRateLimiting)
        {
            await _next(context);
            return;
        }

        var clientId = GetClientIdentifier(context);
        var entry = _rateLimitEntries.GetOrAdd(clientId, _ => new RateLimitEntry { ClientId = clientId });

        if (IsRateLimitExceeded(entry))
        {
            _logger.LogWarning("Rate limit exceeded for client {ClientId}", clientId);
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            await context.Response.WriteAsJsonAsync(new ApiErrorResponse
            {
                Error = new ApiError
                {
                    Code = "RateLimitExceeded",
                    Message = "Too many requests. Please try again later.",
                    Details = $"Maximum {_config.RequestsPerMinute} requests per minute allowed."
                }
            });
            return;
        }

        UpdateRateLimitEntry(entry);
        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // Try to get API key from header
        var apiKey = context.Request.Headers["X-API-Key"].FirstOrDefault();
        if (!string.IsNullOrEmpty(apiKey))
        {
            return apiKey;
        }

        // Fall back to IP address
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private bool IsRateLimitExceeded(RateLimitEntry entry)
    {
        var now = DateTimeOffset.UtcNow;
        var minuteAgo = now.AddMinutes(-1);

        // Remove requests older than 1 minute
        while (entry.RequestHistory.Count > 0 && entry.RequestHistory.Peek() < minuteAgo)
        {
            entry.RequestHistory.Dequeue();
        }

        return entry.RequestHistory.Count >= _config.RequestsPerMinute;
    }

    private void UpdateRateLimitEntry(RateLimitEntry entry)
    {
        var now = DateTimeOffset.UtcNow;
        entry.LastRequest = now;
        entry.RequestCount++;
        entry.RequestHistory.Enqueue(now);

        // Ensure we don't keep more history than necessary
        while (entry.RequestHistory.Count > _config.RequestsPerMinute)
        {
            entry.RequestHistory.Dequeue();
        }
    }
}

public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>();
    }
}
