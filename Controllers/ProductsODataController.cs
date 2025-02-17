using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using ODataDemo.Model;

namespace ODataDemo.Controllers;

[ApiController]
[Route("odata")]
public class ProductsODataController : ODataController
{
    // In-memory sample data
    private static readonly List<ProductModel> _products = InMemoryData.Products;

    // GET odata/Products?$filter=Price ge 300 and Price le 1000&$orderby=Name
    [EnableQuery]
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_products.AsQueryable());
    }

    [HttpPost]
    public IActionResult Post([FromBody] ProductModel product)
    {
        // In a real application, you'd generate the Id and persist the entity.
        product.Id = _products.Max(p => p.Id) + 1;
        _products.Add(product);

        return Ok(product);
    }

    [HttpPut("{key}")]
    public IActionResult Put([FromRoute] int key, [FromBody] ProductModel updatedProduct)
    {
        var existingProduct = _products.FirstOrDefault(p => p.Id == key);
        if (existingProduct == null)
        {
            return NotFound();
        }

        // Update properties - in real applications, more sophisticated handling might be required.
        existingProduct.Name = updatedProduct.Name;
        existingProduct.Price = updatedProduct.Price;

        return Updated(existingProduct);
    }
}