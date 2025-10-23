# Test HttpClient Disposal Fix
# This script tests multiple rapid calls to ensure HttpClient isn't disposed

$bulbIP = "192.168.1.100"  # Replace with your bulb's IP
$baseUrl = "http://localhost:7071/api"

Write-Host "Testing HttpClient disposal fix..." -ForegroundColor Yellow
Write-Host "Making rapid sequential calls to test HttpClient stability" -ForegroundColor Cyan

# Test multiple quick actions in succession
$actions = @("red", "green", "blue", "white")

foreach ($action in $actions) {
    Write-Host "Setting bulb to $action..." -ForegroundColor Green
    
    $payload = @{
        bulbIP = $bulbIP
        action = $action
    } | ConvertTo-Json
    
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/BulbQuickAction" -Method POST -Body $payload -ContentType "application/json"
        Write-Host "? Success: $($response.message)" -ForegroundColor Green
    } catch {
        Write-Host "? Error: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Response: $($_.ErrorDetails.Message)" -ForegroundColor Red
    }
    
    Start-Sleep 1
}

Write-Host "`nIf no ObjectDisposedException errors appeared, the fix is working!" -ForegroundColor Magenta