using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ODataDemo.Model;
using ODataSamples.Data.UnitOfWork;
using System.ComponentModel.DataAnnotations;

namespace ODataSamples.Endpoints;

/// <summary>
/// Extension methods for configuring Product API endpoints
/// </summary>
public static class ProductEndpoints
{
    /// <summary>
    /// Maps Product API endpoints to the application
    /// </summary>
    /// <param name="app">The web application</param>
    /// <returns>The web application for chaining</returns>
    public static WebApplication MapProductEndpoints(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app.MapGroup("api/products")
            .WithTags("Products")
            .WithOpenApi()
            .WithDescription("Product management endpoints");

        group.MapGet("/", GetAllProductsAsync)
            .WithName("GetAllProducts")
            .WithSummary("Get all products")
            .WithDescription("Retrieves all products from the database")
            .Produces<IEnumerable<ProductModel>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        group.MapGet("/{id:int}", GetProductByIdAsync)
            .WithName("GetProductById")
            .WithSummary("Get product by ID")
            .WithDescription("Retrieves a specific product by its ID")
            .Produces<ProductModel>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        group.MapPost("/", CreateProductAsync)
            .WithName("CreateProduct")
            .WithSummary("Create a new product")
            .WithDescription("Creates a new product in the database")
            .Produces<ProductModel>(StatusCodes.Status201Created)
            .Produces<ValidationProblem>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        group.MapPut("/{id:int}", UpdateProductAsync)
            .WithName("UpdateProduct")
            .WithSummary("Update an existing product")
            .WithDescription("Updates an existing product in the database")
            .Produces<ProductModel>(StatusCodes.Status200OK)
            .Produces<ValidationProblem>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        group.MapDelete("/{id:int}", DeleteProductAsync)
            .WithName("DeleteProduct")
            .WithSummary("Delete a product")
            .WithDescription("Deletes a product from the database")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        return app;
    }

    /// <summary>
    /// Gets all products from the database
    /// </summary>
    /// <param name="unitOfWork">The unit of work instance</param>
    /// <param name="logger">The logger instance</param>
    /// <returns>A collection of products</returns>
    private static async Task<Results<Ok<IEnumerable<ProductModel>>, ProblemHttpResult>> GetAllProductsAsync(
        IUnitOfWork unitOfWork,
        ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("Retrieving all products");
            var products = await unitOfWork.Products.GetAll().ToListAsync();
            logger.LogInformation("Retrieved {Count} products", products.Count);
            
            return TypedResults.Ok<IEnumerable<ProductModel>>(products);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving products");
            return TypedResults.Problem(
                detail: "An error occurred while retrieving products",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets a specific product by ID
    /// </summary>
    /// <param name="id">The product ID</param>
    /// <param name="unitOfWork">The unit of work instance</param>
    /// <param name="logger">The logger instance</param>
    /// <returns>The product if found, otherwise NotFound</returns>
    private static async Task<Results<Ok<ProductModel>, NotFound, ProblemHttpResult>> GetProductByIdAsync(
        [Range(1, int.MaxValue)] int id,
        IUnitOfWork unitOfWork,
        ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("Retrieving product with ID {ProductId}", id);
            var product = await unitOfWork.Products.GetByIdAsync(id);
            
            if (product is null)
            {
                logger.LogWarning("Product with ID {ProductId} not found", id);
                return TypedResults.NotFound();
            }

            logger.LogInformation("Retrieved product: {ProductName}", product.Name);
            return TypedResults.Ok(product);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving product with ID {ProductId}", id);
            return TypedResults.Problem(
                detail: "An error occurred while retrieving the product",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Creates a new product
    /// </summary>
    /// <param name="request">The product creation request</param>
    /// <param name="unitOfWork">The unit of work instance</param>
    /// <param name="logger">The logger instance</param>
    /// <returns>The created product</returns>
    private static async Task<Results<Created<ProductModel>, ValidationProblem, ProblemHttpResult>> CreateProductAsync(
        CreateProductRequest request,
        IUnitOfWork unitOfWork,
        ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("Creating new product: {ProductName}", request.Name);

            var product = new ProductModel
            {
                Name = request.Name,
                Price = request.Price,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdProduct = await unitOfWork.Products.AddAsync(product);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Created product with ID {ProductId}: {ProductName}", 
                createdProduct.Id, createdProduct.Name);

            return TypedResults.Created($"/api/products/{createdProduct.Id}", createdProduct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating product: {ProductName}", request.Name);
            return TypedResults.Problem(
                detail: "An error occurred while creating the product",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Updates an existing product
    /// </summary>
    /// <param name="id">The product ID</param>
    /// <param name="request">The product update request</param>
    /// <param name="unitOfWork">The unit of work instance</param>
    /// <param name="logger">The logger instance</param>
    /// <returns>The updated product</returns>
    private static async Task<Results<Ok<ProductModel>, NotFound, ValidationProblem, ProblemHttpResult>> UpdateProductAsync(
        [Range(1, int.MaxValue)] int id,
        UpdateProductRequest request,
        IUnitOfWork unitOfWork,
        ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("Updating product with ID {ProductId}", id);

            var existingProduct = await unitOfWork.Products.GetByIdAsync(id);
            if (existingProduct is null)
            {
                logger.LogWarning("Product with ID {ProductId} not found for update", id);
                return TypedResults.NotFound();
            }

            existingProduct.Name = request.Name;
            existingProduct.Price = request.Price;
            existingProduct.UpdatedAt = DateTime.UtcNow;

            unitOfWork.Products.Update(existingProduct);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Updated product with ID {ProductId}: {ProductName}", 
                existingProduct.Id, existingProduct.Name);

            return TypedResults.Ok(existingProduct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating product with ID {ProductId}", id);
            return TypedResults.Problem(
                detail: "An error occurred while updating the product",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Deletes a product
    /// </summary>
    /// <param name="id">The product ID</param>
    /// <param name="unitOfWork">The unit of work instance</param>
    /// <param name="logger">The logger instance</param>
    /// <returns>NoContent if successful, NotFound if product doesn't exist</returns>
    private static async Task<Results<NoContent, NotFound, ProblemHttpResult>> DeleteProductAsync(
        [Range(1, int.MaxValue)] int id,
        IUnitOfWork unitOfWork,
        ILogger<Program> logger)
    {
        try
        {
            logger.LogInformation("Deleting product with ID {ProductId}", id);

            var deleted = await unitOfWork.Products.DeleteByIdAsync(id);
            if (!deleted)
            {
                logger.LogWarning("Product with ID {ProductId} not found for deletion", id);
                return TypedResults.NotFound();
            }

            await unitOfWork.SaveChangesAsync();
            logger.LogInformation("Deleted product with ID {ProductId}", id);

            return TypedResults.NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting product with ID {ProductId}", id);
            return TypedResults.Problem(
                detail: "An error occurred while deleting the product",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}

/// <summary>
/// Request model for creating a new product
/// </summary>
/// <param name="Name">The product name</param>
/// <param name="Price">The product price</param>
public sealed record CreateProductRequest(
    [Required, StringLength(255, MinimumLength = 1)] string Name,
    [Required, Range(0.01, double.MaxValue)] decimal Price);

/// <summary>
/// Request model for updating an existing product
/// </summary>
/// <param name="Name">The product name</param>
/// <param name="Price">The product price</param>
public sealed record UpdateProductRequest(
    [Required, StringLength(255, MinimumLength = 1)] string Name,
    [Required, Range(0.01, double.MaxValue)] decimal Price);