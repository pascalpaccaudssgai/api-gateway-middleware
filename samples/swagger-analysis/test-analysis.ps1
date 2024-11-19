# PowerShell script to test the Swagger analysis feature

Write-Host "Testing Swagger Analysis Feature" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Green

# Ensure the API Gateway is running
$gatewayUrl = "http://localhost:5000"
$analyzerEndpoint = "$gatewayUrl/api/swaggeranalyzer/analyze"

# Get the current directory
$currentDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# Define source and target Swagger files
$sourceFile = Join-Path $currentDir "source-api.json"
$targetFile = Join-Path $currentDir "target-api.json"

Write-Host "`nChecking files..." -ForegroundColor Yellow
if (-not (Test-Path $sourceFile)) {
    Write-Host "Error: Source API file not found at $sourceFile" -ForegroundColor Red
    exit 1
}
if (-not (Test-Path $targetFile)) {
    Write-Host "Error: Target API file not found at $targetFile" -ForegroundColor Red
    exit 1
}

Write-Host "Source API: $sourceFile" -ForegroundColor Cyan
Write-Host "Target API: $targetFile" -ForegroundColor Cyan

# Test API Gateway availability
Write-Host "`nTesting API Gateway availability..." -ForegroundColor Yellow
try {
    $null = Invoke-RestMethod -Uri $gatewayUrl -Method Head
    Write-Host "API Gateway is running" -ForegroundColor Green
}
catch {
    Write-Host "Error: API Gateway is not running at $gatewayUrl" -ForegroundColor Red
    Write-Host "Please start the API Gateway using 'dotnet run' in the project root" -ForegroundColor Yellow
    exit 1
}

# Analyze the Swagger files
Write-Host "`nAnalyzing Swagger files..." -ForegroundColor Yellow
try {
    $form = @{
        sourceSwagger = Get-Item $sourceFile
        targetSwagger = Get-Item $targetFile
    }
    
    $result = Invoke-RestMethod -Uri $analyzerEndpoint -Method Post -Form $form
    
    # Create output directory if it doesn't exist
    $outputDir = Join-Path $currentDir "output"
    if (-not (Test-Path $outputDir)) {
        New-Item -ItemType Directory -Path $outputDir | Out-Null
    }
    
    # Save the analysis result
    $outputFile = Join-Path $outputDir "analysis-result.json"
    $result | ConvertTo-Json -Depth 10 | Out-File $outputFile
    
    Write-Host "`nAnalysis completed successfully!" -ForegroundColor Green
    Write-Host "Results saved to: $outputFile" -ForegroundColor Cyan
    
    # Display summary
    Write-Host "`nAnalysis Summary:" -ForegroundColor Yellow
    Write-Host "=================" -ForegroundColor Yellow
    
    Write-Host "`nEndpoint Mappings:" -ForegroundColor Cyan
    foreach ($mapping in $result.analysis.endpointMappings) {
        Write-Host "  $($mapping.sourceEndpoint) -> $($mapping.targetEndpoint)" -ForegroundColor White
        Write-Host "    Method: $($mapping.httpMethod)" -ForegroundColor Gray
        Write-Host "    Confidence: $($mapping.confidence)" -ForegroundColor Gray
        Write-Host "    Status: $($mapping.status)" -ForegroundColor Gray
        Write-Host ""
    }
    
    Write-Host "Data Transformations:" -ForegroundColor Cyan
    foreach ($transform in $result.analysis.dataTransformations) {
        Write-Host "  $($transform.sourceType) -> $($transform.targetType)" -ForegroundColor White
        Write-Host "    Confidence: $($transform.confidence)" -ForegroundColor Gray
        Write-Host "    Status: $($transform.status)" -ForegroundColor Gray
        Write-Host "    Properties Mapped: $($transform.propertyMappings.Count)" -ForegroundColor Gray
        Write-Host ""
    }
}
catch {
    Write-Host "Error analyzing Swagger files:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

Write-Host "`nNext Steps:" -ForegroundColor Yellow
Write-Host "1. Review the analysis results in: $outputFile" -ForegroundColor White
Write-Host "2. Apply the generated configuration to your API Gateway" -ForegroundColor White
Write-Host "3. Test the API integration" -ForegroundColor White
