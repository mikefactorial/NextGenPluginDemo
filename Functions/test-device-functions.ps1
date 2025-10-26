# Test script for the new device management functions
# Make sure your Azure Functions app is running locally (func start)

$baseUrl = "http://localhost:7071/api"

Write-Host "Testing Device Management Functions" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Green

# Test 1: List all devices
Write-Host "`n1. Testing ListDevices function..." -ForegroundColor Yellow
try {
    $devicesResponse = Invoke-RestMethod -Uri "$baseUrl/ListDevices" -Method GET -ContentType "application/json"
    Write-Host "? ListDevices succeeded" -ForegroundColor Green
    Write-Host "Response:" -ForegroundColor Cyan
    $devicesResponse | ConvertTo-Json -Depth 3
    
    # Store the first device IP for the alias update test
    $firstDeviceIp = $null
    if ($devicesResponse.devices -and $devicesResponse.devices.Count -gt 0) {
        $firstDeviceIp = $devicesResponse.devices[0].ip
        Write-Host "First device IP found: $firstDeviceIp" -ForegroundColor Cyan
    }
} catch {
    Write-Host "? ListDevices failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Update device alias (if we have a device IP)
if ($firstDeviceIp) {
    Write-Host "`n2. Testing UpdateDeviceAlias function..." -ForegroundColor Yellow
    
    $aliasPayload = @{
        alias = "Test Device - $(Get-Date -Format 'HH:mm:ss')"
    } | ConvertTo-Json
    
    try {
        $aliasResponse = Invoke-RestMethod -Uri "$baseUrl/devices/$firstDeviceIp/alias" -Method POST -Body $aliasPayload -ContentType "application/json"
        Write-Host "? UpdateDeviceAlias succeeded" -ForegroundColor Green
        Write-Host "Response:" -ForegroundColor Cyan
        $aliasResponse | ConvertTo-Json -Depth 2
    } catch {
        Write-Host "? UpdateDeviceAlias failed: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Error details: $($_.ErrorDetails.Message)" -ForegroundColor Red
    }
    
    # Test 3: List devices again to verify the alias change
    Write-Host "`n3. Verifying alias update..." -ForegroundColor Yellow
    try {
        $updatedDevicesResponse = Invoke-RestMethod -Uri "$baseUrl/ListDevices" -Method GET -ContentType "application/json"
        $updatedDevice = $updatedDevicesResponse.devices | Where-Object { $_.ip -eq $firstDeviceIp }
        if ($updatedDevice) {
            Write-Host "? Device found with IP: $($updatedDevice.ip)" -ForegroundColor Green
            Write-Host "   Current alias: '$($updatedDevice.alias)'" -ForegroundColor Cyan
        } else {
            Write-Host "? Could not find updated device" -ForegroundColor Red
        }
    } catch {
        Write-Host "? Verification failed: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host "`n2. Skipping UpdateDeviceAlias test - no devices found" -ForegroundColor Yellow
}

# Test 4: Test error scenarios
Write-Host "`n4. Testing error scenarios..." -ForegroundColor Yellow

# Test empty alias
Write-Host "Testing empty alias..." -ForegroundColor Magenta
$emptyAliasPayload = @{
    alias = ""
} | ConvertTo-Json

try {
    $errorResponse = Invoke-RestMethod -Uri "$baseUrl/devices/192.168.1.1/alias" -Method POST -Body $emptyAliasPayload -ContentType "application/json"
    Write-Host "? Should have failed with empty alias" -ForegroundColor Red
} catch {
    Write-Host "? Correctly rejected empty alias: $($_.ErrorDetails.Message)" -ForegroundColor Green
}

# Test invalid device IP format
Write-Host "Testing with invalid device IP..." -ForegroundColor Magenta
$validAliasPayload = @{
    alias = "Test Device"
} | ConvertTo-Json

try {
    $errorResponse = Invoke-RestMethod -Uri "$baseUrl/devices/invalid-ip/alias" -Method POST -Body $validAliasPayload -ContentType "application/json"
    Write-Host "Response for invalid IP: $($errorResponse | ConvertTo-Json)" -ForegroundColor Yellow
} catch {
    Write-Host "Expected error for invalid IP: $($_.ErrorDetails.Message)" -ForegroundColor Yellow
}

Write-Host "`n? Testing completed!" -ForegroundColor Green
Write-Host "`nEndpoints added:" -ForegroundColor Cyan
Write-Host "- GET  /api/ListDevices" -ForegroundColor White
Write-Host "- POST /api/devices/{deviceIp}/alias" -ForegroundColor White
Write-Host "`nExample usage:" -ForegroundColor Cyan
Write-Host "# List all devices:" -ForegroundColor White
Write-Host "curl -X GET `"http://localhost:7071/api/ListDevices`"" -ForegroundColor Gray
Write-Host "`n# Update device alias:" -ForegroundColor White
Write-Host "curl -X POST `"http://localhost:7071/api/devices/192.168.1.100/alias`" \" -ForegroundColor Gray
Write-Host "     -H `"Content-Type: application/json`" \" -ForegroundColor Gray
Write-Host "     -d '{`"alias`": `"Living Room Light`"}'" -ForegroundColor Gray