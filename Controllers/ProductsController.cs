using Microsoft.AspNetCore.Mvc;
using ODataDemo.Model;

namespace ODataDemo.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private static readonly List<ProductModel> _products = InMemoryData.Products;

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_products);
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

        return Ok(existingProduct);
    }
}