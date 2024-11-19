using System.Net;
using ApiGateway.Models;
using Newtonsoft.Json;

namespace ApiGateway.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception occurred");

        var code = HttpStatusCode.InternalServerError;
        var errorCode = "InternalServerError";

        switch (exception)
        {
            case ArgumentException:
                code = HttpStatusCode.BadRequest;
                errorCode = "InvalidArgument";
                break;
            case InvalidOperationException:
                code = HttpStatusCode.BadRequest;
                errorCode = "InvalidOperation";
                break;
            case UnauthorizedAccessException:
                code = HttpStatusCode.Unauthorized;
                errorCode = "Unauthorized";
                break;
            case KeyNotFoundException:
                code = HttpStatusCode.NotFound;
                errorCode = "NotFound";
                break;
        }

        var response = new ApiErrorResponse
        {
            Error = new ApiError
            {
                Code = errorCode,
                Message = exception.Message,
                Details = _env.IsDevelopment() ? exception.StackTrace : null
            },
            RequestId = context.TraceIdentifier
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        return context.Response.WriteAsync(JsonConvert.SerializeObject(response));
    }
}

public static class ErrorHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ErrorHandlingMiddleware>();
    }
}
