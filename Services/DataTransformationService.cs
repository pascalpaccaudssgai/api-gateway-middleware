using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;
using System.Xml.Linq;

namespace ApiGateway.Services
{
    public class DataTransformationService : IDataTransformationService
    {
        public async Task<string> TransformRequestAsync(string content, string sourceFormat, string targetFormat)
        {
            return await TransformContentAsync(content, sourceFormat, targetFormat);
        }

        public async Task<string> TransformResponseAsync(string content, string sourceFormat, string targetFormat)
        {
            return await TransformContentAsync(content, sourceFormat, targetFormat);
        }

        private async Task<string> TransformContentAsync(string content, string sourceFormat, string targetFormat)
        {
            if (string.IsNullOrEmpty(content)) return content;
            if (sourceFormat.Equals(targetFormat, StringComparison.OrdinalIgnoreCase)) return content;

            // Convert source to intermediate object
            object? intermediateObject = sourceFormat.ToLowerInvariant() switch
            {
                "json" => JsonConvert.DeserializeObject(content),
                "xml" => XDocument.Parse(content),
                _ => throw new NotSupportedException($"Source format {sourceFormat} is not supported")
            };

            if (intermediateObject == null)
                return content;

            // Convert intermediate object to target format
            return targetFormat.ToLowerInvariant() switch
            {
                "json" => JsonConvert.SerializeObject(intermediateObject),
                "xml" => intermediateObject switch
                {
                    XDocument xdoc => xdoc.ToString(),
                    _ => JsonConvert.DeserializeXNode(
                            JsonConvert.SerializeObject(intermediateObject), "root")?.ToString() ?? string.Empty
                },
                _ => throw new NotSupportedException($"Target format {targetFormat} is not supported")
            };
        }
    }
}
