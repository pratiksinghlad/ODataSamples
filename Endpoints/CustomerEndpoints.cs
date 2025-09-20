using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ODataDemo.Model;
using ODataSamples.Data.UnitOfWork;
using System.ComponentModel.DataAnnotations;

namespace ODataSamples.Endpoints;

/// <summary>
/// Extension methods for configuring Customer API endpoints
/// </summary>
public static class CustomerEndpoints
{
    /// <summary>
    /// Maps Customer API endpoints to the application
    /// </summary>
    /// <param name="app">The web application</param>
    /// <returns>The web application for chaining</returns>
    public static WebApplication MapCustomerEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app.MapGroup("api/customers")
            .WithTags("Customers")
            .WithOpenApi()
            .WithDescription("Customer management endpoints");

        group.MapGet("/", GetAllCustomersAsync)
            .WithName("GetAllCustomers")
            .WithSummary("Get all customers")
            .WithDescription("Retrieves all customers from the database")
            .Produces<IEnumerable<CustomerModel>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        group.MapGet("/{id:int}", GetCustomerByIdAsync)
            .WithName("GetCustomerById")
            .WithSummary("Get customer by ID")
            .WithDescription("Retrieves a specific customer by their ID")
            .Produces<CustomerModel>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        group.MapGet("/{id:int}/orders", GetCustomerOrdersAsync)
            .WithName("GetCustomerOrders")
            .WithSummary("Get customer orders")
            .WithDescription("Retrieves all orders for a specific customer")
            .Produces<IEnumerable<OrderModel>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        group.MapPost("/", CreateCustomerAsync)
            .WithName("CreateCustomer")
            .WithSummary("Create a new customer")
            .WithDescription("Creates a new customer in the database")
            .Produces<CustomerModel>(StatusCodes.Status201Created)
            .Produces<ValidationProblem>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        group.MapPut("/{id:int}", UpdateCustomerAsync)
            .WithName("UpdateCustomer")
            .WithSummary("Update an existing customer")
            .WithDescription("Updates an existing customer in the database")
            .Produces<CustomerModel>(StatusCodes.Status200OK)
            .Produces<ValidationProblem>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        group.MapDelete("/{id:int}", DeleteCustomerAsync)
            .WithName("DeleteCustomer")
            .WithSummary("Delete a customer")
            .WithDescription("Deletes a customer from the database")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        return app;
    }

    private static async Task<Results<Ok<IEnumerable<CustomerModel>>, ProblemHttpResult>> GetAllCustomersAsync(
        IUnitOfWork unitOfWork,
        ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("Retrieving all customers");
            var customers = await unitOfWork.Customers.GetAll().ToListAsync();
            logger.LogInformation("Retrieved {Count} customers", customers.Count);
            
            return TypedResults.Ok<IEnumerable<CustomerModel>>(customers);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving customers");
            return TypedResults.Problem(
                detail: "An error occurred while retrieving customers",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<Results<Ok<CustomerModel>, NotFound, ProblemHttpResult>> GetCustomerByIdAsync(
        [Range(1, int.MaxValue)] int id,
        IUnitOfWork unitOfWork,
        ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("Retrieving customer with ID {CustomerId}", id);
            var customer = await unitOfWork.Customers.GetByIdAsync(id);
            
            if (customer is null)
            {
                logger.LogWarning("Customer with ID {CustomerId} not found", id);
                return TypedResults.NotFound();
            }

            logger.LogInformation("Retrieved customer: {CustomerName}", customer.Name);
            return TypedResults.Ok(customer);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving customer with ID {CustomerId}", id);
            return TypedResults.Problem(
                detail: "An error occurred while retrieving the customer",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<Results<Ok<IEnumerable<OrderModel>>, NotFound, ProblemHttpResult>> GetCustomerOrdersAsync(
        [Range(1, int.MaxValue)] int id,
        IUnitOfWork unitOfWork,
        ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("Retrieving orders for customer with ID {CustomerId}", id);
            
            // First check if customer exists
            var customer = await unitOfWork.Customers.GetByIdAsync(id);
            if (customer is null)
            {
                logger.LogWarning("Customer with ID {CustomerId} not found", id);
                return TypedResults.NotFound();
            }

            var orders = await unitOfWork.Orders.GetWhere(o => o.CustomerId == id).ToListAsync();
            logger.LogInformation("Retrieved {Count} orders for customer {CustomerId}", orders.Count, id);
            
            return TypedResults.Ok<IEnumerable<OrderModel>>(orders);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving orders for customer with ID {CustomerId}", id);
            return TypedResults.Problem(
                detail: "An error occurred while retrieving customer orders",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<Results<Created<CustomerModel>, ValidationProblem, ProblemHttpResult>> CreateCustomerAsync(
        CreateCustomerRequest request,
        IUnitOfWork unitOfWork,
        ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("Creating new customer: {CustomerName}", request.Name);

            var customer = new CustomerModel
            {
                Name = request.Name,
                City = request.City,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdCustomer = await unitOfWork.Customers.AddAsync(customer);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Created customer with ID {CustomerId}: {CustomerName}", 
                createdCustomer.Id, createdCustomer.Name);

            return TypedResults.Created($"/api/customers/{createdCustomer.Id}", createdCustomer);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating customer: {CustomerName}", request.Name);
            return TypedResults.Problem(
                detail: "An error occurred while creating the customer",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<Results<Ok<CustomerModel>, NotFound, ValidationProblem, ProblemHttpResult>> UpdateCustomerAsync(
        [Range(1, int.MaxValue)] int id,
        UpdateCustomerRequest request,
        IUnitOfWork unitOfWork,
        ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("Updating customer with ID {CustomerId}", id);

            var existingCustomer = await unitOfWork.Customers.GetByIdAsync(id);
            if (existingCustomer is null)
            {
                logger.LogWarning("Customer with ID {CustomerId} not found for update", id);
                return TypedResults.NotFound();
            }

            existingCustomer.Name = request.Name;
            existingCustomer.City = request.City;
            existingCustomer.UpdatedAt = DateTime.UtcNow;

            unitOfWork.Customers.Update(existingCustomer);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Updated customer with ID {CustomerId}: {CustomerName}", 
                existingCustomer.Id, existingCustomer.Name);

            return TypedResults.Ok(existingCustomer);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating customer with ID {CustomerId}", id);
            return TypedResults.Problem(
                detail: "An error occurred while updating the customer",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<Results<NoContent, NotFound, ProblemHttpResult>> DeleteCustomerAsync(
        [Range(1, int.MaxValue)] int id,
        IUnitOfWork unitOfWork,
        ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("Deleting customer with ID {CustomerId}", id);

            var deleted = await unitOfWork.Customers.DeleteByIdAsync(id);
            if (!deleted)
            {
                logger.LogWarning("Customer with ID {CustomerId} not found for deletion", id);
                return TypedResults.NotFound();
            }

            await unitOfWork.SaveChangesAsync();
            logger.LogInformation("Deleted customer with ID {CustomerId}", id);

            return TypedResults.NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting customer with ID {CustomerId}", id);
            return TypedResults.Problem(
                detail: "An error occurred while deleting the customer",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}

/// <summary>
/// Request model for creating a new customer
/// </summary>
/// <param name="Name">The customer name</param>
/// <param name="City">The customer city</param>
public sealed record CreateCustomerRequest(
    [Required, StringLength(255, MinimumLength = 1)] string Name,
    [Required, StringLength(100, MinimumLength = 1)] string City);

/// <summary>
/// Request model for updating an existing customer
/// </summary>
/// <param name="Name">The customer name</param>
/// <param name="City">The customer city</param>
public sealed record UpdateCustomerRequest(
    [Required, StringLength(255, MinimumLength = 1)] string Name,
    [Required, StringLength(100, MinimumLength = 1)] string City);