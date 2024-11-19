# API Gateway Middleware Documentation

## Overview

The API Gateway Middleware provides a flexible way to handle API requests, transform data formats, and manage various aspects of API communication.

## Endpoints

### API Mapping

```http
GET /api/mappings
```

Returns all configured API mappings.

**Response Format:**
```json
{
  "mappings": [
    {
      "sourceEndpoint": "/api/users",
      "targetEndpoint": "https://api.example.com/users",
      "httpMethod": "GET",
      "targetHttpMethod": "GET",
      "sourceFormat": "Json",
      "targetFormat": "Json"
    }
  ]
}
```

### Health Check

```http
GET /health
```

Returns the health status of the API Gateway.

**Response Format:**
```json
{
  "status": "healthy",
  "timestamp": "2024-01-20T10:00:00Z",
  "version": "1.0.0"
}
```

## Configuration

### API Mapping Configuration

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
        "TargetFormat": "Json",
        "CacheEnabled": true,
        "CacheDuration": "00:05:00"
      }
    ]
  }
}
```

### Rate Limiting Configuration

```json
{
  "RateLimit": {
    "EnableRateLimiting": true,
    "RequestsPerMinute": 60,
    "BurstSize": 10,
    "ClientIdentifier": "ApiKey"
  }
}
```

## Middleware Components

### 1. Rate Limiting Middleware

Handles request rate limiting based on configuration:
- Requests per minute
- Burst size
- Client identification

### 2. Caching Middleware

Manages response caching:
- In-memory cache
- Configurable duration
- Automatic invalidation

### 3. Error Handling Middleware

Provides consistent error handling:
- Standard error format
- Development/Production modes
- Detailed logging

### 4. API Gateway Middleware

Core request handling:
- Endpoint mapping
- Protocol translation
- Format conversion

## Response Headers

| Header | Description |
|--------|-------------|
| `X-RateLimit-Limit` | Maximum requests per minute |
| `X-RateLimit-Remaining` | Remaining requests |
| `X-RateLimit-Reset` | Time until limit reset |
| `X-Cache` | Cache status (HIT/MISS) |

## Error Responses

All error responses follow this format:

```json
{
  "error": {
    "code": "ERROR_CODE",
    "message": "Human readable message",
    "details": {
      "additionalInfo": "More details about the error"
    }
  }
}
```

Common error codes:
- `RATE_LIMIT_EXCEEDED`
- `INVALID_FORMAT`
- `TARGET_UNAVAILABLE`
- `TRANSFORMATION_ERROR`

## Examples

### JSON to XML Transformation

Request:
```http
GET /api/data
Accept: application/xml
```

Response:
```xml
<?xml version="1.0"?>
<response>
    <data>
        <item>value</item>
    </data>
</response>
```

### Rate Limited Response

```http
HTTP/1.1 429 Too Many Requests
X-RateLimit-Limit: 60
X-RateLimit-Remaining: 0
X-RateLimit-Reset: 30
Content-Type: application/json

{
  "error": {
    "code": "RATE_LIMIT_EXCEEDED",
    "message": "Too many requests"
  }
}
```
