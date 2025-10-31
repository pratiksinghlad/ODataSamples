using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using ODataDemo.Model;
using ODataSamples.Data.UnitOfWork;
using System.ComponentModel.DataAnnotations;

namespace ODataSamples.Endpoints;

/// <summary>
/// Extension methods for configuring OData endpoints using Minimal API
/// </summary>
public static class ODataEndpoints
{
    /// <summary>
    /// Maps OData endpoints to the application using Minimal API
    /// </summary>
    /// <param name="app">The web application</param>
    /// <returns>The web application for chaining</returns>
    public static WebApplication MapODataEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // Customer OData endpoints
        MapCustomerODataEndpoints(app);
        
        // Product OData endpoints
        MapProductODataEndpoints(app);
        
        // Order OData endpoints
        MapOrderODataEndpoints(app);

        return app;
    }

    /// <summary>
    /// Maps Customer OData endpoints
    /// </summary>
    private static void MapCustomerODataEndpoints(WebApplication app)
    {
        var group = app.MapGroup("odata/Customers")
            .WithTags("Customers")
            .WithOpenApi()
            .WithDescription("OData endpoints for Customer entities with advanced querying capabilities");

        // GET /odata/CustomersOData
        group.MapGet("/", ([FromServices] IUnitOfWork unitOfWork, [FromServices] ILogger<Program> logger) =>
        {
            try
            {
                logger.LogInformation("Processing OData query for customers");
                var customers = unitOfWork.Customers.GetAll();
                logger.LogInformation("OData customers query processed successfully");
                return Results.Ok(customers);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing OData customers query");
                return Results.Problem(
                    detail: "An error occurred while processing the customers query",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        })
            .WithName("GetCustomersOData")
            .WithSummary("Get customers with OData query support")
            .WithDescription("Supports $filter, $expand, $select, $orderby, $top, $skip, $count")
            .Produces<IQueryable<CustomerModel>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        // GET /odata/CustomersOData({key})
        group.MapGet("({key:int})", async (int key, [FromServices] IUnitOfWork unitOfWork, [FromServices] ILogger<Program> logger) =>
        {
            try
            {
                logger.LogInformation("Getting customer {CustomerId} with OData query options", key);
                
                var customer = await unitOfWork.Customers.GetByIdAsync(key);
                if (customer is null)
                {
                    logger.LogWarning("Customer {CustomerId} not found", key);
                    return Results.NotFound();
                }

                logger.LogInformation("Customer {CustomerId} retrieved", key);
                return Results.Ok(customer);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting customer {CustomerId}", key);
                return Results.Problem(
                    detail: "An error occurred while retrieving the customer",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        })
            .WithName("GetCustomerODataByKey")
            .WithSummary("Get customer by key with OData query support")
            .WithDescription("Supports $expand and $select for specific customer")
            .Produces<CustomerModel>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        // POST /odata/CustomersOData
        group.MapPost("/", async (CustomerModel customer, [FromServices] IUnitOfWork unitOfWork, [FromServices] ILogger<Program> logger) =>
        {
            try
            {
                logger.LogInformation("Creating customer via OData: {CustomerName}", customer.Name);
                
                customer.Id = 0;
                customer.CreatedAt = DateTime.UtcNow;
                customer.UpdatedAt = DateTime.UtcNow;

                var createdCustomer = await unitOfWork.Customers.AddAsync(customer);
                await unitOfWork.SaveChangesAsync();

                logger.LogInformation("Created customer {CustomerId} via OData", createdCustomer.Id);
                return Results.Created($"/odata/Customers({createdCustomer.Id})", createdCustomer);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating customer via OData");
                return Results.Problem(
                    detail: "An error occurred while creating the customer",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        })
            .WithName("CreateCustomerOData")
            .WithSummary("Create a new customer via OData")
            .WithDescription("Creates a new customer and returns the created entity")
            .Produces<CustomerModel>(StatusCodes.Status201Created)
            .Produces<ValidationProblem>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        // PUT /odata/CustomersOData({key})
        group.MapPut("({key:int})", async (int key, CustomerModel updatedCustomer, [FromServices] IUnitOfWork unitOfWork, [FromServices] ILogger<Program> logger) =>
        {
            try
            {
                logger.LogInformation("Updating customer {CustomerId} via OData", key);
                
                var existingCustomer = await unitOfWork.Customers.GetByIdAsync(key);
                if (existingCustomer is null)
                {
                    logger.LogWarning("Customer {CustomerId} not found for update", key);
                    return Results.NotFound();
                }

                existingCustomer.Name = updatedCustomer.Name;
                existingCustomer.City = updatedCustomer.City;
                existingCustomer.UpdatedAt = DateTime.UtcNow;

                unitOfWork.Customers.Update(existingCustomer);
                await unitOfWork.SaveChangesAsync();

                logger.LogInformation("Updated customer {CustomerId} via OData", key);
                return Results.Ok(existingCustomer);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating customer {CustomerId}", key);
                return Results.Problem(
                    detail: "An error occurred while updating the customer",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        })
            .WithName("UpdateCustomerOData")
            .WithSummary("Update an existing customer via OData")
            .WithDescription("Updates an existing customer and returns the updated entity")
            .Produces<CustomerModel>(StatusCodes.Status200OK)
            .Produces<ValidationProblem>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        // DELETE /odata/CustomersOData({key})
        group.MapDelete("({key:int})", async (int key, [FromServices] IUnitOfWork unitOfWork, [FromServices] ILogger<Program> logger) =>
        {
            try
            {
                logger.LogInformation("Deleting customer {CustomerId} via OData", key);
                
                var deleted = await unitOfWork.Customers.DeleteByIdAsync(key);
                if (!deleted)
                {
                    logger.LogWarning("Customer {CustomerId} not found for deletion", key);
                    return Results.NotFound();
                }

                await unitOfWork.SaveChangesAsync();
                logger.LogInformation("Deleted customer {CustomerId} via OData", key);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting customer {CustomerId}", key);
                return Results.Problem(
                    detail: "An error occurred while deleting the customer",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        })
            .WithName("DeleteCustomerOData")
            .WithSummary("Delete a customer via OData")
            .WithDescription("Deletes a customer from the database")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    /// <summary>
    /// Maps Product OData endpoints
    /// </summary>
    private static void MapProductODataEndpoints(WebApplication app)
    {
        var group = app.MapGroup("odata/Products")
            .WithTags("Products")
            .WithOpenApi()
            .WithDescription("OData endpoints for Product entities with advanced querying capabilities");

        group.MapGet("/", ([FromServices] IUnitOfWork unitOfWork, [FromServices] ILogger<Program> logger) =>
        {
            try
            {
                logger.LogInformation("Processing OData query for products");
                var products = unitOfWork.Products.GetAll();
                logger.LogInformation("OData products query processed successfully");
                return Results.Ok(products);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing OData products query");
                return Results.Problem(
                    detail: "An error occurred while processing the products query",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        })
            .WithName("GetProductsOData")
            .WithSummary("Get products with OData query support")
            .WithDescription("Supports $filter, $expand, $select, $orderby, $top, $skip, $count")
            .Produces<IQueryable<ProductModel>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        group.MapGet("({key:int})", async (int key, [FromServices] IUnitOfWork unitOfWork, [FromServices] ILogger<Program> logger) =>
        {
            try
            {
                logger.LogInformation("Getting product {ProductId}", key);
                
                var product = await unitOfWork.Products.GetByIdAsync(key);
                if (product is null)
                {
                    logger.LogWarning("Product {ProductId} not found", key);
                    return Results.NotFound();
                }

                logger.LogInformation("Product {ProductId} retrieved", key);
                return Results.Ok(product);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting product {ProductId}", key);
                return Results.Problem(
                    detail: "An error occurred while retrieving the product",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        })
            .WithName("GetProductODataByKey")
            .WithSummary("Get product by key with OData query support")
            .Produces<ProductModel>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/", async (ProductModel product, [FromServices] IUnitOfWork unitOfWork, [FromServices] ILogger<Program> logger) =>
        {
            try
            {
                logger.LogInformation("Creating product via OData: {ProductName}", product.Name);
                
                product.Id = 0;
                product.CreatedAt = DateTime.UtcNow;
                product.UpdatedAt = DateTime.UtcNow;

                var createdProduct = await unitOfWork.Products.AddAsync(product);
                await unitOfWork.SaveChangesAsync();

                logger.LogInformation("Created product {ProductId} via OData", createdProduct.Id);
                return Results.Created($"/odata/Products({createdProduct.Id})", createdProduct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating product via OData");
                return Results.Problem(
                    detail: "An error occurred while creating the product",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        })
            .WithName("CreateProductOData")
            .WithSummary("Create a new product via OData")
            .Produces<ProductModel>(StatusCodes.Status201Created)
            .Produces<ValidationProblem>(StatusCodes.Status400BadRequest);

        group.MapPut("({key:int})", async (int key, ProductModel updatedProduct, [FromServices] IUnitOfWork unitOfWork, [FromServices] ILogger<Program> logger) =>
        {
            try
            {
                logger.LogInformation("Updating product {ProductId} via OData", key);
                
                var existingProduct = await unitOfWork.Products.GetByIdAsync(key);
                if (existingProduct is null)
                {
                    logger.LogWarning("Product {ProductId} not found for update", key);
                    return Results.NotFound();
                }

                existingProduct.Name = updatedProduct.Name;
                existingProduct.Price = updatedProduct.Price;
                existingProduct.UpdatedAt = DateTime.UtcNow;

                unitOfWork.Products.Update(existingProduct);
                await unitOfWork.SaveChangesAsync();

                logger.LogInformation("Updated product {ProductId} via OData", key);
                return Results.Ok(existingProduct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating product {ProductId}", key);
                return Results.Problem(
                    detail: "An error occurred while updating the product",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        })
            .WithName("UpdateProductOData")
            .WithSummary("Update an existing product via OData")
            .Produces<ProductModel>(StatusCodes.Status200OK)
            .Produces<ValidationProblem>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapDelete("({key:int})", async (int key, [FromServices] IUnitOfWork unitOfWork, [FromServices] ILogger<Program> logger) =>
        {
            try
            {
                logger.LogInformation("Deleting product {ProductId} via OData", key);
                
                var deleted = await unitOfWork.Products.DeleteByIdAsync(key);
                if (!deleted)
                {
                    logger.LogWarning("Product {ProductId} not found for deletion", key);
                    return Results.NotFound();
                }

                await unitOfWork.SaveChangesAsync();
                logger.LogInformation("Deleted product {ProductId} via OData", key);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting product {ProductId}", key);
                return Results.Problem(
                    detail: "An error occurred while deleting the product",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        })
            .WithName("DeleteProductOData")
            .WithSummary("Delete a product via OData")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Maps Order OData endpoints
    /// </summary>
    private static void MapOrderODataEndpoints(WebApplication app)
    {
        var group = app.MapGroup("odata/Orders")
            .WithTags("Orders")
            .WithOpenApi()
            .WithDescription("OData endpoints for Order entities with advanced querying capabilities");

        group.MapGet("/", ([FromServices] IUnitOfWork unitOfWork, [FromServices] ILogger<Program> logger) =>
        {
            try
            {
                logger.LogInformation("Processing OData query for orders");
                var orders = unitOfWork.Orders.GetAll();
                logger.LogInformation("OData orders query processed successfully");
                return Results.Ok(orders);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing OData orders query");
                return Results.Problem(
                    detail: "An error occurred while processing the orders query",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        })
            .WithName("GetOrdersOData")
            .WithSummary("Get orders with OData query support")
            .WithDescription("Supports $filter, $expand, $select, $orderby, $top, $skip, $count")
            .Produces<IQueryable<OrderModel>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        group.MapGet("({key:int})", async (int key, [FromServices] IUnitOfWork unitOfWork, [FromServices] ILogger<Program> logger) =>
        {
            try
            {
                logger.LogInformation("Getting order {OrderId}", key);
                
                var order = await unitOfWork.Orders.GetByIdAsync(key);
                if (order is null)
                {
                    logger.LogWarning("Order {OrderId} not found", key);
                    return Results.NotFound();
                }

                logger.LogInformation("Order {OrderId} retrieved", key);
                return Results.Ok(order);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting order {OrderId}", key);
                return Results.Problem(
                    detail: "An error occurred while retrieving the order",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        })
            .WithName("GetOrderODataByKey")
            .WithSummary("Get order by key with OData query support")
            .Produces<OrderModel>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPost("/", async (OrderModel order, [FromServices] IUnitOfWork unitOfWork, [FromServices] ILogger<Program> logger) =>
        {
            try
            {
                logger.LogInformation("Creating order via OData for customer {CustomerId}", order.CustomerId);
                
                order.Id = 0;
                order.CreatedAt = DateTime.UtcNow;
                order.UpdatedAt = DateTime.UtcNow;

                var createdOrder = await unitOfWork.Orders.AddAsync(order);
                await unitOfWork.SaveChangesAsync();

                logger.LogInformation("Created order {OrderId} via OData", createdOrder.Id);
                return Results.Created($"/odata/Orders({createdOrder.Id})", createdOrder);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating order via OData");
                return Results.Problem(
                    detail: "An error occurred while creating the order",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        })
            .WithName("CreateOrderOData")
            .WithSummary("Create a new order via OData")
            .Produces<OrderModel>(StatusCodes.Status201Created)
            .Produces<ValidationProblem>(StatusCodes.Status400BadRequest);

        group.MapDelete("({key:int})", async (int key, [FromServices] IUnitOfWork unitOfWork, [FromServices] ILogger<Program> logger) =>
        {
            try
            {
                logger.LogInformation("Deleting order {OrderId} via OData", key);
                
                var deleted = await unitOfWork.Orders.DeleteByIdAsync(key);
                if (!deleted)
                {
                    logger.LogWarning("Order {OrderId} not found for deletion", key);
                    return Results.NotFound();
                }

                await unitOfWork.SaveChangesAsync();
                logger.LogInformation("Deleted order {OrderId} via OData", key);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting order {OrderId}", key);
                return Results.Problem(
                    detail: "An error occurred while deleting the order",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        })
            .WithName("DeleteOrderOData")
            .WithSummary("Delete an order via OData")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }
}