using ApiGateway.Models;

namespace ApiGateway.Services;

public interface IApiConfigurationService
{
    Task<IEnumerable<ApiMapping>> GetAllMappings();
    Task<ApiMapping?> GetMappingForEndpoint(string endpoint, string method);
    Task<bool> AddMapping(ApiMapping mapping);
    Task<bool> UpdateMapping(ApiMapping mapping);
    Task<bool> DeleteMapping(string endpoint, string method);
}
