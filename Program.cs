using Microsoft.AspNetCore.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using ODataDemo.Model;
using ODataSamples.Data;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddOData(options => options
        .Select()
        .Filter()
        .OrderBy()
        .Count()
        .Expand()
        .SetMaxTop(100)
        .AddRouteComponents("odata", GetEdmModel()));

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapScalarApiReference();
app.UseAuthorization();
app.UseCors("AllowAll");

// OData Controllers (keeping existing functionality)
app.MapControllers();

// Minimal API endpoints for Products (non-OData)
var productsApi = app.MapGroup("api/products")
    .WithTags("Products")
    .WithOpenApi();

productsApi.MapGet("/", () => Results.Ok(InMemoryData.Products))
    .WithName("GetAllProducts")
    .WithSummary("Get all products");

productsApi.MapGet("/{id:int}", (int id) =>
{
    var product = InMemoryData.Products.FirstOrDefault(p => p.Id == id);
    return product is not null ? Results.Ok(product) : Results.NotFound();
})
    .WithName("GetProductById")
    .WithSummary("Get product by ID");

productsApi.MapPost("/", (ProductModel product) =>
{
    product.Id = InMemoryData.Products.Max(p => p.Id) + 1;
    InMemoryData.Products.Add(product);
    return Results.Created($"/api/products/{product.Id}", product);
})
    .WithName("CreateProduct")
    .WithSummary("Create a new product");

productsApi.MapPut("/{id:int}", (int id, ProductModel updatedProduct) =>
{
    var existingProduct = InMemoryData.Products.FirstOrDefault(p => p.Id == id);
    if (existingProduct is null)
        return Results.NotFound();
        
    existingProduct.Name = updatedProduct.Name;
    existingProduct.Price = updatedProduct.Price;
    return Results.Ok(existingProduct);
})
    .WithName("UpdateProduct")
    .WithSummary("Update an existing product");

productsApi.MapDelete("/{id:int}", (int id) =>
{
    var product = InMemoryData.Products.FirstOrDefault(p => p.Id == id);
    if (product is null)
        return Results.NotFound();
        
    InMemoryData.Products.Remove(product);
    return Results.NoContent();
})
    .WithName("DeleteProduct")
    .WithSummary("Delete a product");

// Minimal API endpoints for Orders (non-OData)
var ordersApi = app.MapGroup("api/orders")
    .WithTags("Orders")
    .WithOpenApi();

ordersApi.MapGet("/", () => Results.Ok(InMemoryData.Orders))
    .WithName("GetAllOrders")
    .WithSummary("Get all orders");

ordersApi.MapGet("/{id:int}", (int id) =>
{
    var order = InMemoryData.Orders.FirstOrDefault(o => o.Id == id);
    return order is not null ? Results.Ok(order) : Results.NotFound();
})
    .WithName("GetOrderById")
    .WithSummary("Get order by ID");

// Minimal API endpoints for Customers (non-OData)
var customersApi = app.MapGroup("api/customers")
    .WithTags("Customers")
    .WithOpenApi();

customersApi.MapGet("/", () => Results.Ok(InMemoryData.Customers))
    .WithName("GetAllCustomers")
    .WithSummary("Get all customers");

customersApi.MapGet("/{id:int}", (int id) =>
{
    var customer = InMemoryData.Customers.FirstOrDefault(c => c.Id == id);
    return customer is not null ? Results.Ok(customer) : Results.NotFound();
})
    .WithName("GetCustomerById")
    .WithSummary("Get customer by ID");

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

    return builder.GetEdmModel();
}