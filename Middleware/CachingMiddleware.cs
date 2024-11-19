using System.Text;
using ApiGateway.Services;
using Microsoft.IO;

namespace ApiGateway.Middleware;

public class CachingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachingMiddleware> _logger;
    private static readonly RecyclableMemoryStreamManager _streamManager = new();

    public CachingMiddleware(
        RequestDelegate next,
        ICacheService cacheService,
        ILogger<CachingMiddleware> logger)
    {
        _next = next;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!IsCacheable(context.Request))
        {
            await _next(context);
            return;
        }

        var cacheKey = GetCacheKey(context.Request);
        var cachedResponse = await _cacheService.GetAsync<CachedResponse>(cacheKey);

        if (cachedResponse != null)
        {
            _logger.LogDebug("Cache hit for {Method} {Path}", context.Request.Method, context.Request.Path);
            await ApplyCachedResponse(context, cachedResponse);
            return;
        }

        var originalBody = context.Response.Body;
        using var responseStream = _streamManager.GetStream();
        context.Response.Body = responseStream;

        await _next(context);

        responseStream.Seek(0, SeekOrigin.Begin);
        await responseStream.CopyToAsync(originalBody);

        if (context.Response.StatusCode == 200)
        {
            responseStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(responseStream).ReadToEndAsync();
            
            var response = new CachedResponse
            {
                Body = responseBody,
                ContentType = context.Response.ContentType,
                StatusCode = context.Response.StatusCode,
                Headers = context.Response.Headers
                    .Where(h => h.Key != "Set-Cookie") // Don't cache cookies
                    .ToDictionary(h => h.Key, h => h.Value.ToArray())
            };

            await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));
        }

        context.Response.Body = originalBody;
    }

    private bool IsCacheable(HttpRequest request)
    {
        return request.Method == HttpMethod.Get.Method ||
               request.Method == HttpMethod.Head.Method;
    }

    private string GetCacheKey(HttpRequest request)
    {
        var keyBuilder = new StringBuilder();
        keyBuilder.Append($"{request.Method}:{request.Path}");

        foreach (var (key, value) in request.Query.OrderBy(q => q.Key))
        {
            keyBuilder.Append($":{key}={value}");
        }

        // Include relevant headers in cache key
        var headers = new[] { "Accept", "Accept-Language", "Accept-Encoding" };
        foreach (var header in headers)
        {
            if (request.Headers.TryGetValue(header, out var value))
            {
                keyBuilder.Append($":{header}={value}");
            }
        }

        return keyBuilder.ToString();
    }

    private async Task ApplyCachedResponse(HttpContext context, CachedResponse cachedResponse)
    {
        context.Response.StatusCode = cachedResponse.StatusCode;
        context.Response.ContentType = cachedResponse.ContentType;

        foreach (var header in cachedResponse.Headers)
        {
            context.Response.Headers[header.Key] = header.Value;
        }

        await context.Response.WriteAsync(cachedResponse.Body);
    }
}

public class CachedResponse
{
    public string Body { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public Dictionary<string, string[]> Headers { get; set; } = new();
}

public static class CachingMiddlewareExtensions
{
    public static IApplicationBuilder UseCaching(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CachingMiddleware>();
    }
}
