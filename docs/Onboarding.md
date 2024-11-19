# API Gateway Middleware - Onboarding Guide

## Getting Started

### Prerequisites
- .NET 7 SDK or later
- Visual Studio 2022 or VS Code
- Git
- Docker (optional)

### Installation Steps

1. **Clone the Repository**
```bash
git clone <repository-url>
cd api-gateway-middleware
```

2. **Restore Dependencies**
```bash
dotnet restore
```

3. **Build the Project**
```bash
dotnet build
```

4. **Run the Application**
```bash
dotnet run
```

## Configuration Guide

### Basic Configuration
1. Open `appsettings.json`
2. Configure the API mappings:
```json
{
  "ApiGateway": {
    "Mappings": [
      {
        "SourceEndpoint": "/api/users",
        "TargetEndpoint": "https://api.example.com/users",
        "HttpMethod": "GET",
        "TargetHttpMethod": "GET",
        "SourceFormat": "Json",
        "TargetFormat": "Json"
      }
    ]
  }
}
```

### Rate Limiting Configuration
```json
{
  "RateLimit": {
    "EnableRateLimiting": true,
    "RequestsPerMinute": 60,
    "BurstSize": 10
  }
}
```

### Caching Configuration
```json
{
  "Caching": {
    "DefaultDuration": "00:05:00",
    "MaxCacheSize": "1024"
  }
}
```

## Development Guide

### Project Structure
```
ApiGateway/
├── Controllers/
├── Middleware/
│   ├── RateLimitingMiddleware.cs
│   ├── CachingMiddleware.cs
│   └── ErrorHandlingMiddleware.cs
├── Services/
│   ├── ApiMappingService.cs
│   ├── DataTransformationService.cs
│   └── InMemoryCacheService.cs
├── Models/
│   ├── ApiMapping.cs
│   └── ApiError.cs
└── Program.cs
```

### Adding New Features

1. **Create New Middleware**
```csharp
public class CustomMiddleware
{
    private readonly RequestDelegate _next;

    public CustomMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add your middleware logic here
        await _next(context);
    }
}
```

2. **Register Middleware**
```csharp
app.UseMiddleware<CustomMiddleware>();
```

### Running Tests
```bash
dotnet test
```

## Deployment Guide

### Docker Deployment
1. Build the Docker image:
```bash
docker build -t api-gateway .
```

2. Run the container:
```bash
docker run -p 5000:80 api-gateway
```

### Production Configuration
1. Set environment variables:
```bash
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://+:5000
```

2. Update production settings in `appsettings.Production.json`

## Troubleshooting

### Common Issues

1. **Rate Limiting Issues**
- Check rate limit configuration
- Verify client identification method
- Review rate limit logs

2. **Caching Issues**
- Verify cache duration settings
- Check cache key generation
- Monitor cache hit/miss ratio

3. **Transformation Errors**
- Validate input/output formats
- Check transformation mappings
- Review error logs

### Logging

1. **Enable Debug Logging**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information"
    }
  }
}
```

2. **View Logs**
```bash
tail -f logs/api-gateway.log
```

## Support and Resources

- Documentation: `/docs`
- Sample Code: `/samples`
- Issue Tracker: GitHub Issues
- Contact: support@example.com

## Best Practices

1. **API Mapping**
- Use clear endpoint naming
- Document all mappings
- Test transformations thoroughly

2. **Rate Limiting**
- Set appropriate limits
- Monitor usage patterns
- Implement graceful degradation

3. **Caching**
- Cache only necessary data
- Set appropriate TTL
- Implement cache invalidation

4. **Error Handling**
- Use consistent error formats
- Log all errors
- Provide helpful error messages
