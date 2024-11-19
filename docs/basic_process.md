# API Gateway Integration Process with Multiple Swagger/OAS Files

## Overview
This document explains how to use the API Gateway with multiple Swagger/OpenAPI Specification (OAS) files, specifically when you have one constant API (A) and multiple partner APIs (B).

## Architecture
```
┌─────────────┐     ┌──────────────┐     ┌────────────────┐
│ Your API (A)│ ←── │ API Gateway  │ ──→ │Partner APIs (B)│
└─────────────┘     └──────────────┘     └────────────────┘
                          ↓
                    Transformations
                    Rate Limiting
                    Caching
```

## Basic Setup Process

### 1. Swagger File Organization
```
/swagger/
  ├── api-a.json         # Your constant API specification
  ├── api-b1.json        # Partner API 1 specification
  ├── api-b2.json        # Partner API 2 specification
  └── api-b3.json        # Partner API 3 specification
```

### 2. Configuration Structure
```json
{
  "ApiGateway": {
    "SwaggerConfigurations": {
      "BaseApi": {
        "SwaggerPath": "./swagger/api-a.json",
        "Description": "Base API Configuration"
      },
      "PartnerApis": [
        {
          "Name": "PartnerB1",
          "SwaggerPath": "./swagger/api-b1.json",
          "Mappings": [...]
        }
      ]
    }
  }
}
```

## Integration Process

### 1. Initial Setup
1. Place your base Swagger file (A) in the designated location
2. Configure the base API settings in appsettings.json
3. Set up basic gateway parameters (rate limiting, caching, etc.)

### 2. Adding a New Partner API
1. Obtain the partner's Swagger/OAS file
2. Place it in the swagger directory with a unique name (e.g., api-b1.json)
3. Create transformation rules:
   - Request transformation (your format → partner format)
   - Response transformation (partner format → your format)
4. Add the partner configuration to appsettings.json

### 3. Configuration Steps for Each Partner
1. Define endpoint mappings:
   ```json
   {
     "SourceEndpoint": "/api/v1/resource",
     "TargetEndpoint": "/api/partner1/resource",
     "HttpMethod": "GET",
     "TargetHttpMethod": "GET"
   }
   ```

2. Set up transformations:
   ```json
   {
     "TransformationRules": {
       "RequestTransform": "transform-request-b1.json",
       "ResponseTransform": "transform-response-b1.json"
     }
   }
   ```

## Request Flow

1. **Incoming Request**
   - Request arrives at your API endpoint
   - Gateway validates against your Swagger (A)

2. **Transformation**
   - Request is transformed according to rules
   - Headers, body, and parameters are mapped

3. **Forwarding**
   - Transformed request is sent to partner API
   - Rate limiting and security checks applied

4. **Response Handling**
   - Partner response is received
   - Response is transformed back to your format
   - Caching is applied if configured

## Best Practices

1. **Version Management**
   - Keep track of Swagger file versions
   - Document changes in partner APIs
   - Version your transformation rules

2. **Testing**
   - Test each transformation individually
   - Validate request/response pairs
   - Monitor performance metrics

3. **Maintenance**
   - Regular validation of Swagger files
   - Update transformations when APIs change
   - Monitor error rates and performance

4. **Security**
   - Implement authentication/authorization
   - Validate request/response data
   - Rate limit by partner/endpoint

## Troubleshooting

### Common Issues
1. **Transformation Errors**
   - Check transformation rule syntax
   - Verify field mappings
   - Validate data types

2. **Gateway Issues**
   - Check configuration syntax
   - Verify endpoint accessibility
   - Review rate limiting settings

3. **Partner API Changes**
   - Monitor for API changes
   - Update Swagger files
   - Adjust transformations

## Monitoring and Maintenance

1. **Performance Monitoring**
   - Track response times
   - Monitor error rates
   - Check resource usage

2. **Regular Updates**
   - Review partner API changes
   - Update transformation rules
   - Optimize configurations

3. **Documentation**
   - Keep partner API contacts updated
   - Document custom transformations
   - Maintain change logs
