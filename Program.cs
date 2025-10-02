using ODataSamples.Extensions;
using ODataSamples.Endpoints;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);


// Add configuration sources (removed duplicates)
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json",  true, true);
builder.Configuration.AddJsonFile("secrets/appsettings.secrets.json", true, true);
builder.Configuration.AddUserSecrets<Program>();

// Configure services using extension methods
builder.Services.AddDatabaseServices(builder.Configuration, builder.Environment);
builder.Services.AddRepositoryServices();
builder.Services.AddBusinessServices();
builder.Services.AddODataServices();
builder.Services.AddCorsConfiguration(builder.Environment);

// Add OpenAPI and health checks
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

// Configure structured logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

if (builder.Environment.IsDevelopment())
{
    builder.Logging.SetMinimumLevel(LogLevel.Information);
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information);
}
else
{
    builder.Logging.SetMinimumLevel(LogLevel.Warning);
}

var app = builder.Build();

// Configure the application pipeline
app.UseGlobalExceptionHandler();
app.ConfigureDevelopmentPipeline();
app.ConfigureProductionPipeline();

// Configure routing and authorization
app.UseAuthorization();

// Add health check endpoint
app.MapHealthChecks("/health");

// Initialize database
await app.InitializeDatabaseAsync();

// Map API endpoints
app.MapProductEndpoints();
app.MapCustomerEndpoints();
app.MapOrderEndpoints();
app.MapODataEndpoints();

// Configure Scalar API documentation
app.MapScalarApiReference();

// Run the application
app.Run();