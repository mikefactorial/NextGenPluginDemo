using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using IOTBulbFunctions.Services;
using NextGenDemo.Shared.Models;

namespace IOTBulbFunctions
{
    public class BulbFunctions
    {
        private readonly ILogger<BulbFunctions> _logger;
        private readonly IBulbControlService _bulbControlService;

        public BulbFunctions(ILogger<BulbFunctions> logger, IBulbControlService bulbControlService)
        {
            _logger = logger;
            _bulbControlService = bulbControlService;
        }

        [Function("ControlBulb")]
        public async Task<IActionResult> ControlBulb([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            try
            {
                _logger.LogInformation("Smart bulb control function triggered.");

                // Read the request body
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                
                if (string.IsNullOrEmpty(requestBody))
                {
                    return new BadRequestObjectResult(new { error = "Request body is required" });
                }

                // Deserialize the request with enum string conversion
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                };
                
                var bulbRequest = JsonSerializer.Deserialize<BulbControlRequest>(requestBody, options);
                
                if (bulbRequest == null)
                {
                    return new BadRequestObjectResult(new { error = "Invalid request format" });
                }

                // Validate the request
                var validationResult = ValidateBulbRequest(bulbRequest);
                if (!string.IsNullOrEmpty(validationResult))
                {
                    return new BadRequestObjectResult(new { error = validationResult });
                }

                // Execute the color pattern asynchronously
                var cancellationTokenSource = new CancellationTokenSource();
                
                // Start the pattern execution in the background
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _bulbControlService.ExecuteColorPatternAsync(bulbRequest, cancellationTokenSource.Token);
                        _logger.LogInformation("Color pattern execution completed for bulb {BulbIP}", bulbRequest.BulbIP);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error executing color pattern for bulb {BulbIP}", bulbRequest.BulbIP);
                    }
                }, cancellationTokenSource.Token);

                return new OkObjectResult(new 
                { 
                    message = "Bulb control pattern started successfully",
                    bulbIP = bulbRequest.BulbIP,
                    colorCount = bulbRequest.Colors.Count,
                    pattern = bulbRequest.Pattern.Type.ToString(),
                    repeatCount = bulbRequest.Pattern.RepeatCount
                });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse JSON request");
                return new BadRequestObjectResult(new { error = "Invalid JSON format", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in ControlBulb function");
                return new StatusCodeResult(500);
            }
        }

        [Function("BulbQuickAction")]
        public async Task<IActionResult> BulbQuickAction([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            try
            {
                _logger.LogInformation("Quick bulb action function triggered.");

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                
                if (string.IsNullOrEmpty(requestBody))
                {
                    return new BadRequestObjectResult(new { error = "Request body is required" });
                }
                _logger.LogInformation($"Request Body: {requestBody}");
                var options = new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                };
                var request = JsonSerializer.Deserialize<BulbControlRequest>(requestBody, options);
                
                if (request == null || string.IsNullOrEmpty(request.BulbIP) || string.IsNullOrEmpty(request.Action))
                {
                    return new BadRequestObjectResult(new { error = "BulbIP and Action are required" });
                }

                bool success = request.Action.ToLower() switch
                {
                    "on" => await _bulbControlService.SetBulbPowerAsync(request.BulbIP, true),
                    "off" => await _bulbControlService.SetBulbPowerAsync(request.BulbIP, false),
                    "red" => await _bulbControlService.SetBulbColorHexAsync(request.BulbIP, "#FF0000"),
                    "green" => await _bulbControlService.SetBulbColorHexAsync(request.BulbIP, "#00FF00"),
                    "blue" => await _bulbControlService.SetBulbColorHexAsync(request.BulbIP, "#0000FF"),
                    "yellow" => await _bulbControlService.SetBulbColorHexAsync(request.BulbIP, "#FFFF00"),
                    "purple" => await _bulbControlService.SetBulbColorHexAsync(request.BulbIP, "#800080"),
                    "white" => await _bulbControlService.SetBulbColorHexAsync(request.BulbIP, "#FFFFFF"),
                    _ => false
                };

                if (success)
                {
                    return new OkObjectResult(new 
                    { 
                        message = $"Successfully executed action '{request.Action}' on bulb {request.BulbIP}" 
                    });
                }
                else
                {
                    return new BadRequestObjectResult(new 
                    { 
                        error = $"Failed to execute action '{request.Action}' or unsupported action" 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in BulbQuickAction function: {ex.ToString()}");
                return new StatusCodeResult(500);
            }
        }

        private static string ValidateBulbRequest(BulbControlRequest request)
        {
            if (string.IsNullOrEmpty(request.BulbIP))
                return "BulbIP is required";

            if (!request.Colors.Any())
                return "At least one color step is required";

            for (int i = 0; i < request.Colors.Count; i++)
            {
                var color = request.Colors[i];
                
                // Check if at least one color method is specified
                if (string.IsNullOrEmpty(color.Hex) && 
                    (!color.Hue.HasValue || !color.Saturation.HasValue) && 
                    !color.Kelvin.HasValue)
                {
                    return $"Color step {i + 1}: Must specify either Hex, HSB (Hue + Saturation), or Kelvin";
                }

                // Validate ranges
                if (color.Hue.HasValue && (color.Hue < 0 || color.Hue > 360))
                    return $"Color step {i + 1}: Hue must be between 0 and 360";

                if (color.Saturation.HasValue && (color.Saturation < 0 || color.Saturation > 100))
                    return $"Color step {i + 1}: Saturation must be between 0 and 100";

                if (color.Brightness.HasValue && (color.Brightness < 1 || color.Brightness > 100))
                    return $"Color step {i + 1}: Brightness must be between 1 and 100";

                if (color.Kelvin.HasValue && (color.Kelvin < 2700 || color.Kelvin > 6500))
                    return $"Color step {i + 1}: Kelvin must be between 2700 and 6500";

                if (color.DurationMs < 100)
                    return $"Color step {i + 1}: Duration must be at least 100ms";
            }

            return string.Empty;
        }
    }
}
