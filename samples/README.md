# API Gateway Middleware Samples

This directory contains sample applications demonstrating various features and use cases of the API Gateway Middleware.

## Weather Service Sample

A sample weather service that demonstrates:
- JSON and XML endpoint mapping
- Format conversion (XML to JSON)
- Response caching
- Rate limiting

### Project Structure
```
WeatherService/
├── WeatherController.cs    # Sample controller with JSON/XML endpoints
├── Program.cs             # Application setup and middleware configuration
├── appsettings.json      # API Gateway configuration
└── WeatherService.csproj # Project file with dependencies
```

### Configuration
The sample includes two API mappings:
1. JSON endpoint (`/api/weather`)
   - Returns weather forecast in JSON format
   - Demonstrates basic JSON-to-JSON mapping

2. XML endpoint (`/api/weather/xml`)
   - Returns weather forecast in XML format
   - Demonstrates XML-to-JSON conversion
   - Shows format transformation capabilities

### Running the Sample
1. Navigate to the WeatherService directory:
   ```bash
   cd samples/WeatherService
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Run the application:
   ```bash
   dotnet run
   ```

4. Test the endpoints:
   - JSON endpoint: `http://localhost:5001/weather`
   - XML endpoint: `http://localhost:5001/weather/xml`
   - Through API Gateway: 
     - `http://localhost:5000/api/weather`
     - `http://localhost:5000/api/weather/xml`

### Features Demonstrated

1. **Protocol Translation**
   - HTTP method mapping
   - Content type conversion
   - Endpoint routing

2. **Rate Limiting**
   - 60 requests per minute
   - Burst size of 10
   - API key-based client identification

3. **Caching**
   - 5-minute cache duration
   - Automatic cache invalidation
   - Cache key generation

4. **Error Handling**
   - Consistent error format
   - Development error details
   - Production error sanitization

### Adding More Samples

To add new samples:
1. Create a new directory under `samples/`
2. Add a project referencing the API Gateway
3. Configure API mappings in `appsettings.json`
4. Document the sample in this README

## Support

For questions about the samples:
1. Check the [documentation](../docs/)
2. Open an issue
3. Contact support@example.com
