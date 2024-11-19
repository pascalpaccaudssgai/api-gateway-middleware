namespace ApiGateway.Models;

public class RateLimitConfig
{
    public int RequestsPerMinute { get; set; } = 60;
    public int BurstSize { get; set; } = 10;
    public bool EnableRateLimiting { get; set; } = true;
}

public class RateLimitEntry
{
    public string ClientId { get; set; } = string.Empty;
    public DateTimeOffset LastRequest { get; set; }
    public int RequestCount { get; set; }
    public Queue<DateTimeOffset> RequestHistory { get; set; } = new();
}
