using ODataDemo.Model;

namespace ODataSamples.Data.Repositories.Interfaces;

/// <summary>
/// Repository interface for Customer entity with specific operations
/// </summary>
public interface ICustomerRepository : IRepository<CustomerModel>
{
    /// <summary>
    /// Gets customers by city
    /// </summary>
    /// <param name="city">City name</param>
    /// <returns>IQueryable of customers in the specified city</returns>
    IQueryable<CustomerModel> GetByCity(string city);

    /// <summary>
    /// Gets customers with their orders
    /// </summary>
    /// <returns>IQueryable of customers with orders included</returns>
    IQueryable<CustomerModel> GetWithOrders();

    /// <summary>
    /// Gets customers by name pattern
    /// </summary>
    /// <param name="namePattern">Name pattern to search</param>
    /// <returns>IQueryable of customers matching name pattern</returns>
    IQueryable<CustomerModel> GetByNamePattern(string namePattern);

    /// <summary>
    /// Gets customers with order count
    /// </summary>
    /// <returns>IQueryable of customers with order statistics</returns>
    IQueryable<CustomerModel> GetWithOrderStatistics();

    /// <summary>
    /// Gets customers by order date range
    /// </summary>
    /// <param name="fromDate">Start date</param>
    /// <param name="toDate">End date</param>
    /// <returns>IQueryable of customers who placed orders in the date range</returns>
    IQueryable<CustomerModel> GetByOrderDateRange(DateTime fromDate, DateTime toDate);
}