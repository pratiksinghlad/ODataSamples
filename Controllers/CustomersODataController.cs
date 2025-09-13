using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using ODataDemo.Model;
using ODataSamples.Data.UnitOfWork;

namespace ODataDemo.Controllers;

/// <summary>
/// OData controller for Customers with full query capabilities using Repository pattern
/// </summary>
public class CustomersODataController : ODataController
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the CustomersODataController
    /// </summary>
    /// <param name="unitOfWork">Unit of work for data access</param>
    public CustomersODataController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Gets all customers with OData query support
    /// </summary>
    /// <returns>Queryable collection of customers</returns>
    /// <remarks>
    /// Supports OData queries like:
    /// - GET /odata/CustomersOData?$expand=Orders
    /// - GET /odata/CustomersOData?$filter=City eq 'Seattle'&$orderby=Name
    /// - GET /odata/CustomersOData?$expand=Orders($select=Id,OrderDate)&$select=Name,City
    /// </remarks>
    [EnableQuery(MaxExpansionDepth = 3, MaxTop = 1000, MaxOrderByNodeCount = 10)]
    public IActionResult Get()
    {
        try
        {
            var customers = _unitOfWork.Customers.GetAll();
            return Ok(customers);
        }
        catch (Exception ex)
        {
            // TODO: Inject and use a logger service for production scenarios
            Console.Error.WriteLine($"Error retrieving customers: {ex}");
            return StatusCode(
                500,
                new { error = "An error occurred while retrieving customers. Please contact support if the problem persists." });
        }
    }

    /// <summary>
    /// Gets a specific customer by key with OData query support
    /// </summary>
    /// <param name="key">The customer ID</param>
    /// <returns>The customer if found, otherwise NotFound</returns>
    /// <remarks>
    /// Example: GET /odata/CustomersOData(1)?$expand=Orders
    /// </remarks>
    [EnableQuery(MaxExpansionDepth = 3)]
    public async Task<IActionResult> Get(int key)
    {
        try
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(key);
            return customer is not null ? Ok(customer) : NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(
                500,
                new { error = "An error occurred while retrieving the customer", details = ex.Message });
        }
    }

    /// <summary>
    /// Creates a new customer
    /// </summary>
    /// <param name="customer">The customer to create</param>
    /// <returns>The created customer</returns>
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CustomerModel customer)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Reset ID and timestamps as they are auto-generated
            customer.Id = 0;
            customer.CreatedAt = DateTime.UtcNow;
            customer.UpdatedAt = DateTime.UtcNow;

            var createdCustomer = await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            return Created($"/odata/CustomersOData({createdCustomer.Id})", createdCustomer);
        }
        catch (Exception ex)
        {
            return StatusCode(
                500,
                new { error = "An error occurred while creating the customer", details = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing customer
    /// </summary>
    /// <param name="key">The customer ID</param>
    /// <param name="updatedCustomer">The updated customer data</param>
    /// <returns>The updated customer or NotFound if not exists</returns>
    [HttpPut]
    public async Task<IActionResult> Put(int key, [FromBody] CustomerModel updatedCustomer)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var existingCustomer = await _unitOfWork.Customers.GetByIdAsync(key);
            if (existingCustomer is null)
            {
                return NotFound();
            }

            // Update properties while preserving audit fields
            existingCustomer.Name = updatedCustomer.Name;
            existingCustomer.City = updatedCustomer.City;
            existingCustomer.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Customers.Update(existingCustomer);
            await _unitOfWork.SaveChangesAsync();

            return Updated(existingCustomer);
        }
        catch (Exception ex)
        {
            return StatusCode(
                500,
                new { error = "An error occurred while updating the customer", details = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a customer
    /// </summary>
    /// <param name="key">The customer ID</param>
    /// <returns>NoContent if successful, NotFound if not exists</returns>
    [HttpDelete]
    public async Task<IActionResult> Delete(int key)
    {
        try
        {
            var deleted = await _unitOfWork.Customers.DeleteByIdAsync(key);
            if (!deleted)
            {
                return NotFound();
            }

            await _unitOfWork.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(
                500,
                new { error = "An error occurred while deleting the customer", details = ex.Message });
        }
    }

    /// <summary>
    /// Gets customers by city (custom OData function)
    /// </summary>
    /// <param name="city">City name</param>
    /// <returns>Customers in the specified city</returns>
    [HttpGet("GetByCity")]
    [EnableQuery]
    public IActionResult GetByCity([FromQuery] string city)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return BadRequest(new { error = "City parameter is required" });
            }

            var customers = _unitOfWork.Customers.GetByCity(city);
            return Ok(customers);
        }
        catch (Exception ex)
        {
            return StatusCode(
                500,
                new { error = "An error occurred while retrieving customers by city", details = ex.Message });
        }
    }
}