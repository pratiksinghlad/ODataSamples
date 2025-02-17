using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace ODataDemo.Controllers;

[ApiController]
[Route("odata")]
public class OrdersODataController : ODataController
{
    // GET /odata/Orders
    // Supports expansions, filters, sorting, etc.
    [EnableQuery]
    [HttpGet]
    public IActionResult Get()
    {
        // Return IQueryable so OData can apply $filter, $expand, $orderby, etc.
        return Ok(InMemoryData.Orders.AsQueryable());
    }

    // GET /odata/Orders(101)
    // e.g. /odata/Orders(101)?$expand=Customer,OrderItems
    [EnableQuery]
    [HttpGet("{key}")]
    public IActionResult Get([FromRoute] int key)
    {
        var order = InMemoryData.Orders.FirstOrDefault(o => o.Id == key);
        return order == null ? NotFound() : Ok(order);
    }
}