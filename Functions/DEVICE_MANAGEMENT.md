# Device Management Functions

This document describes the new device management functionality added to the Azure Functions app.

## New Functions Added

### 1. ListDevices Function

**Endpoint**: `GET /api/ListDevices`

**Description**: Retrieves a list of all connected smart devices from the remote API.

**Features**:
- No authentication required
- Returns device information including IP, alias, power state, model, and software version
- Handles network errors gracefully
- Returns HTTP 502 (Bad Gateway) if the upstream API is unavailable

**Response Format**:
```json
{
  "message": "Devices retrieved successfully",
  "deviceCount": 2,
  "devices": [
    {
      "ip": "172.20.10.3",
      "alias": "Mike!",
      "on": true,
      "model": "KL125(US)",
      "sw_ver": "1.0.15 Build 240429 Rel.154143"
    },
    {
      "ip": "172.20.10.5",
      "alias": "Mike! Smart Plug",
      "on": false,
      "model": "EP10(US)",
      "mac": "48:22:54:DE:73:A2",
      "sw_ver": "1.0.5 Build 221021 Rel.183404"
    }
  ]
}
```

### 2. UpdateDeviceAlias Function

**Endpoint**: `POST /api/devices/{deviceIp}/alias`

**Description**: Updates the friendly name (alias) of a specific device.

**Features**:
- Route parameter: `deviceIp` - IP address of the device to update
- Request body validation (alias required, max 100 characters)
- Input sanitization and error handling
- Returns detailed success/error responses

**Request Format**:
```json
{
  "alias": "Living Room Light"
}
```

**Response Format** (Success):
```json
{
  "message": "Successfully updated device alias",
  "deviceIp": "192.168.1.100",
  "newAlias": "Living Room Light"
}
```

## Service Layer Updates

### IBulbControlService Interface
Added two new methods:
- `Task<List<DeviceInfo>?> GetDevicesAsync()` - Retrieves device list from remote API
- `Task<bool> UpdateDeviceAliasAsync(string deviceIp, string newAlias)` - Updates device alias

### BulbControlService Implementation
- Comprehensive error handling for network issues, timeouts, and JSON parsing errors
- Input validation for device IP and alias parameters
- Proper HTTP client usage with authentication headers
- Detailed logging for debugging and monitoring

## New Models

### DeviceInfo Class
```csharp
public class DeviceInfo
{
    public string Ip { get; set; }
    public string Alias { get; set; }
    public bool On { get; set; }
    public string Model { get; set; }
    public string SoftwareVersion { get; set; }
    public string MacAddress { get; set; } // Optional
}
```

### UpdateAliasRequest Class
```csharp
public class UpdateAliasRequest
{
    public string Alias { get; set; }
}
```

## Error Handling

### HTTP Status Codes
- **200 OK**: Success
- **400 Bad Request**: Invalid input (empty alias, missing parameters, etc.)
- **500 Internal Server Error**: Unexpected server error
- **502 Bad Gateway**: Remote API unavailable

### Validation Rules
- Device IP: Required in URL path, cannot be empty
- Alias: Required, cannot be empty or whitespace, maximum 100 characters
- Request body: Must be valid JSON

## Testing

### Unit Tests Coverage
- **Function Tests**: All success and error scenarios for both functions
- **Service Tests**: HTTP client interactions, error handling, payload validation
- **Input Validation**: Empty/null parameters, invalid JSON, oversized inputs
- **Network Error Simulation**: Timeouts, connection failures, invalid responses

### Test Files
- `Functions/Tests/DeviceManagementFunctionsTests.cs` - Function-level tests
- `Functions/Tests/BulbControlServiceDeviceManagementTests.cs` - Service-level tests

### Test Script
Run `Functions/test-device-functions.ps1` to test the functions locally:
```powershell
.\test-device-functions.ps1
```

## Usage Examples

### PowerShell Examples
```powershell
# List all devices
$devices = Invoke-RestMethod -Uri "http://localhost:7071/api/ListDevices" -Method GET

# Update device alias
$body = @{ alias = "Kitchen Light" } | ConvertTo-Json
Invoke-RestMethod -Uri "http://localhost:7071/api/devices/192.168.1.100/alias" -Method POST -Body $body -ContentType "application/json"
```

### cURL Examples
```bash
# List all devices
curl -X GET "http://localhost:7071/api/ListDevices"

# Update device alias
curl -X POST "http://localhost:7071/api/devices/192.168.1.100/alias" \
     -H "Content-Type: application/json" \
     -d '{"alias": "Living Room Light"}'
```

## Integration Notes

### Remote API Dependencies
- `GET /api/devices` - Returns device list
- `POST /{deviceIp}/alias` - Updates device alias

### Configuration Requirements
- `BulbApiBaseUrl` - Base URL for the remote API
- `BulbApiKey` - API key for authentication (optional)

### Logging
All functions log important events including:
- Function invocations
- API call success/failure
- Input validation errors
- Network issues and timeouts

## Deployment Notes

1. Ensure the remote API endpoints are accessible from your deployment environment
2. Configure the `BulbApiBaseUrl` and `BulbApiKey` in your application settings
3. Test connectivity to the remote API after deployment
4. Monitor logs for any authentication or network issues

## Security Considerations

- API key is properly secured in configuration
- Input validation prevents injection attacks  
- No sensitive data is logged
- HTTPS is recommended for production deployments