using ApiGateway.Models;

namespace ApiGateway.Services;

public interface IApiMappingService
{
    Task<ApiMapping?> GetMappingForEndpoint(string endpoint, string method);
    Task<object> TransformRequest(ApiMapping mapping, HttpRequest request);
    Task<object> TransformResponse(ApiMapping mapping, object response);
    Task<HttpResponseMessage> ForwardRequest(ApiMapping mapping, object transformedRequest);
}
