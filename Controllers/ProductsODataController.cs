using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using ODataDemo.Model;
using ODataSamples.Data.UnitOfWork;

namespace ODataDemo.Controllers;

/// <summary>
/// OData controller for Products with full query capabilities using Repository pattern
/// </summary>
public class ProductsODataController : ODataController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductsODataController> _logger;

    /// <summary>
    /// Initializes a new instance of the ProductsODataController
    /// </summary>
    /// <param name="unitOfWork">Unit of work for data access</param>
    /// <param name="logger">Logger for error logging</param>
    public ProductsODataController(IUnitOfWork unitOfWork, ILogger<ProductsODataController> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all products with OData query support
    /// </summary>
    /// <returns>Queryable collection of products</returns>
    /// <remarks>
    /// Supports OData queries like:
    /// - GET odata/ProductsOData?$filter=Price ge 300 and Price le 1000&$orderby=Name
    /// - GET odata/ProductsOData?$select=Name,Price&$top=10
    /// - GET odata/ProductsOData?$filter=contains(Name,'laptop')&$orderby=Price desc
    /// </remarks>
    [EnableQuery(MaxTop = 1000, MaxOrderByNodeCount = 10)]
    public IActionResult Get()
    {
        try
        {
            var products = _unitOfWork.Products.GetAll();
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving products.");
            return StatusCode(500, new { error = "An error occurred while retrieving products" });
        }
    }

    /// <summary>
    /// Gets a specific product by key with OData query support
    /// </summary>
    /// <param name="key">The product ID</param>
    /// <returns>The product if found, otherwise NotFound</returns>
    /// <remarks>
    /// Example: GET /odata/ProductsOData(1)?$select=Name,Price
    /// </remarks>
    [EnableQuery]
    public async Task<IActionResult> Get(int key)
    {
        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(key);
            return product is not null ? Ok(product) : NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving the product.");
            return StatusCode(500, new { error = "An error occurred while retrieving the product" });
        }
    }

    /// <summary>
    /// Creates a new product
    /// </summary>
    /// <param name="product">The product to create</param>
    /// <returns>The created product</returns>
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ProductModel product)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Reset ID and timestamps as they are auto-generated
            product.Id = 0;
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            var createdProduct = await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return Created($"/odata/ProductsOData({createdProduct.Id})", createdProduct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating the product.");
            return StatusCode(500, new { error = "An error occurred while creating the product" });
        }
    }

    /// <summary>
    /// Updates an existing product
    /// </summary>
    /// <param name="key">The product ID</param>
    /// <param name="updatedProduct">The updated product data</param>
    /// <returns>The updated product or NotFound if not exists</returns>
    [HttpPut]
    public async Task<IActionResult> Put(int key, [FromBody] ProductModel updatedProduct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var existingProduct = await _unitOfWork.Products.GetByIdAsync(key);
            if (existingProduct is null)
            {
                return NotFound();
            }

            // Update properties while preserving audit fields
            existingProduct.Name = updatedProduct.Name;
            existingProduct.Price = updatedProduct.Price;
            existingProduct.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Products.Update(existingProduct);
            await _unitOfWork.SaveChangesAsync();

            return Updated(existingProduct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating the product.");
            return StatusCode(500, new { error = "An error occurred while updating the product" });
        }
    }

    /// <summary>
    /// Deletes a product
    /// </summary>
    /// <param name="key">The product ID</param>
    /// <returns>NoContent if successful, NotFound if not exists</returns>
    [HttpDelete]
    public async Task<IActionResult> Delete(int key)
    {
        try
        {
            var deleted = await _unitOfWork.Products.DeleteByIdAsync(key);
            if (!deleted)
            {
                return NotFound();
            }

            await _unitOfWork.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting the product.");
            return StatusCode(500, new { error = "An error occurred while deleting the product" });
        }
    }

    /// <summary>
    /// Gets products by price range (custom OData function)
    /// </summary>
    /// <param name="minPrice">Minimum price</param>
    /// <param name="maxPrice">Maximum price</param>
    /// <returns>Products in the specified price range</returns>
    [HttpGet("GetByPriceRange")]
    [EnableQuery]
    public IActionResult GetByPriceRange([FromQuery] decimal minPrice, [FromQuery] decimal maxPrice)
    {
        try
        {
            var products = _unitOfWork.Products.GetByPriceRange(minPrice, maxPrice);
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving products by price range.");
            return StatusCode(500, new { error = "An error occurred while retrieving products by price range" });
        }
    }
}