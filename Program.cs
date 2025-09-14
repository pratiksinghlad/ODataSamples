using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using ODataDemo.Model;
using ODataSamples.Data.Context;
using ODataSamples.Data.Repositories.Interfaces;
using ODataSamples.Data.Repositories.Implementations;
using ODataSamples.Data.UnitOfWork;
using ODataSamples.Data.Seeding;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add configuration sources
builder.Configuration.AddJsonFile("appsettings.secret.json", optional: true, reloadOnChange: true);

// Add services to the container
builder.Services.AddControllers()
    .AddOData(options => options
        .Select()
        .Filter()
        .OrderBy()
        .Count()
        .Expand()
        .SetMaxTop(1000)
        .AddRouteComponents("odata", GetEdmModel()));

// Configure Entity Framework with MySQL
var connectionString = builder.Configuration.GetConnectionString("DBConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DBConnection' not found. Please ensure it is defined in your appsettings.json or appsettings.secret.json configuration files under the 'ConnectionStrings' section.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Use MySQL for all environments
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mySqlOptions =>
    {
        mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
        mySqlOptions.CommandTimeout(30);
    });
    
    // Enable detailed errors in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Register repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Register Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register data seeding service
builder.Services.AddScoped<DataSeeder>();

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}

app.MapScalarApiReference();
app.UseAuthorization();
app.UseCors("AllowAll");

// Add health check endpoint
app.MapHealthChecks("/health");

// Database initialization and seeding
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Applying database migrations...");
        
        // Apply any pending migrations
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
}

// OData Controllers (with EF Core and Repository pattern)
app.MapControllers();

// Minimal API endpoints for Products (non-OData) - Enhanced with EF Core
var productsApi = app.MapGroup("api/products")
    .WithTags("Products")
    .WithOpenApi();

productsApi.MapGet("/", async (IUnitOfWork unitOfWork) =>
{
    try
    {
        var products = await unitOfWork.Products.GetAll().ToListAsync();
        return Results.Ok(products);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            title: "Error retrieving products",
            statusCode: 500);
    }
})
    .WithName("GetAllProducts")
    .WithSummary("Get all products using EF Core");

productsApi.MapGet("/{id:int}", async (int id, IUnitOfWork unitOfWork) =>
{
    try
    {
        var product = await unitOfWork.Products.GetByIdAsync(id);
        return product is not null ? Results.Ok(product) : Results.NotFound();
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            title: "Error retrieving product",
            statusCode: 500);
    }
})
    .WithName("GetProductById")
    .WithSummary("Get product by ID using EF Core");

productsApi.MapPost("/", async (ProductModel product, IUnitOfWork unitOfWork) =>
{
    try
    {
        product.Id = 0; // Ensure auto-generation
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;

        var createdProduct = await unitOfWork.Products.AddAsync(product);
        await unitOfWork.SaveChangesAsync();
        
        return Results.Created($"/api/products/{createdProduct.Id}", createdProduct);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            title: "Error creating product",
            statusCode: 500);
    }
})
    .WithName("CreateProduct")
    .WithSummary("Create a new product using EF Core");

productsApi.MapPut("/{id:int}", async (int id, ProductModel updatedProduct, IUnitOfWork unitOfWork) =>
{
    try
    {
        var existingProduct = await unitOfWork.Products.GetByIdAsync(id);
        if (existingProduct is null)
            return Results.NotFound();

        existingProduct.Name = updatedProduct.Name;
        existingProduct.Price = updatedProduct.Price;
        existingProduct.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Products.Update(existingProduct);
        await unitOfWork.SaveChangesAsync();
        
        return Results.Ok(existingProduct);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            title: "Error updating product",
            statusCode: 500);
    }
})
    .WithName("UpdateProduct")
    .WithSummary("Update an existing product using EF Core");

productsApi.MapDelete("/{id:int}", async (int id, IUnitOfWork unitOfWork) =>
{
    try
    {
        var deleted = await unitOfWork.Products.DeleteByIdAsync(id);
        if (!deleted)
            return Results.NotFound();

        await unitOfWork.SaveChangesAsync();
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            title: "Error deleting product",
            statusCode: 500);
    }
})
    .WithName("DeleteProduct")
    .WithSummary("Delete a product using EF Core");

// Enhanced Orders API with EF Core
var ordersApi = app.MapGroup("api/orders")
    .WithTags("Orders")
    .WithOpenApi();

ordersApi.MapGet("/", async (IUnitOfWork unitOfWork) =>
{
    try
    {
        var orders = await unitOfWork.Orders.GetWithFullDetails().ToListAsync();
        return Results.Ok(orders);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            title: "Error retrieving orders",
            statusCode: 500);
    }
})
    .WithName("GetAllOrders")
    .WithSummary("Get all orders with full details using EF Core");

ordersApi.MapGet("/{id:int}", async (int id, IUnitOfWork unitOfWork) =>
{
    try
    {
        var order = await unitOfWork.Orders.GetByIdAsync(id, o => o.Customer!, o => o.OrderItems!);
        return order is not null ? Results.Ok(order) : Results.NotFound();
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            title: "Error retrieving order",
            statusCode: 500);
    }
})
    .WithName("GetOrderById")
    .WithSummary("Get order by ID with details using EF Core");

// Enhanced Customers API with EF Core
var customersApi = app.MapGroup("api/customers")
    .WithTags("Customers")
    .WithOpenApi();

customersApi.MapGet("/", async (IUnitOfWork unitOfWork) =>
{
    try
    {
        var customers = await unitOfWork.Customers.GetAll().ToListAsync();
        return Results.Ok(customers);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            title: "Error retrieving customers",
            statusCode: 500);
    }
})
    .WithName("GetAllCustomers")
    .WithSummary("Get all customers using EF Core");

customersApi.MapGet("/{id:int}", async (int id, IUnitOfWork unitOfWork) =>
{
    try
    {
        var customer = await unitOfWork.Customers.GetByIdAsync(id);
        return customer is not null ? Results.Ok(customer) : Results.NotFound();
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            title: "Error retrieving customer",
            statusCode: 500);
    }
})
    .WithName("GetCustomerById")
    .WithSummary("Get customer by ID using EF Core");

app.Run();

/// <summary>
/// Configures the EDM model for OData endpoints
/// </summary>
/// <returns>The configured EDM model</returns>
static IEdmModel GetEdmModel()
{
    var builder = new ODataConventionModelBuilder();

    builder.EntitySet<OrderModel>("OrdersOData");
    builder.EntitySet<CustomerModel>("CustomersOData");
    builder.EntitySet<ProductModel>("ProductsOData");
    builder.EntitySet<OrderItemModel>("OrderItemsOData");

    // Configure entity properties for better OData support
    builder.EntityType<ProductModel>()
        .HasKey(p => p.Id);
    
    builder.EntityType<ProductModel>()
        .Property(p => p.Name).IsRequired();
    
    builder.EntityType<CustomerModel>()
        .HasKey(c => c.Id);
    
    builder.EntityType<CustomerModel>()
        .Property(c => c.Name).IsRequired();
    
    var orderType = builder.EntityType<OrderModel>();
    orderType.HasKey(o => o.Id);
    orderType.Ignore(o => o.TotalValue);
    orderType.Ignore(o => o.ItemCount);

    return builder.GetEdmModel();
}