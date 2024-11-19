namespace ApiGateway.Models;

public class ApiError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public IDictionary<string, string[]>? ValidationErrors { get; set; }
}

public class ApiErrorResponse
{
    public ApiError Error { get; set; } = new();
    public string RequestId { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}
