using Microsoft.AspNetCore.Mvc;
using ODataDemo.Model;
using ODataSamples.Data;

namespace ODataDemo.Controllers;

/// <summary>
/// Traditional REST API controller for Products (non-OData)
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    /// <summary>
    /// Gets all products
    /// </summary>
    /// <returns>Collection of all products</returns>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(InMemoryData.Products);
    }

    /// <summary>
    /// Creates a new product
    /// </summary>
    /// <param name="product">The product to create</param>
    /// <returns>The created product</returns>
    [HttpPost]
    public IActionResult Post([FromBody] ProductModel product)
    {
        ArgumentNullException.ThrowIfNull(product);
        
        // Generate new ID and add to collection
        product.Id = InMemoryData.Products.Count > 0 ? InMemoryData.Products.Max(p => p.Id) + 1 : 1;
        InMemoryData.Products.Add(product);

        return Created($"/api/Products/{product.Id}", product);
    }

    /// <summary>
    /// Updates an existing product
    /// </summary>
    /// <param name="key">The product ID</param>
    /// <param name="updatedProduct">The updated product data</param>
    /// <returns>The updated product or NotFound if not exists</returns>
    [HttpPut("{key}")]
    public IActionResult Put([FromRoute] int key, [FromBody] ProductModel updatedProduct)
    {
        ArgumentNullException.ThrowIfNull(updatedProduct);
        
        var existingProduct = InMemoryData.Products.FirstOrDefault(p => p.Id == key);
        if (existingProduct is null)
        {
            return NotFound();
        }

        // Update properties using modern C# patterns
        existingProduct.Name = updatedProduct.Name;
        existingProduct.Price = updatedProduct.Price;

        return Ok(existingProduct);
    }
}