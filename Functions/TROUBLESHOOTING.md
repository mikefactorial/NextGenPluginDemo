# Smart Bulb Functions - Troubleshooting Guide

## Fixed Issues

### ? HttpClient ObjectDisposedException Error
**Problem**: `System.ObjectDisposedException: 'Cannot access a disposed object. Object name: 'System.Net.Http.HttpClient'.'`

**Root Cause**: Incorrect service registration in `Program.cs` caused HttpClient to be disposed prematurely.

**Solution**: 
- Removed duplicate service registration
- Used only `AddHttpClient<>` which properly manages HttpClient lifecycle
- Added null checks and validation in constructor
- Added specific error handling for disposal issues

**Before** (caused disposal errors):
```csharp
builder.Services.AddHttpClient<IBulbControlService, BulbControlService>();
builder.Services.AddScoped<IBulbControlService, BulbControlService>(); // Duplicate registration
```

**After** (properly managed):
```csharp
builder.Services.AddHttpClient<IBulbControlService, BulbControlService>(); // Single registration
```

### ? JSON Enum Serialization Error
**Problem**: `System.Text.Json.JsonException: The JSON value could not be converted to IOTBulbFunctions.Models.TransitionType`

**Solution**: Added `JsonStringEnumConverter` to handle enum string values in JSON.

**Before** (caused error):
```json
{
  "pattern": {
    "transition": 0  // Numeric enum values
  }
}
```

**After** (works correctly):
```json
{
  "pattern": {
    "transition": "Fade"  // String enum values
  }
}
```

### ? Enhanced Error Handling
**Improvements Made**:
- Added specific handling for `ObjectDisposedException`
- Added network error handling (`HttpRequestException`)
- Added timeout handling (`TaskCanceledException`) 
- Added detailed response logging for failed API calls
- Added constructor validation and null checks
- Added HTTP client timeout configuration (30 seconds)

### ? Configuration and API Key Setup
**Problem**: API calls failing due to missing headers or configuration.

**Solution**: Implemented secure configuration-based API key management.

## Current Configuration

### Enum String Values
Use these exact string values in your JSON:

**PatternType**:
- `"Sequential"` - Colors in order
- `"Random"` - Random color selection  
- `"PingPong"` - Forward then backward
- `"Pulse"` - Breathing effect

**TransitionType**:
- `"Instant"` - Immediate changes
- `"Fade"` - Gradual transitions
- `"Flash"` - Brief flash between colors

### Valid JSON Example
```json
{
  "bulbIP": "192.168.1.50",
  "colors": [
    { "hex": "#FF0000", "durationMs": 1000 },
    { "hex": "#00FF00", "durationMs": 1000 }
  ],
  "pattern": {
    "type": "Sequential",
    "repeatCount": 2,
    "transition": "Fade",
    "transitionDurationMs": 500
  }
}
```

## Common Issues & Solutions

### ?? JSON Format Errors
**Symptoms**: 
- JsonException during deserialization
- "The JSON value could not be converted" errors

**Solutions**:
1. Use string values for enums (not numbers)
2. Check property name capitalization
3. Validate JSON syntax with online validator
4. Ensure all required fields are present

### ?? API Authentication Errors
**Symptoms**:
- 401 Unauthorized responses
- API calls failing silently

**Solutions**:
1. Verify `BulbApi:ApiKey` is set in configuration
2. Check API key value matches server requirements
3. Ensure `X-API-Key` header is being sent
4. Test API directly with curl/Postman first

### ?? Network Connectivity Issues
**Symptoms**:
- Timeouts or connection refused errors
- Functions succeed but bulb doesn't respond

**Solutions**:
1. Verify bulb IP address is correct and reachable
2. Check if bulb is on the same network
3. Test API endpoint directly: `https://deidra-nonconverging-farah.ngrok-free.dev/api`
4. Verify ngrok tunnel is active and accessible

### ?? Function Execution Issues
**Symptoms**:
- Functions start but don't complete patterns
- Unexpected behavior in color sequences

**Solutions**:
1. Check Azure Functions logs for errors
2. Verify cancellation tokens aren't being triggered
3. Test with simple patterns first (1-2 colors)
4. Ensure bulb supports requested color formats

## Testing Steps

### 1. Test JSON Format
```powershell
.\test-json-format.ps1
```

### 2. Test Quick Actions First
```powershell
# Test simple on/off before complex patterns
$payload = @{ bulbIP = "YOUR_IP"; action = "on" } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:7071/api/BulbQuickAction" -Method POST -Body $payload -ContentType "application/json"
```

### 3. Test Simple Pattern
```powershell
# Start with basic 2-color pattern
$payload = @{
    bulbIP = "YOUR_IP"
    colors = @(
        @{ hex = "#FF0000"; durationMs = 2000 },
        @{ hex = "#00FF00"; durationMs = 2000 }
    )
    pattern = @{
        type = "Sequential"
        repeatCount = 1
        transition = "Instant"
        transitionDurationMs = 0
    }
} | ConvertTo-Json -Depth 5
```

### 4. Verify Logs
Check Azure Functions logs in Visual Studio or Azure Portal for detailed error information.

## Debug Commands

### Check Function Status
```powershell
# Local development
curl http://localhost:7071/api/Function1
```

### Test API Directly
```powershell
# Test the underlying bulb API
$headers = @{ "X-API-Key" = "MySecretDemo2025"; "ngrok-skip-browser-warning" = "true" }
Invoke-RestMethod -Uri "https://deidra-nonconverging-farah.ngrok-free.dev/api/test/YOUR_BULB_IP/on" -Headers $headers
```

### Validate Configuration
```powershell
# Check if settings are loaded (from logs)
# Look for: "API key not configured" warning
```

## Next Steps
1. ? Fix JSON enum serialization
2. ? Implement secure API key configuration  
3. ? Add comprehensive error handling
4. ?? Test with actual smart bulb hardware
5. ?? Deploy to Azure and test production environment