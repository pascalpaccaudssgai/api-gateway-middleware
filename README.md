# API Gateway with Swagger Analysis

A powerful API Gateway solution that automatically analyzes, maps, and transforms between different API specifications. Built with .NET 7, it provides intelligent mapping and transformation capabilities starting with REST/Swagger APIs, with planned support for SOAP and GraphQL.

## Key Features

- **Intelligent API Analysis**
  - Automatic endpoint matching
  - Smart schema mapping
  - Confidence scoring
  - Transformation suggestions

- **Protocol Support**
  - REST/Swagger (Current)
  - SOAP (Planned)
  - GraphQL (Planned)
  - Custom protocols (Extensible)

- **Performance**
  - Fast analysis (<2s)
  - Efficient transformations
  - Response caching
  - Rate limiting

- **Security**
  - Input validation
  - Rate limiting
  - Authentication support
  - Error handling

## Quick Start

1. **Prerequisites**
   - .NET 7 SDK
   - Visual Studio 2022 or VS Code
   - Source and target Swagger/OAS files

2. **Installation**
   ```bash
   git clone <repository-url>
   cd api-gateway
   dotnet restore
   ```

3. **Run the Gateway**
   ```bash
   dotnet run
   ```

4. **Analyze APIs**
   ```http
   POST /api/swaggeranalyzer/analyze
   Content-Type: multipart/form-data

   sourceSwagger: [Your API Swagger File]
   targetSwagger: [Target API Swagger File]
   ```

## Documentation

- [Onboarding Guide](docs/onboarding.md)
- [Functional Specifications](docs/functional_spec.md)
- [Basic Process](docs/basic_process.md)

## Configuration

```json
{
  "ApiGateway": {
    "SwaggerConfigurations": {
      "BaseApi": {
        "SwaggerPath": "./swagger/api-a.json",
        "Description": "Base API Configuration"
      },
      "PartnerApi": {
        "SwaggerPath": "./swagger/api-b.json",
        "Mappings": [...]
      }
    }
  }
}
```

## Use Cases

1. **API Integration**
   - Map between different API versions
   - Transform between different protocols
   - Normalize API responses

2. **API Modernization**
   - Convert legacy APIs to modern standards
   - Transform between different data formats
   - Upgrade protocol versions

3. **API Federation**
   - Combine multiple APIs
   - Standardize interfaces
   - Unify authentication

## Contributing

1. Fork the repository
2. Create your feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

- Create an issue
- Check documentation
- Contact support team
- Join discussions

## Roadmap

1. **Phase 1: REST/Swagger (Current)**
   - Endpoint matching
   - Data transformation
   - Configuration generation

2. **Phase 2: SOAP Integration**
   - WSDL support
   - XML transformation
   - SOAP action handling

3. **Phase 3: GraphQL Support**
   - Schema analysis
   - Query transformation
   - Subscription handling

4. **Future Enhancements**
   - Machine learning matching
   - Visual mapping interface
   - Custom protocol support
   - Advanced analytics
