using System.Net;
using System.Text.Json;
using ApiGateway.Models;
using ApiGateway.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ApiGateway.Tests.Unit
{
    public class ApiMappingServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ILogger<ApiMappingService>> _mockLogger;
        private readonly Mock<IExternalToolService> _mockExternalToolService;
        private readonly Mock<HttpClient> _mockHttpClient;
        private readonly ApiMappingService _service;

        public ApiMappingServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<ApiMappingService>>();
            _mockExternalToolService = new Mock<IExternalToolService>();
            _mockHttpClient = new Mock<HttpClient>();
            _service = new ApiMappingService(
                _mockConfiguration.Object,
                _mockLogger.Object,
                _mockHttpClient.Object,
                _mockExternalToolService.Object);
        }

        [Fact]
        public async Task GetMappingForEndpoint_ReturnsMapping()
        {
            // Arrange
            var endpoint = "/test";
            var method = "GET";

            // Act
            var result = await _service.GetMappingForEndpoint(endpoint, method);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(endpoint, result.SourceEndpoint);
            Assert.Equal(method, result.HttpMethod);
        }

        [Fact]
        public async Task TransformRequest_TransformsJsonRequest()
        {
            // Arrange
            var mapping = new ApiMapping
            {
                SourceFormat = DataFormat.Json,
                TargetFormat = DataFormat.Json,
                BodyFieldMappings = new Dictionary<string, string>
                {
                    { "sourceField", "targetField" }
                }
            };
            var request = new Mock<HttpRequest>();
            var requestBody = JsonSerializer.Serialize(new { sourceField = "value" });
            request.Setup(r => r.Body).Returns(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(requestBody)));

            // Act
            var result = await _service.TransformRequest(mapping, request.Object);

            // Assert
            var transformedRequest = JsonSerializer.Deserialize<Dictionary<string, object>>(result.ToString());
            Assert.NotNull(transformedRequest);
            Assert.Equal("value", transformedRequest["targetField"]);
        }

        [Fact]
        public async Task TransformResponse_TransformsJsonResponse()
        {
            // Arrange
            var mapping = new ApiMapping
            {
                SourceFormat = DataFormat.Json,
                TargetFormat = DataFormat.Json,
                BodyFieldMappings = new Dictionary<string, string>
                {
                    { "sourceField", "targetField" }
                }
            };
            var response = JsonSerializer.Serialize(new { sourceField = "value" });

            // Act
            var result = await _service.TransformResponse(mapping, response);

            // Assert
            var transformedResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(result.ToString());
            Assert.NotNull(transformedResponse);
            Assert.Equal("value", transformedResponse["targetField"]);
        }

        [Fact]
        public async Task ForwardRequest_ForwardsRequest()
        {
            // Arrange
            var mapping = new ApiMapping
            {
                TargetEndpoint = "https://api.example.com/test",
                TargetHttpMethod = "GET"
            };
            var transformedRequest = new { };

            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("response content")
            };
            _mockHttpClient.Setup(c => c.SendAsync(It.IsAny<HttpRequestMessage>())).ReturnsAsync(mockResponse);

            // Act
            var result = await _service.ForwardRequest(mapping, transformedRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var content = await result.Content.ReadAsStringAsync();
            Assert.Equal("response content", content);
        }
    }
}
