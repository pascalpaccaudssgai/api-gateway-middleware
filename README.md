# API Gateway Middleware

A flexible, scalable API gateway middleware solution built with .NET 7 for handling complex API integrations and transformations across different protocols and data formats.

## Features

- 🔄 **Protocol Translation**
  - HTTP method mapping
  - JSON/XML format conversion
  - Flexible endpoint mapping

- ⚡ **Rate Limiting**
  - Configurable request limits
  - Burst handling
  - IP/API key-based limiting

- 📦 **Response Caching**
  - In-memory caching
  - Configurable duration
  - Smart cache invalidation

- 🛡️ **Error Handling**
  - Consistent error format
  - Development/Production modes
  - Detailed logging

## Quick Start

1. **Prerequisites**
   - .NET 7 SDK
   - Visual Studio 2022 or VS Code

2. **Installation**
   ```bash
   git clone <repository-url>
   cd api-gateway-middleware
   dotnet restore
   ```

3. **Configuration**
   ```json
   {
     "ApiGateway": {
       "Mappings": [
         {
           "SourceEndpoint": "/api/users",
           "TargetEndpoint": "https://api.example.com/users",
           "HttpMethod": "GET",
           "TargetHttpMethod": "GET",
           "SourceFormat": "Json",
           "TargetFormat": "Json"
         }
       ]
     }
   }
   ```

4. **Run**
   ```bash
   dotnet run
   ```

## Documentation

- [Functional Specification](docs/FunctionalSpecification.md)
- [Onboarding Guide](docs/Onboarding.md)
- [API Documentation](docs/API.md)
- [Sample Code](samples/)

## Architecture

```
ApiGateway/
├── Middleware/        # Custom middleware components
├── Services/          # Core services
├── Models/            # Data models
└── Configuration/     # Configuration classes
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

For support and questions, please:
1. Check the [documentation](docs/)
2. Try the [samples](samples/)
3. Open an issue
4. Contact support@example.com
