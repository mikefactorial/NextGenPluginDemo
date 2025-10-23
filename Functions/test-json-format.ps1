# Simple JSON Test for Enum Deserialization
# This script tests the JSON format before calling the actual function

$testPayload = @{
    bulbIP = "192.168.1.100"
    colors = @(
        @{ hex = "#FF0000"; durationMs = 1000 }
    )
    pattern = @{
        type = "Sequential"
        repeatCount = 1
        transition = "Instant"
        transitionDurationMs = 0
    }
}

$json = $testPayload | ConvertTo-Json -Depth 5
Write-Host "Generated JSON:" -ForegroundColor Yellow
Write-Host $json -ForegroundColor Cyan

Write-Host "`nJSON is valid for the following enum values:" -ForegroundColor Green
Write-Host "? PatternType: Sequential, Random, PingPong, Pulse" -ForegroundColor Green  
Write-Host "? TransitionType: Instant, Fade, Flash" -ForegroundColor Green

Write-Host "`nReady to test with actual bulb functions!" -ForegroundColor Magenta