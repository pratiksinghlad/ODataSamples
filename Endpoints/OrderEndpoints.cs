using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ODataDemo.Model;
using ODataSamples.Data.UnitOfWork;
using System.ComponentModel.DataAnnotations;

namespace ODataSamples.Endpoints;

/// <summary>
/// Extension methods for configuring Order API endpoints
/// </summary>
public static class OrderEndpoints
{
    /// <summary>
    /// Maps Order API endpoints to the application
    /// </summary>
    /// <param name="app">The web application</param>
    /// <returns>The web application for chaining</returns>
    public static WebApplication MapOrderEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app.MapGroup("api/orders")
            .WithTags("Orders")
            .WithOpenApi()
            .WithDescription("Order management endpoints");

        group.MapGet("/", GetAllOrdersAsync)
            .WithName("GetAllOrders")
            .WithSummary("Get all orders")
            .WithDescription("Retrieves all orders with full details from the database")
            .Produces<IEnumerable<OrderModel>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        group.MapGet("/{id:int}", GetOrderByIdAsync)
            .WithName("GetOrderById")
            .WithSummary("Get order by ID")
            .WithDescription("Retrieves a specific order by its ID with full details")
            .Produces<OrderModel>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        group.MapGet("/recent", GetRecentOrdersAsync)
            .WithName("GetRecentOrders")
            .WithSummary("Get recent orders")
            .WithDescription("Retrieves orders from the last specified number of days")
            .Produces<IEnumerable<OrderModel>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        group.MapPost("/", CreateOrderAsync)
            .WithName("CreateOrder")
            .WithSummary("Create a new order")
            .WithDescription("Creates a new order in the database")
            .Produces<OrderModel>(StatusCodes.Status201Created)
            .Produces<ValidationProblem>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        group.MapDelete("/{id:int}", DeleteOrderAsync)
            .WithName("DeleteOrder")
            .WithSummary("Delete an order")
            .WithDescription("Deletes an order from the database")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        return app;
    }

    private static async Task<Results<Ok<IEnumerable<OrderModel>>, ProblemHttpResult>> GetAllOrdersAsync(
        IUnitOfWork unitOfWork,
        ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("Retrieving all orders with full details");
            var orders = await unitOfWork.Orders.GetWithFullDetails().ToListAsync();
            logger.LogInformation("Retrieved {Count} orders", orders.Count);
            
            return TypedResults.Ok<IEnumerable<OrderModel>>(orders);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving orders");
            return TypedResults.Problem(
                detail: "An error occurred while retrieving orders",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<Results<Ok<OrderModel>, NotFound, ProblemHttpResult>> GetOrderByIdAsync(
        [Range(1, int.MaxValue)] int id,
        IUnitOfWork unitOfWork,
        ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("Retrieving order with ID {OrderId}", id);
            var order = await unitOfWork.Orders.GetByIdAsync(id, o => o.Customer!, o => o.OrderItems!);
            
            if (order is null)
            {
                logger.LogWarning("Order with ID {OrderId} not found", id);
                return TypedResults.NotFound();
            }

            logger.LogInformation("Retrieved order with ID {OrderId} for customer {CustomerName}", 
                order.Id, order.Customer?.Name ?? "Unknown");
            return TypedResults.Ok(order);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving order with ID {OrderId}", id);
            return TypedResults.Problem(
                detail: "An error occurred while retrieving the order",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<Results<Ok<IEnumerable<OrderModel>>, ProblemHttpResult>> GetRecentOrdersAsync(
        [Range(1, 365)] int days,
        IUnitOfWork unitOfWork,
        ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("Retrieving orders from the last {Days} days", days);
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            
            var orders = await unitOfWork.Orders
                .GetWhere(o => o.OrderDate >= cutoffDate)
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            
            logger.LogInformation("Retrieved {Count} recent orders", orders.Count);
            return TypedResults.Ok<IEnumerable<OrderModel>>(orders);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving recent orders from the last {Days} days", days);
            return TypedResults.Problem(
                detail: "An error occurred while retrieving recent orders",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<Results<Created<OrderModel>, ValidationProblem, ProblemHttpResult>> CreateOrderAsync(
        CreateOrderRequest request,
        IUnitOfWork unitOfWork,
        ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("Creating new order for customer {CustomerId}", request.CustomerId);

            // Verify customer exists
            var customer = await unitOfWork.Customers.GetByIdAsync(request.CustomerId);
            if (customer is null)
            {
                logger.LogWarning("Customer with ID {CustomerId} not found", request.CustomerId);
                return TypedResults.Problem(
                    detail: $"Customer with ID {request.CustomerId} not found",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            var order = new OrderModel
            {
                CustomerId = request.CustomerId,
                OrderDate = request.OrderDate ?? DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdOrder = await unitOfWork.Orders.AddAsync(order);
            await unitOfWork.SaveChangesAsync();

            // Load the order with navigation properties for the response
            var orderWithDetails = await unitOfWork.Orders.GetByIdAsync(
                createdOrder.Id, 
                o => o.Customer!, 
                o => o.OrderItems!);

            logger.LogInformation("Created order with ID {OrderId} for customer {CustomerName}", 
                createdOrder.Id, customer.Name);

            return TypedResults.Created($"/api/orders/{createdOrder.Id}", orderWithDetails!);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating order for customer {CustomerId}", request.CustomerId);
            return TypedResults.Problem(
                detail: "An error occurred while creating the order",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<Results<NoContent, NotFound, ProblemHttpResult>> DeleteOrderAsync(
        [Range(1, int.MaxValue)] int id,
        IUnitOfWork unitOfWork,
        ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("Deleting order with ID {OrderId}", id);

            var deleted = await unitOfWork.Orders.DeleteByIdAsync(id);
            if (!deleted)
            {
                logger.LogWarning("Order with ID {OrderId} not found for deletion", id);
                return TypedResults.NotFound();
            }

            await unitOfWork.SaveChangesAsync();
            logger.LogInformation("Deleted order with ID {OrderId}", id);

            return TypedResults.NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting order with ID {OrderId}", id);
            return TypedResults.Problem(
                detail: "An error occurred while deleting the order",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}

/// <summary>
/// Request model for creating a new order
/// </summary>
/// <param name="CustomerId">The customer ID</param>
/// <param name="OrderDate">The order date (optional, defaults to current UTC time)</param>
public sealed record CreateOrderRequest(
    [Required, Range(1, int.MaxValue)] int CustomerId,
    DateTime? OrderDate = null);