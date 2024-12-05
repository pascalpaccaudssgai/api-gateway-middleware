using System.Text.Json;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Any;
using System.Text.RegularExpressions;

namespace ApiGateway.Services
{
    public class SwaggerAnalyzerService
    {
        private readonly ILogger<SwaggerAnalyzerService> _logger;
        private readonly IExternalToolService? _externalToolService;
        private readonly IConfiguration _configuration;

        public SwaggerAnalyzerService(ILogger<SwaggerAnalyzerService> logger, IExternalToolService? externalToolService = null, IConfiguration configuration = null)
        {
            _logger = logger;
            _externalToolService = externalToolService;
            _configuration = configuration;
        }

        public async Task<ApiMappingResult> AnalyzeAndGenerateMapping(string sourceSwaggerPath, string targetSwaggerPath)
        {
            var sourceDoc = await LoadSwaggerDocument(sourceSwaggerPath);
            var targetDoc = await LoadSwaggerDocument(targetSwaggerPath);

            var result = new ApiMappingResult
            {
                EndpointMappings = await GenerateEndpointMappings(sourceDoc, targetDoc),
                DataTransformations = await GenerateDataTransformations(sourceDoc, targetDoc)
            };

            // Check if external tool is enabled
            if (_externalToolService != null && _configuration.GetValue<bool>("ApiGateway:ExternalTool:Enabled"))
            {
                var externalResult = await _externalToolService.AnalyzeAndGenerateMappingAsync(sourceSwaggerPath, targetSwaggerPath);
                if (externalResult != null)
                {
                    result = externalResult;
                }
            }

            return result;
        }

        private async Task<OpenApiDocument> LoadSwaggerDocument(string path)
        {
            using var stream = File.OpenRead(path);
            var reader = new OpenApiStreamReader();
            return (await reader.ReadAsync(stream)).OpenApiDocument;
        }

        private async Task<List<EndpointMapping>> GenerateEndpointMappings(OpenApiDocument sourceDoc, OpenApiDocument targetDoc)
        {
            var mappings = new List<EndpointMapping>();

            foreach (var sourcePath in sourceDoc.Paths)
            {
                var bestMatch = FindBestMatchingEndpoint(sourcePath, targetDoc.Paths);
                if (bestMatch.HasValue)
                {
                    foreach (var sourceOp in sourcePath.Value.Operations)
                    {
                        var targetOps = bestMatch.Value.Value.Operations;
                        var targetOp = FindMatchingOperation(sourceOp.Value, targetOps);
                        if (targetOp != null)
                        {
                            mappings.Add(new EndpointMapping
                            {
                                SourceEndpoint = sourcePath.Key,
                                TargetEndpoint = bestMatch.Value.Key,
                                HttpMethod = sourceOp.Key.ToString(),
                                TargetHttpMethod = targetOp.Value.Key.ToString(),
                                Confidence = CalculateConfidence(sourceOp.Value, targetOp.Value.Value)
                            });
                        }
                    }
                }
            }

            return mappings;
        }

        private KeyValuePair<OperationType, OpenApiOperation>? FindMatchingOperation(
            OpenApiOperation sourceOp,
            IDictionary<OperationType, OpenApiOperation> targetOps)
        {
            // First try to find exact method match
            if (targetOps.Count > 0)
            {
                return targetOps.FirstOrDefault();
            }

            return null;
        }

        private async Task<List<DataTransformation>> GenerateDataTransformations(OpenApiDocument sourceDoc, OpenApiDocument targetDoc)
        {
            var transformations = new List<DataTransformation>();

            foreach (var sourceSchema in sourceDoc.Components.Schemas)
            {
                var bestMatch = FindBestMatchingSchema(sourceSchema.Value, targetDoc.Components.Schemas);
                if (bestMatch.Key != null)
                {
                    transformations.Add(new DataTransformation
                    {
                        SourceType = sourceSchema.Key,
                        TargetType = bestMatch.Key,
                        PropertyMappings = GeneratePropertyMappings(sourceSchema.Value, bestMatch.Value),
                        Confidence = CalculateSchemaConfidence(sourceSchema.Value, bestMatch.Value)
                    });
                }
            }

            return transformations;
        }

        private Dictionary<string, string> GeneratePropertyMappings(OpenApiSchema sourceSchema, OpenApiSchema targetSchema)
        {
            var mappings = new Dictionary<string, string>();
            
            foreach (var sourceProp in sourceSchema.Properties)
            {
                var bestMatch = FindBestMatchingProperty(sourceProp, targetSchema.Properties);
                if (bestMatch.Key != null)
                {
                    mappings[sourceProp.Key] = bestMatch.Key;
                }
            }

            return mappings;
        }

        private KeyValuePair<string, OpenApiSchema> FindBestMatchingSchema(
            OpenApiSchema sourceSchema,
            IDictionary<string, OpenApiSchema> targetSchemas)
        {
            return targetSchemas
                .OrderByDescending(schema => CalculateSchemaConfidence(sourceSchema, schema.Value))
                .FirstOrDefault();
        }

        private double CalculateSchemaConfidence(OpenApiSchema source, OpenApiSchema target)
        {
            if (source == null || target == null) return 0;

            var propertyMatches = source.Properties
                .Count(sp => target.Properties
                    .Any(tp => IsPropertySimilar(sp.Key, tp.Key, sp.Value, tp.Value)));

            return (double)propertyMatches / Math.Max(source.Properties.Count, target.Properties.Count);
        }

        private bool IsPropertySimilar(string sourceName, string targetName, OpenApiSchema sourceSchema, OpenApiSchema targetSchema)
        {
            // Normalize names for comparison
            var normalizedSource = NormalizeName(sourceName);
            var normalizedTarget = NormalizeName(targetName);

            // Check name similarity
            var nameSimilarity = CalculateStringSimilarity(normalizedSource, normalizedTarget);
            
            // Check type compatibility
            var typeMatch = sourceSchema.Type == targetSchema.Type;

            // Check format compatibility if specified
            var formatMatch = string.IsNullOrEmpty(sourceSchema.Format) || 
                            string.IsNullOrEmpty(targetSchema.Format) ||
                            sourceSchema.Format == targetSchema.Format;

            return nameSimilarity > 0.7 && typeMatch && formatMatch;
        }

        private string NormalizeName(string name)
        {
            // Convert camelCase or PascalCase to lowercase with spaces
            var words = Regex.Replace(name, "([a-z])([A-Z])", "$1 $2").ToLower();
            // Remove common prefixes/suffixes
            words = Regex.Replace(words, "^(get|set|post|put|delete|patch|request|response|dto)\\s", "");
            return words.Trim();
        }

        private double CalculateStringSimilarity(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return 0;
            
            var distance = LevenshteinDistance(s1, s2);
            var maxLength = Math.Max(s1.Length, s2.Length);
            
            return 1 - ((double)distance / maxLength);
        }

        private int LevenshteinDistance(string s1, string s2)
        {
            var matrix = new int[s1.Length + 1, s2.Length + 1];

            for (var i = 0; i <= s1.Length; i++)
                matrix[i, 0] = i;
            for (var j = 0; j <= s2.Length; j++)
                matrix[0, j] = j;

            for (var i = 1; i <= s1.Length; i++)
            {
                for (var j = 1; j <= s2.Length; j++)
                {
                    var cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }

            return matrix[s1.Length, s2.Length];
        }

        private KeyValuePair<string, OpenApiSchema> FindBestMatchingProperty(
            KeyValuePair<string, OpenApiSchema> sourceProp,
            IDictionary<string, OpenApiSchema> targetProps)
        {
            return targetProps
                .OrderByDescending(tp => IsPropertySimilar(sourceProp.Key, tp.Key, sourceProp.Value, tp.Value))
                .FirstOrDefault();
        }

        private double CalculateConfidence(OpenApiOperation source, OpenApiOperation target)
        {
            if (source == null || target == null) return 0;

            var descriptionSimilarity = CalculateStringSimilarity(
                source.Description ?? "",
                target.Description ?? "");

            var parameterMatch = source.Parameters
                .Count(sp => target.Parameters
                    .Any(tp => IsParameterSimilar(sp, tp)));

            var parameterScore = source.Parameters.Count > 0
                ? (double)parameterMatch / Math.Max(source.Parameters.Count, target.Parameters.Count)
                : 1;

            return (descriptionSimilarity + parameterScore) / 2;
        }

        private bool IsParameterSimilar(OpenApiParameter source, OpenApiParameter target)
        {
            var nameSimilarity = CalculateStringSimilarity(
                NormalizeName(source.Name),
                NormalizeName(target.Name));

            return nameSimilarity > 0.7 &&
                   source.In == target.In &&
                   source.Schema?.Type == target.Schema?.Type;
        }

        private KeyValuePair<string, OpenApiPathItem>? FindBestMatchingEndpoint(
            KeyValuePair<string, OpenApiPathItem> sourcePath,
            OpenApiPaths targetPaths)
        {
            return targetPaths
                .OrderByDescending(tp => CalculatePathSimilarity(sourcePath.Key, tp.Key))
                .FirstOrDefault(tp => tp.Key != null);
        }

        private double CalculatePathSimilarity(string sourcePath, string targetPath)
        {
            // Normalize paths by removing version numbers and common prefixes
            var normalizedSource = NormalizePath(sourcePath);
            var normalizedTarget = NormalizePath(targetPath);

            return CalculateStringSimilarity(normalizedSource, normalizedTarget);
        }

        private string NormalizePath(string path)
        {
            // Remove version numbers
            path = Regex.Replace(path, @"v\d+/?", "");
            // Remove common API prefixes
            path = Regex.Replace(path, @"^/api/", "");
            // Remove path parameters
            path = Regex.Replace(path, @"\{[^}]+\}", "");
            return path.Trim('/');
        }

        private double CalculateOperationSimilarity(OpenApiOperation source, OpenApiOperation target)
        {
            if (source == null || target == null) return 0;

            var descriptionMatch = CalculateStringSimilarity(
                source.Description ?? "",
                target.Description ?? "");

            var responseMatch = source.Responses.Keys
                .Count(sk => target.Responses.ContainsKey(sk));

            var responseScore = source.Responses.Count > 0
                ? (double)responseMatch / Math.Max(source.Responses.Count, target.Responses.Count)
                : 1;

            return (descriptionMatch + responseScore) / 2;
        }
    }

    public class ApiMappingResult
    {
        public List<EndpointMapping> EndpointMappings { get; set; } = new();
        public List<DataTransformation> DataTransformations { get; set; } = new();
    }

    public class EndpointMapping
    {
        public string SourceEndpoint { get; set; }
        public string TargetEndpoint { get; set; }
        public string HttpMethod { get; set; }
        public string TargetHttpMethod { get; set; }
        public double Confidence { get; set; }
    }

    public class DataTransformation
    {
        public string SourceType { get; set; }
        public string TargetType { get; set; }
        public Dictionary<string, string> PropertyMappings { get; set; }
        public double Confidence { get; set; }
    }
}
