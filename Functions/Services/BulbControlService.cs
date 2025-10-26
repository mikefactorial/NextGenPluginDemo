using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NextGenDemo.Shared.Models;

namespace IOTBulbFunctions.Services
{
    public interface IBulbControlService
    {
        Task<bool> SetBulbPowerAsync(string bulbIP, bool isOn);
        Task<bool> SetBulbColorHexAsync(string bulbIP, string hex);
        Task<bool> SetBulbColorHSBAsync(string bulbIP, int hue, int saturation, int? brightness = null);
        Task<bool> SetBulbTemperatureAsync(string bulbIP, int kelvin);
        Task<bool> SetBulbBrightnessAsync(string bulbIP, int brightness);
        Task ExecuteColorPatternAsync(BulbControlRequest request, CancellationToken cancellationToken = default);
        Task<List<DeviceInfo>?> GetDevicesAsync();
        Task<bool> UpdateDeviceAliasAsync(string deviceIp, string newAlias);
    }

    public class BulbControlService : IBulbControlService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BulbControlService> _logger;
        private readonly BulbApiSettings _apiSettings;

        public BulbControlService(HttpClient httpClient, ILogger<BulbControlService> logger, IOptions<BulbApiSettings> apiSettings)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiSettings = apiSettings?.Value ?? throw new ArgumentNullException(nameof(apiSettings));
            
            // Validate configuration
            if (string.IsNullOrEmpty(_apiSettings.BulbApiBaseUrl))
            {
                _logger.LogError("BulbApiBaseUrl is not configured");
                throw new InvalidOperationException("BulbApiBaseUrl configuration is required");
            }
            
            // Configure default headers
            _httpClient.DefaultRequestHeaders.Add("ngrok-skip-browser-warning", "true");
            
            // Add the API key header securely
            if (!string.IsNullOrEmpty(_apiSettings.BulbApiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("X-API-Key", _apiSettings.BulbApiKey);
                _logger.LogInformation("BulbControlService initialized with API key authentication");
            }
            else
            {
                _logger.LogWarning("API key not configured. Some API calls may fail.");
            }
            
            // Set timeout for HTTP requests
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            
            _logger.LogInformation("BulbControlService initialized for base URL: {BaseUrl}", _apiSettings.BulbApiBaseUrl);
        }

        public async Task<List<DeviceInfo>?> GetDevicesAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving device list from API");
                var response = await _httpClient.GetAsync($"{_apiSettings.BulbApiBaseUrl}/devices");
                _logger.LogInformation("Successfully retrieved device list");

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Deserializing device list response");
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Successfully retrieved device list");
                    
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var devices = JsonSerializer.Deserialize<List<DeviceInfo>>(content, options);
                    _logger.LogInformation("Retrieved {DeviceCount} devices", devices?.Count ?? 0);
                    return devices;
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve devices. Status: {StatusCode}, Response: {Response}", 
                        response.StatusCode, await response.Content.ReadAsStringAsync());
                    return null;
                }
            }
            catch (ObjectDisposedException ex)
            {
                _logger.LogError(ex, "HttpClient disposed while retrieving devices. This may indicate a service registration issue.");
                return null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error while retrieving devices");
                return null;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout while retrieving devices");
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize device list response");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error while retrieving device: {ex.ToString()}");
                return null;
            }
        }

        public async Task<bool> UpdateDeviceAliasAsync(string deviceIp, string newAlias)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(deviceIp))
                {
                    _logger.LogWarning("Device IP cannot be null or empty");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(newAlias))
                {
                    _logger.LogWarning("New alias cannot be null or empty");
                    return false;
                }

                var payload = new UpdateAliasRequest { Alias = newAlias };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                _logger.LogInformation("Updating device {DeviceIp} alias to {NewAlias}", deviceIp, newAlias);
                var response = await _httpClient.PostAsync($"{_apiSettings.BulbApiBaseUrl}/{deviceIp}/alias", content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully updated device {DeviceIp} alias to {NewAlias}", deviceIp, newAlias);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to update device alias. Status: {StatusCode}, Response: {Response}", 
                        response.StatusCode, await response.Content.ReadAsStringAsync());
                    return false;
                }
            }
            catch (ObjectDisposedException ex)
            {
                _logger.LogError(ex, "HttpClient disposed while updating device alias for {DeviceIp}. This may indicate a service registration issue.", deviceIp);
                return false;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error while updating device alias for {DeviceIp}", deviceIp);
                return false;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout while updating device alias for {DeviceIp}", deviceIp);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating device alias for {DeviceIp}", deviceIp);
                return false;
            }
        }

        public async Task<bool> SetBulbPowerAsync(string bulbIP, bool isOn)
        {
            try
            {
                var endpoint = isOn ? "on" : "off";
                _logger.LogInformation("Setting bulb {BulbIP} power {State}", bulbIP, endpoint);
                var response = await _httpClient.GetAsync($"{_apiSettings.BulbApiBaseUrl}/test/{bulbIP}/{endpoint}");
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully set bulb {BulbIP} power {State}", bulbIP, endpoint);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to set bulb power. Status: {StatusCode}, Response: {Response}", 
                        response.StatusCode, await response.Content.ReadAsStringAsync());
                    return false;
                }
            }
            catch (ObjectDisposedException ex)
            {
                _logger.LogError(ex, "HttpClient disposed while setting bulb power state for {BulbIP}. This may indicate a service registration issue.", bulbIP);
                return false;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error while setting bulb power state for {BulbIP}", bulbIP);
                return false;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout while setting bulb power state for {BulbIP}", bulbIP);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while setting bulb power state for {BulbIP}", bulbIP);
                return false;
            }
        }

        public async Task<bool> SetBulbColorHexAsync(string bulbIP, string hex)
        {
            try
            {
                var payload = new { hex = hex.StartsWith("#") ? hex : $"#{hex}" };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                _logger.LogInformation("Setting bulb {BulbIP} to hex color {Hex}", bulbIP, hex);
                var response = await _httpClient.PostAsync($"{_apiSettings.BulbApiBaseUrl}/bulb/{bulbIP}/hex", content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully set bulb {BulbIP} to hex color {Hex}", bulbIP, hex);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to set bulb color. Status: {StatusCode}, Response: {Response}", 
                        response.StatusCode, await response.Content.ReadAsStringAsync());
                    return false;
                }
            }
            catch (ObjectDisposedException ex)
            {
                _logger.LogError(ex, "HttpClient disposed while setting bulb color (hex) for {BulbIP}. This may indicate a service registration issue.", bulbIP);
                return false;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error while setting bulb color (hex) for {BulbIP}", bulbIP);
                return false;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout while setting bulb color (hex) for {BulbIP}", bulbIP);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while setting bulb color (hex) for {BulbIP}", bulbIP);
                return false;
            }
        }

        public async Task<bool> SetBulbColorHSBAsync(string bulbIP, int hue, int saturation, int? brightness = null)
        {
            try
            {
                var payload = new 
                { 
                    hue = hue, 
                    saturation = saturation,
                    brightness = brightness
                };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_apiSettings.BulbApiBaseUrl}/bulb/{bulbIP}/color", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set bulb color (HSB) for {BulbIP}", bulbIP);
                return false;
            }
        }

        public async Task<bool> SetBulbTemperatureAsync(string bulbIP, int kelvin)
        {
            try
            {
                var payload = new { kelvin = kelvin };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_apiSettings.BulbApiBaseUrl}/bulb/{bulbIP}/temperature", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set bulb temperature for {BulbIP}", bulbIP);
                return false;
            }
        }

        public async Task<bool> SetBulbBrightnessAsync(string bulbIP, int brightness)
        {
            try
            {
                var payload = new { brightness = brightness };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_apiSettings.BulbApiBaseUrl}/bulb/{bulbIP}/brightness", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set bulb brightness for {BulbIP}", bulbIP);
                return false;
            }
        }

        public async Task ExecuteColorPatternAsync(BulbControlRequest request, CancellationToken cancellationToken = default)
        {
            if (!request.Colors.Any())
            {
                _logger.LogWarning("No colors provided for pattern execution");
                return;
            }

            var colors = request.Colors.ToList();
            var random = new Random();
            var repeatCount = request.Pattern.RepeatCount == 0 ? int.MaxValue : request.Pattern.RepeatCount;

            // Turn on the bulb first
            await SetBulbPowerAsync(request.BulbIP, true);

            for (int cycle = 0; cycle < repeatCount && !cancellationToken.IsCancellationRequested; cycle++)
            {
                var colorsToProcess = request.Pattern.Type switch
                {
                    PatternType.Sequential => colors,
                    PatternType.Random => colors.OrderBy(x => random.Next()).ToList(),
                    PatternType.PingPong => cycle % 2 == 0 ? colors : colors.AsEnumerable().Reverse().ToList(),
                    _ => colors
                };

                foreach (var colorStep in colorsToProcess)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    await ApplyColorStepAsync(request.BulbIP, colorStep);

                    if (request.Pattern.Type == PatternType.Pulse)
                    {
                        // For pulse effect, dim the color and then restore
                        await Task.Delay(colorStep.DurationMs / 3, cancellationToken);
                        if (colorStep.Brightness.HasValue)
                        {
                            await SetBulbBrightnessAsync(request.BulbIP, Math.Max(1, colorStep.Brightness.Value / 4));
                        }
                        await Task.Delay(colorStep.DurationMs / 3, cancellationToken);
                        await ApplyColorStepAsync(request.BulbIP, colorStep); // Restore full brightness
                        await Task.Delay(colorStep.DurationMs / 3, cancellationToken);
                    }
                    else
                    {
                        await Task.Delay(colorStep.DurationMs, cancellationToken);
                    }

                    // Apply transition effect
                    if (request.Pattern.Transition == TransitionType.Flash)
                    {
                        await SetBulbPowerAsync(request.BulbIP, false);
                        await Task.Delay(request.Pattern.TransitionDurationMs / 2, cancellationToken);
                        await SetBulbPowerAsync(request.BulbIP, true);
                        await Task.Delay(request.Pattern.TransitionDurationMs / 2, cancellationToken);
                    }
                    else if (request.Pattern.Transition == TransitionType.Fade)
                    {
                        // Simulate fade by dimming briefly
                        var currentBrightness = colorStep.Brightness ?? 100;
                        await SetBulbBrightnessAsync(request.BulbIP, Math.Max(1, currentBrightness / 2));
                        await Task.Delay(request.Pattern.TransitionDurationMs, cancellationToken);
                    }
                }
            }
        }

        private async Task ApplyColorStepAsync(string bulbIP, ColorStep colorStep)
        {
            bool success = false;

            // Priority: Hex > HSB > Temperature
            if (!string.IsNullOrEmpty(colorStep.Hex))
            {
                success = await SetBulbColorHexAsync(bulbIP, colorStep.Hex);
            }
            else if (colorStep.Hue.HasValue && colorStep.Saturation.HasValue)
            {
                success = await SetBulbColorHSBAsync(bulbIP, colorStep.Hue.Value, colorStep.Saturation.Value, colorStep.Brightness);
            }
            else if (colorStep.Kelvin.HasValue)
            {
                success = await SetBulbTemperatureAsync(bulbIP, colorStep.Kelvin.Value);
            }

            // Set brightness separately if not already set with HSB
            if (success && colorStep.Brightness.HasValue && string.IsNullOrEmpty(colorStep.Hex) && 
                (!colorStep.Hue.HasValue || !colorStep.Saturation.HasValue))
            {
                await SetBulbBrightnessAsync(bulbIP, colorStep.Brightness.Value);
            }
        }
    }
}