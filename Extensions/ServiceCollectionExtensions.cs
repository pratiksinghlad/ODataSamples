using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using ODataDemo.Model;
using ODataSamples.Data.Context;
using ODataSamples.Data.Repositories.Implementations;
using ODataSamples.Data.Repositories.Interfaces;
using ODataSamples.Data.Seeding;
using ODataSamples.Data.UnitOfWork;
using ODataSamples.Services;

namespace ODataSamples.Extensions;

/// <summary>
/// Extension methods for configuring services in the dependency injection container
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds and configures Entity Framework Core with MySQL
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <param name="environment">Web host environment</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddDatabaseServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        var connectionString = configuration.GetConnectionString("DBConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DBConnection' not found. Please ensure it is defined in your configuration.");
        }

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mySqlOptions =>
            {
                mySqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                mySqlOptions.CommandTimeout(30);
            });

            // Configure development-specific options
            if (environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
                options.LogTo(Console.WriteLine, LogLevel.Information);
            }
        });

        return services;
    }

    /// <summary>
    /// Adds repository pattern services to the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    /// <summary>
    /// Adds business services to the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<DataSeeder>();

        return services;
    }

    /// <summary>
    /// Adds and configures OData services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddODataServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Add MVC services required for OData
        services.AddMvc()
            .AddOData(options => options
                .Select()
                .Filter()
                .OrderBy()
                .Count()
                .Expand()
                .SetMaxTop(1000)
                .AddRouteComponents("odata", GetEdmModel()));

        return services;
    }

    /// <summary>
    /// Adds CORS configuration for development and production
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="environment">Web host environment</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCorsConfiguration(
        this IServiceCollection services,
        IWebHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(environment);

        services.AddCors(options =>
        {
            if (environment.IsDevelopment())
            {
                options.AddPolicy("Development", policy => policy
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            }
            else
            {
                options.AddPolicy("Production", policy => policy
                    .WithOrigins("https://yourdomain.com") // Configure for production
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            }
        });

        return services;
    }

    /// <summary>
    /// Configures the EDM model for OData endpoints
    /// </summary>
    /// <returns>The configured EDM model</returns>
    private static IEdmModel GetEdmModel()
    {
        var builder = new ODataConventionModelBuilder();

        // Register entity sets
        builder.EntitySet<OrderModel>("OrdersOData");
        builder.EntitySet<CustomerModel>("CustomersOData");
        builder.EntitySet<ProductModel>("ProductsOData");
        builder.EntitySet<OrderItemModel>("OrderItemsOData");

        // Configure entity properties for better OData support
        var productType = builder.EntityType<ProductModel>();
        productType.HasKey(p => p.Id);
        productType.Property(p => p.Name).IsRequired();

        var customerType = builder.EntityType<CustomerModel>();
        customerType.HasKey(c => c.Id);
        customerType.Property(c => c.Name).IsRequired();

        var orderType = builder.EntityType<OrderModel>();
        orderType.HasKey(o => o.Id);
        orderType.Ignore(o => o.TotalValue);
        orderType.Ignore(o => o.ItemCount);

        return builder.GetEdmModel();
    }
}