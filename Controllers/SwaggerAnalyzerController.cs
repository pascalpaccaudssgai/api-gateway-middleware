using Microsoft.AspNetCore.Mvc;
using ApiGateway.Services;
using System.Text.Json;

namespace ApiGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SwaggerAnalyzerController : ControllerBase
    {
        private readonly SwaggerAnalyzerService _analyzerService;
        private readonly ILogger<SwaggerAnalyzerController> _logger;

        public SwaggerAnalyzerController(
            SwaggerAnalyzerService analyzerService,
            ILogger<SwaggerAnalyzerController> logger)
        {
            _analyzerService = analyzerService;
            _logger = logger;
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeSwagger([FromForm] IFormFile sourceSwagger, [FromForm] IFormFile targetSwagger)
        {
            try
            {
                // Save uploaded files temporarily
                var sourcePath = Path.GetTempFileName();
                var targetPath = Path.GetTempFileName();

                using (var stream = new FileStream(sourcePath, FileMode.Create))
                {
                    await sourceSwagger.CopyToAsync(stream);
                }

                using (var stream = new FileStream(targetPath, FileMode.Create))
                {
                    await targetSwagger.CopyToAsync(stream);
                }

                // Analyze the Swagger files
                var result = await _analyzerService.AnalyzeAndGenerateMapping(sourcePath, targetPath);

                // Clean up temporary files
                System.IO.File.Delete(sourcePath);
                System.IO.File.Delete(targetPath);

                // Generate configuration
                var config = new
                {
                    ApiGateway = new
                    {
                        SwaggerConfigurations = new
                        {
                            BaseApi = new
                            {
                                SwaggerPath = "./swagger/api-a.json",
                                Description = "Base API Configuration"
                            },
                            PartnerApi = new
                            {
                                SwaggerPath = "./swagger/api-b.json",
                                Mappings = result.EndpointMappings
                                    .Where(m => m.Confidence > 0.7) // Only include high-confidence mappings
                                    .Select(m => new
                                    {
                                        m.SourceEndpoint,
                                        m.TargetEndpoint,
                                        m.HttpMethod,
                                        m.TargetHttpMethod,
                                        DataTransformations = result.DataTransformations
                                            .Where(dt => dt.Confidence > 0.7)
                                            .Select(dt => new
                                            {
                                                dt.SourceType,
                                                dt.TargetType,
                                                dt.PropertyMappings
                                            })
                                    })
                            }
                        }
                    }
                };

                return Ok(new
                {
                    Configuration = config,
                    Analysis = new
                    {
                        EndpointMappings = result.EndpointMappings
                            .OrderByDescending(m => m.Confidence)
                            .Select(m => new
                            {
                                m.SourceEndpoint,
                                m.TargetEndpoint,
                                m.HttpMethod,
                                m.TargetHttpMethod,
                                m.Confidence,
                                Status = m.Confidence switch
                                {
                                    > 0.9 => "Excellent Match",
                                    > 0.7 => "Good Match",
                                    > 0.5 => "Possible Match",
                                    _ => "Low Confidence Match"
                                }
                            }),
                        DataTransformations = result.DataTransformations
                            .OrderByDescending(dt => dt.Confidence)
                            .Select(dt => new
                            {
                                dt.SourceType,
                                dt.TargetType,
                                dt.PropertyMappings,
                                dt.Confidence,
                                Status = dt.Confidence switch
                                {
                                    > 0.9 => "Excellent Match",
                                    > 0.7 => "Good Match",
                                    > 0.5 => "Possible Match",
                                    _ => "Low Confidence Match"
                                }
                            })
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing Swagger files");
                return StatusCode(500, new { error = "Error analyzing Swagger files", details = ex.Message });
            }
        }
    }
}
