using System.Text.Json;
using System.Xml.Linq;
using ApiGateway.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace ApiGateway.Services;

public class ApiMappingService : IApiMappingService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiMappingService> _logger;
    private readonly HttpClient _httpClient;

    public ApiMappingService(
        IConfiguration configuration,
        ILogger<ApiMappingService> logger,
        HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<ApiMapping?> GetMappingForEndpoint(string endpoint, string method)
    {
        // In a real implementation, this would load from configuration or database
        // For now, we'll return a sample mapping
        return new ApiMapping
        {
            SourceEndpoint = endpoint,
            TargetEndpoint = "https://api.example.com" + endpoint,
            HttpMethod = method,
            TargetHttpMethod = method,
            SourceFormat = ApiGateway.Models.DataFormat.Json,
            TargetFormat = ApiGateway.Models.DataFormat.Json
        };
    }

    public async Task<object> TransformRequest(ApiMapping mapping, HttpRequest request)
    {
        try
        {
            // Read the request body
            string requestBody;
            using (var reader = new StreamReader(request.Body))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            // Transform the data based on the source and target formats
            var sourceData = mapping.SourceFormat switch
            {
                ApiGateway.Models.DataFormat.Json => JsonConvert.DeserializeObject<Dictionary<string, object>>(requestBody),
                ApiGateway.Models.DataFormat.Xml => ParseXmlToObject(requestBody),
                _ => throw new NotSupportedException($"Format {mapping.SourceFormat} is not supported")
            };

            // Apply field mappings
            var transformedData = ApplyFieldMappings(sourceData, mapping.BodyFieldMappings);

            // Convert to target format
            return mapping.TargetFormat switch
            {
                ApiGateway.Models.DataFormat.Json => JsonConvert.SerializeObject(transformedData),
                ApiGateway.Models.DataFormat.Xml => ConvertToXml(transformedData),
                _ => throw new NotSupportedException($"Format {mapping.TargetFormat} is not supported")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transforming request");
            throw;
        }
    }

    public async Task<object> TransformResponse(ApiMapping mapping, object response)
    {
        // Similar to TransformRequest but in reverse
        // Implementation would mirror the request transformation
        return response;
    }

    public async Task<HttpResponseMessage> ForwardRequest(ApiMapping mapping, object transformedRequest)
    {
        try
        {
            var client = new RestClient();
            var request = new RestRequest(mapping.TargetEndpoint, GetRestSharpMethod(mapping.TargetHttpMethod));

            // Add the transformed body
            if (transformedRequest != null)
            {
                request.AddBody(transformedRequest);
            }

            var response = await client.ExecuteAsync(request);
            
            if (response.IsSuccessful)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(response.Content ?? string.Empty)
                };
            }

            return new HttpResponseMessage(response.StatusCode)
            {
                Content = new StringContent(response.ErrorMessage ?? "Unknown error")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error forwarding request");
            throw;
        }
    }

    private Method GetRestSharpMethod(string method) => method.ToUpperInvariant() switch
    {
        "GET" => Method.Get,
        "POST" => Method.Post,
        "PUT" => Method.Put,
        "DELETE" => Method.Delete,
        "PATCH" => Method.Patch,
        _ => throw new NotSupportedException($"HTTP method {method} is not supported")
    };

    private Dictionary<string, object>? ParseXmlToObject(string xml)
    {
        var doc = XDocument.Parse(xml);
        var json = JsonConvert.SerializeXNode(doc);
        return JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
    }

    private string ConvertToXml(object obj)
    {
        var json = JsonConvert.SerializeObject(obj);
        var xmlDoc = JsonConvert.DeserializeXNode(json, "root");
        return xmlDoc?.ToString() ?? string.Empty;
    }

    private Dictionary<string, object>? ApplyFieldMappings(Dictionary<string, object>? sourceData, Dictionary<string, string>? mappings)
    {
        if (sourceData == null || mappings == null || !mappings.Any())
            return sourceData;

        var result = new Dictionary<string, object>();
        foreach (var mapping in mappings)
        {
            if (sourceData.TryGetValue(mapping.Key, out var value))
            {
                result[mapping.Value] = value;
            }
        }
        return result;
    }
}
