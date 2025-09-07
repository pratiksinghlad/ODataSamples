using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using ODataDemo.Model;
using ODataSamples.Data;

namespace ODataDemo.Controllers;

/// <summary>
/// OData controller for Products with full query capabilities
/// </summary>
public class ProductsODataController : ODataController
{
    /// <summary>
    /// Gets all products with OData query support
    /// </summary>
    /// <returns>Queryable collection of products</returns>
    /// <remarks>
    /// Supports OData queries like:
    /// - GET odata/ProductsOData?$filter=Price ge 300 and Price le 1000&$orderby=Name
    /// - GET odata/ProductsOData?$select=Name,Price&$top=10
    /// </remarks>
    [EnableQuery]
    public IActionResult Get()
    {
        return Ok(InMemoryData.Products.AsQueryable());
    }

    /// <summary>
    /// Creates a new product
    /// </summary>
    /// <param name="product">The product to create</param>
    /// <returns>The created product</returns>
    public IActionResult Post([FromBody] ProductModel product)
    {
        ArgumentNullException.ThrowIfNull(product);
        
        // Generate new ID and add to collection
        product.Id = InMemoryData.Products.Count > 0 ? InMemoryData.Products.Max(p => p.Id) + 1 : 1;
        InMemoryData.Products.Add(product);

        return Ok(product);
    }

    /// <summary>
    /// Updates an existing product
    /// </summary>
    /// <param name="key">The product ID</param>
    /// <param name="updatedProduct">The updated product data</param>
    /// <returns>The updated product or NotFound if not exists</returns>
    public IActionResult Put(int key, [FromBody] ProductModel updatedProduct)
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

        return Updated(existingProduct);
    }
}