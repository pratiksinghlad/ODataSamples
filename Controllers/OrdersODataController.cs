using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using ODataSamples.Data;

namespace ODataDemo.Controllers;

/// <summary>
/// OData controller for Orders with full query capabilities including expansions
/// </summary>
public class OrdersODataController : ODataController
{
    /// <summary>
    /// Gets all orders with OData query support
    /// </summary>
    /// <returns>Queryable collection of orders</returns>
    /// <remarks>
    /// Supports OData queries like:
    /// - GET /odata/OrdersOData?$expand=Customer,OrderItems
    /// - GET /odata/OrdersOData?$filter=OrderDate gt 2025-01-01T00:00:00Z&$orderby=OrderDate desc
    /// </remarks>
    [EnableQuery]
    public IActionResult Get()
    {
        return Ok(InMemoryData.Orders.AsQueryable());
    }

    /// <summary>
    /// Gets a specific order by key with OData query support
    /// </summary>
    /// <param name="key">The order ID</param>
    /// <returns>The order if found, otherwise NotFound</returns>
    /// <remarks>
    /// Example: GET /odata/OrdersOData(101)?$expand=Customer,OrderItems
    /// </remarks>
    [EnableQuery]
    public IActionResult Get(int key)
    {
        var order = InMemoryData.Orders.FirstOrDefault(o => o.Id == key);
        return order is not null ? Ok(order) : NotFound();
    }
}