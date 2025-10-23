# Test script for Smart Bulb Functions
# Replace YOUR_BULB_IP with your actual smart bulb IP address

$bulbIP = "192.168.68.56"  # Replace with your bulb's IP
$baseUrl = "http://localhost:7210/api"  # Local Functions URL

# Test 1: Quick Action - Turn bulb on
Write-Host "Test 1: Turning bulb ON..." -ForegroundColor Green
$quickActionPayload = @{
    bulbIP = $bulbIP
    action = "on"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/BulbQuickAction" -Method POST -Body $quickActionPayload -ContentType "application/json"
    Write-Host "Success: $($response.message)" -ForegroundColor Green
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Start-Sleep 2

# Test 2: Quick Action - Set to blue
Write-Host "`nTest 2: Setting bulb to BLUE..." -ForegroundColor Blue
$quickActionPayload = @{
    bulbIP = $bulbIP
    action = "blue"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/BulbQuickAction" -Method POST -Body $quickActionPayload -ContentType "application/json"
    Write-Host "Success: $($response.message)" -ForegroundColor Green
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Start-Sleep 3

# Test 3: Rainbow Pattern
Write-Host "`nTest 3: Starting RAINBOW pattern..." -ForegroundColor Yellow
$rainbowPayload = @{
    bulbIP = $bulbIP
    colors = @(
        @{ hex = "#FF0000"; durationMs = 500 },  # Red
        @{ hex = "#FF7F00"; durationMs = 500 },  # Orange
        @{ hex = "#FFFF00"; durationMs = 500 },  # Yellow
        @{ hex = "#00FF00"; durationMs = 500 },  # Green
        @{ hex = "#0000FF"; durationMs = 500 },  # Blue
        @{ hex = "#4B0082"; durationMs = 500 },  # Indigo
        @{ hex = "#9400D3"; durationMs = 500 }   # Violet
    )
    pattern = @{
        type = "Sequential"
        repeatCount = 2
        transition = "Fade"
        transitionDurationMs = 500
    }
} | ConvertTo-Json -Depth 5

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/ControlBulb" -Method POST -Body $rainbowPayload -ContentType "application/json"
    Write-Host "Success: $($response.message)" -ForegroundColor Green
    Write-Host "Pattern will run for about 14 seconds (2 cycles x 7 colors x 1 second each)" -ForegroundColor Cyan
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nAll tests completed!" -ForegroundColor Magenta
Write-Host "Remember to replace YOUR_BULB_IP with your actual bulb IP address" -ForegroundColor Yellow