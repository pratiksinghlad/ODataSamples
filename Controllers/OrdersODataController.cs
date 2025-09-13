using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using ODataDemo.Model;
using ODataSamples.Data.UnitOfWork;

namespace ODataDemo.Controllers;

/// <summary>
/// OData controller for Orders with full query capabilities including expansions using Repository pattern
/// </summary>
public class OrdersODataController : ODataController
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the OrdersODataController
    /// </summary>
    /// <param name="unitOfWork">Unit of work for data access</param>
    public OrdersODataController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Gets all orders with OData query support
    /// </summary>
    /// <returns>Queryable collection of orders</returns>
    /// <remarks>
    /// Supports OData queries like:
    /// - GET /odata/OrdersOData?$expand=Customer,OrderItems
    /// - GET /odata/OrdersOData?$filter=OrderDate gt 2025-01-01T00:00:00Z&$orderby=OrderDate desc
    /// - GET /odata/OrdersOData?$expand=Customer($select=Name,City)&$select=Id,OrderDate,CustomerId
    /// - GET /odata/OrdersOData?$filter=Customer/City eq 'Seattle'&$expand=Customer,OrderItems
    /// </remarks>
    [EnableQuery(MaxExpansionDepth = 3, MaxTop = 1000, MaxOrderByNodeCount = 10)]
    public IActionResult Get()
    {
        try
        {
            var orders = _unitOfWork.Orders.GetAll();
            return Ok(orders);
        }
        catch (Exception ex)
        {
            // TODO: Inject and use a logger to log the exception details securely
            // _logger.LogError(ex, "An error occurred while retrieving orders");
            return StatusCode(500, new { error = "An internal server error occurred. Please contact support if the problem persists." });
        }
    }

    /// <summary>
    /// Gets a specific order by key with OData query support
    /// </summary>
    /// <param name="key">The order ID</param>
    /// <returns>The order if found, otherwise NotFound</returns>
    /// <remarks>
    /// Example: GET /odata/OrdersOData(101)?$expand=Customer,OrderItems
    /// </remarks>
    [EnableQuery(MaxExpansionDepth = 3)]
    public async Task<IActionResult> Get(int key)
    {
        try
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(key);
            return order is not null ? Ok(order) : NotFound();
        }
        catch (Exception ex)
        {
            // TODO: Inject and use a logger to log the exception details securely
            // _logger.LogError(ex, "An error occurred while retrieving the order");
            return StatusCode(500, new { error = "An internal server error occurred. Please contact support if the problem persists." });
        }
    }

    /// <summary>
    /// Creates a new order
    /// </summary>
    /// <param name="order">The order to create</param>
    /// <returns>The created order</returns>
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] OrderModel order)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Validate customer exists
            var customerExists = await _unitOfWork.Customers.ExistsAsync(c => c.Id == order.CustomerId);
            if (!customerExists)
            {
                return BadRequest(new { error = "Customer not found" });
            }

            // Reset ID and timestamps as they are auto-generated
            order.Id = 0;
            order.CreatedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            // Set timestamps for order items
            foreach (var item in order.OrderItems)
            {
                item.Id = 0;
                item.CreatedAt = DateTime.UtcNow;
                item.UpdatedAt = DateTime.UtcNow;
            }

            var createdOrder = await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            return Created($"/odata/OrdersOData({createdOrder.Id})", createdOrder);
        }
        catch (Exception ex)
        {
            // TODO: Inject and use a logger to log the exception details securely
            // _logger.LogError(ex, "An error occurred while creating the order");
            return StatusCode(500, new { error = "An internal server error occurred. Please contact support if the problem persists." });
        }
    }

    /// <summary>
    /// Updates an existing order
    /// </summary>
    /// <param name="key">The order ID</param>
    /// <param name="updatedOrder">The updated order data</param>
    /// <returns>The updated order or NotFound if not exists</returns>
    [HttpPut]
    public async Task<IActionResult> Put(int key, [FromBody] OrderModel updatedOrder)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var existingOrder = await _unitOfWork.Orders.GetByIdAsync(key);
            if (existingOrder is null)
            {
                return NotFound();
            }

            // Validate customer exists
            var customerExists = await _unitOfWork.Customers.ExistsAsync(c => c.Id == updatedOrder.CustomerId);
            if (!customerExists)
            {
                return BadRequest(new { error = "Customer not found" });
            }

            // Update properties while preserving audit fields
            existingOrder.OrderDate = updatedOrder.OrderDate;
            existingOrder.CustomerId = updatedOrder.CustomerId;
            existingOrder.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Orders.Update(existingOrder);
            await _unitOfWork.SaveChangesAsync();

            return Updated(existingOrder);
        }
        catch (Exception ex)
        {
            // TODO: Inject and use a logger to log the exception details securely
            // _logger.LogError(ex, "An error occurred while updating the order");
            return StatusCode(500, new { error = "An internal server error occurred. Please contact support if the problem persists." });
        }
    }

    /// <summary>
    /// Deletes an order
    /// </summary>
    /// <param name="key">The order ID</param>
    /// <returns>NoContent if successful, NotFound if not exists</returns>
    [HttpDelete]
    public async Task<IActionResult> Delete(int key)
    {
        try
        {
            var deleted = await _unitOfWork.Orders.DeleteByIdAsync(key);
            if (!deleted)
            {
                return NotFound();
            }

            await _unitOfWork.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            // TODO: Inject and use a logger to log the exception details securely
            // _logger.LogError(ex, "An error occurred while deleting the order");
            return StatusCode(500, new { error = "An internal server error occurred. Please contact support if the problem persists." });
        }
    }

    /// <summary>
    /// Gets orders with full details (customer and order items included)
    /// </summary>
    /// <returns>Orders with full details</returns>
    [HttpGet("GetWithFullDetails")]
    [EnableQuery(MaxExpansionDepth = 3)]
    public IActionResult GetWithFullDetails()
    {
        try
        {
            var orders = _unitOfWork.Orders.GetWithFullDetails();
            return Ok(orders);
        }
        catch (Exception ex)
        {
            // TODO: Inject and use a logger to log the exception details securely
            // _logger.LogError(ex, "An error occurred while retrieving orders with details");
            return StatusCode(500, new { error = "An internal server error occurred. Please contact support if the problem persists." });
        }
    }

    /// <summary>
    /// Gets recent orders from the last specified number of days
    /// </summary>
    /// <param name="days">Number of days to look back</param>
    /// <returns>Recent orders</returns>
    [HttpGet("GetRecentOrders")]
    [EnableQuery]
    public IActionResult GetRecentOrders([FromQuery] int days = 30)
    {
        try
        {
            var orders = _unitOfWork.Orders.GetRecentOrders(days);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            // TODO: Inject and use a logger to log the exception details securely
            // _logger.LogError(ex, "An error occurred while retrieving recent orders");
            return StatusCode(500, new { error = "An internal server error occurred. Please contact support if the problem persists." });
        }
    }
}