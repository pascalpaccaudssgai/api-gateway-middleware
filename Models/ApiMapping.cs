namespace ApiGateway.Models;

public class ApiMapping
{
    public string SourceEndpoint { get; set; } = string.Empty;
    public string TargetEndpoint { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = "GET";
    public string TargetHttpMethod { get; set; } = "GET";
    public Dictionary<string, string> HeaderMappings { get; set; } = new();
    public Dictionary<string, string> QueryParameterMappings { get; set; } = new();
    public Dictionary<string, string> BodyFieldMappings { get; set; } = new();
    public List<string> RequiredFields { get; set; } = new();
    public DataFormat SourceFormat { get; set; } = DataFormat.Json;
    public DataFormat TargetFormat { get; set; } = DataFormat.Json;
}

public enum DataFormat
{
    Json,
    Xml,
    FormData
}
