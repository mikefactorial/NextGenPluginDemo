using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using IOTBulbFunctions.Services;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Register configuration - flat settings without colons
builder.Services.AddOptions<BulbApiSettings>()
    .Configure<IConfiguration>((settings, configuration) =>
    {
        settings.BulbApiBaseUrl = configuration["BulbApiBaseUrl"] ?? string.Empty;
        settings.BulbApiKey = configuration["BulbApiKey"] ?? string.Empty;
    });

// Register services - Use AddHttpClient which automatically registers the service
builder.Services.AddHttpClient<IBulbControlService, BulbControlService>();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
