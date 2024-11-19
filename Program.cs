using ApiGateway.Services;
using ApiGateway.Middleware;
using ApiGateway.Models;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<SwaggerAnalyzerService>();

// Configure rate limiting
builder.Services.Configure<RateLimitConfig>(builder.Configuration.GetSection("RateLimit"));

// Add caching
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, InMemoryCacheService>();

// Register services
builder.Services.AddSingleton<IApiMappingService, ApiMappingService>();
builder.Services.AddSingleton<IDataTransformationService, DataTransformationService>();
builder.Services.AddSingleton<IApiConfigurationService, ApiConfigurationService>();
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Add custom middleware in the correct order
app.UseErrorHandling(); // First to catch all errors
app.UseRateLimiting(); // Then rate limiting
app.UseCaching(); // Then caching
app.UseApiGatewayMiddleware(); // Finally our API gateway

app.MapControllers();

app.Run();
