# Swagger Analysis Example

This example demonstrates how to use the API Gateway's Swagger analysis feature to automatically map between two different APIs: a Product Catalog API and an Inventory Management API.

## Sample Files

1. `source-api.json`: Product Catalog API (Your API)
   - Endpoints for managing products
   - Basic product information
   - Stock availability

2. `target-api.json`: Inventory Management API (Partner API)
   - Endpoints for inventory management
   - Detailed item information
   - Quantity tracking

## Usage Example

### 1. Start the API Gateway
```bash
cd ../../
dotnet run
```

### 2. Analyze the APIs
Using curl:
```bash
curl -X POST http://localhost:5000/api/swaggeranalyzer/analyze \
  -F "sourceSwagger=@samples/swagger-analysis/source-api.json" \
  -F "targetSwagger=@samples/swagger-analysis/target-api.json"
```

Using PowerShell:
```powershell
$sourceFile = Get-Item "samples/swagger-analysis/source-api.json"
$targetFile = Get-Item "samples/swagger-analysis/target-api.json"
$form = @{
    sourceSwagger = $sourceFile
    targetSwagger = $targetFile
}
Invoke-RestMethod -Uri "http://localhost:5000/api/swaggeranalyzer/analyze" -Method Post -Form $form
```

### 3. Expected Response

```json
{
  "configuration": {
    "apiGateway": {
      "swaggerConfigurations": {
        "baseApi": {
          "swaggerPath": "./swagger/api-a.json",
          "description": "Product Catalog API"
        },
        "partnerApi": {
          "swaggerPath": "./swagger/api-b.json",
          "mappings": [
            {
              "sourceEndpoint": "/api/products",
              "targetEndpoint": "/inventory/items",
              "httpMethod": "GET",
              "targetHttpMethod": "GET",
              "dataTransformations": [
                {
                  "sourceType": "Product",
                  "targetType": "Item",
                  "propertyMappings": {
                    "productId": "itemId",
                    "name": "itemName",
                    "description": "details",
                    "price": "cost",
                    "category": "itemType",
                    "inStock": "available"
                  }
                }
              ]
            },
            {
              "sourceEndpoint": "/api/products/{id}",
              "targetEndpoint": "/inventory/items/{itemId}",
              "httpMethod": "GET",
              "targetHttpMethod": "GET"
            }
          ]
        }
      }
    }
  },
  "analysis": {
    "endpointMappings": [
      {
        "sourceEndpoint": "/api/products",
        "targetEndpoint": "/inventory/items",
        "httpMethod": "GET",
        "targetHttpMethod": "GET",
        "confidence": 0.95,
        "status": "Excellent Match"
      },
      {
        "sourceEndpoint": "/api/products/{id}",
        "targetEndpoint": "/inventory/items/{itemId}",
        "httpMethod": "GET",
        "targetHttpMethod": "GET",
        "confidence": 0.92,
        "status": "Excellent Match"
      }
    ],
    "dataTransformations": [
      {
        "sourceType": "Product",
        "targetType": "Item",
        "propertyMappings": {
          "productId": "itemId",
          "name": "itemName",
          "description": "details",
          "price": "cost",
          "category": "itemType",
          "inStock": "available"
        },
        "confidence": 0.89,
        "status": "Good Match"
      }
    ]
  }
}
```

## Understanding the Results

1. **Endpoint Mappings**
   - The analyzer found matching endpoints between the APIs
   - High confidence scores indicate good matches
   - Path parameters are correctly mapped

2. **Data Transformations**
   - Property mappings between Product and Item schemas
   - Type conversions are preserved
   - Required fields are maintained

3. **Configuration Generation**
   - Complete API Gateway configuration
   - Ready-to-use mappings
   - Transformation rules included

## Next Steps

1. Review the generated configuration
2. Adjust mappings if needed
3. Apply the configuration to the API Gateway
4. Test the integration

## Additional Examples

Check other examples in the samples directory for:
- SOAP to REST mapping
- GraphQL integration
- Custom protocol handling
