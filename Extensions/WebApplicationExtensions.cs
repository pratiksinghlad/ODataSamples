using Microsoft.EntityFrameworkCore;
using ODataSamples.Data.Context;
using ODataSamples.Data.Seeding;
using System.Net;
using System.Text.Json;

namespace ODataSamples.Extensions;

/// <summary>
/// Extension methods for configuring the application pipeline
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures the middleware pipeline for development environment
    /// </summary>
    /// <param name="app">The web application</param>
    /// <returns>The web application for chaining</returns>
    public static WebApplication ConfigureDevelopmentPipeline(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseDeveloperExceptionPage();
            app.UseCors("Development");
        }

        return app;
    }

    /// <summary>
    /// Configures the middleware pipeline for production environment
    /// </summary>
    /// <param name="app">The web application</param>
    /// <returns>The web application for chaining</returns>
    public static WebApplication ConfigureProductionPipeline(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/error");
            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseCors("Production");
        }

        return app;
    }

    /// <summary>
    /// Adds global exception handling middleware
    /// </summary>
    /// <param name="app">The web application</param>
    /// <returns>The web application for chaining</returns>
    public static WebApplication UseGlobalExceptionHandler(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        return app;
    }

    /// <summary>
    /// Initializes and seeds the database
    /// </summary>
    /// <param name="app">The web application</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public static async Task<WebApplication> InitializeDatabaseAsync(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("Applying database migrations...");
            await context.Database.MigrateAsync();

            logger.LogInformation("Database migrations applied successfully. Seeding data...");
            var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
            await seeder.SeedAsync();

            logger.LogInformation("Database initialization completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database");
            
            if (app.Environment.IsDevelopment())
            {
                throw; // Re-throw in development to see the error
            }
            else
            {
                // In production, log and exit gracefully
                Environment.Exit(1);
            }
        }

        return app;
    }
}

/// <summary>
/// Global exception handling middleware for production-ready error responses
/// </summary>
public sealed class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalExceptionHandlerMiddleware"/> class
    /// </summary>
    /// <param name="next">The next middleware in the pipeline</param>
    /// <param name="logger">The logger instance</param>
    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes the HTTP request and handles any unhandled exceptions
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while processing the request");
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Handles exceptions and returns appropriate HTTP responses
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <param name="exception">The exception that occurred</param>
    /// <returns>A task representing the asynchronous operation</returns>
    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            ArgumentNullException => (HttpStatusCode.BadRequest, "Invalid request data"),
            ArgumentException => (HttpStatusCode.BadRequest, "Invalid request parameters"),
            InvalidOperationException => (HttpStatusCode.BadRequest, "Invalid operation"),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized access"),
            NotImplementedException => (HttpStatusCode.NotImplemented, "Feature not implemented"),
            TimeoutException => (HttpStatusCode.RequestTimeout, "Request timeout"),
            _ => (HttpStatusCode.InternalServerError, "An internal server error occurred")
        };

        var response = new
        {
            error = new
            {
                message = message,
                statusCode = (int)statusCode,
                timestamp = DateTime.UtcNow,
                path = context.Request.Path.Value
            }
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}