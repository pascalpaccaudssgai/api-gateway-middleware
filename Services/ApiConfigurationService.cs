using ApiGateway.Models;
using System.Collections.Concurrent;

namespace ApiGateway.Services;

public class ApiConfigurationService : IApiConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiConfigurationService> _logger;
    private readonly ConcurrentDictionary<string, ApiMapping> _mappings;

    public ApiConfigurationService(
        IConfiguration configuration,
        ILogger<ApiConfigurationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _mappings = new ConcurrentDictionary<string, ApiMapping>();
        LoadMappingsFromConfiguration();
    }

    private void LoadMappingsFromConfiguration()
    {
        try
        {
            var mappings = _configuration.GetSection("ApiGateway:Mappings")
                .Get<List<ApiMapping>>() ?? new List<ApiMapping>();

            foreach (var mapping in mappings)
            {
                var key = GetMappingKey(mapping.SourceEndpoint, mapping.HttpMethod);
                _mappings.TryAdd(key, mapping);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading API mappings from configuration");
        }
    }

    public Task<IEnumerable<ApiMapping>> GetAllMappings()
    {
        return Task.FromResult(_mappings.Values.AsEnumerable());
    }

    public Task<ApiMapping?> GetMappingForEndpoint(string endpoint, string method)
    {
        var key = GetMappingKey(endpoint, method);
        _mappings.TryGetValue(key, out var mapping);
        return Task.FromResult(mapping);
    }

    public Task<bool> AddMapping(ApiMapping mapping)
    {
        var key = GetMappingKey(mapping.SourceEndpoint, mapping.HttpMethod);
        var success = _mappings.TryAdd(key, mapping);
        
        if (success)
        {
            _logger.LogInformation("Added new mapping for {Endpoint} {Method}", 
                mapping.SourceEndpoint, mapping.HttpMethod);
        }
        
        return Task.FromResult(success);
    }

    public Task<bool> UpdateMapping(ApiMapping mapping)
    {
        var key = GetMappingKey(mapping.SourceEndpoint, mapping.HttpMethod);
        var success = _mappings.TryUpdate(key, mapping, _mappings[key]);
        
        if (success)
        {
            _logger.LogInformation("Updated mapping for {Endpoint} {Method}", 
                mapping.SourceEndpoint, mapping.HttpMethod);
        }
        
        return Task.FromResult(success);
    }

    public Task<bool> DeleteMapping(string endpoint, string method)
    {
        var key = GetMappingKey(endpoint, method);
        var success = _mappings.TryRemove(key, out _);
        
        if (success)
        {
            _logger.LogInformation("Deleted mapping for {Endpoint} {Method}", endpoint, method);
        }
        
        return Task.FromResult(success);
    }

    private string GetMappingKey(string endpoint, string method)
    {
        return $"{method.ToUpperInvariant()}:{endpoint.ToLowerInvariant()}";
    }
}
