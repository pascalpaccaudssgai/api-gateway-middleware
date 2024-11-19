using ApiGateway.Services;
using System.Net;

namespace ApiGateway.Middleware;

public class ApiGatewayMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IApiMappingService _mappingService;
    private readonly ILogger<ApiGatewayMiddleware> _logger;

    public ApiGatewayMiddleware(
        RequestDelegate next,
        IApiMappingService mappingService,
        ILogger<ApiGatewayMiddleware> logger)
    {
        _next = next;
        _mappingService = mappingService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            var endpoint = context.Request.Path.Value ?? string.Empty;
            var method = context.Request.Method;

            // Get the API mapping for this endpoint
            var mapping = await _mappingService.GetMappingForEndpoint(endpoint, method);
            if (mapping == null)
            {
                // No mapping found, pass through to next middleware
                await _next(context);
                return;
            }

            // Transform the request
            var transformedRequest = await _mappingService.TransformRequest(mapping, context.Request);

            // Forward the request
            var response = await _mappingService.ForwardRequest(mapping, transformedRequest);

            // Transform the response
            var transformedResponse = await _mappingService.TransformResponse(mapping, response.Content);

            // Write the response
            context.Response.StatusCode = (int)response.StatusCode;
            await context.Response.WriteAsJsonAsync(transformedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing request");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = "An error occurred processing your request" });
        }
    }
}

public static class ApiGatewayMiddlewareExtensions
{
    public static IApplicationBuilder UseApiGatewayMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ApiGatewayMiddleware>();
    }
}
