using ODataDemo.Model;

namespace ODataSamples.Data.Repositories.Interfaces;

/// <summary>
/// Repository interface for Order entity with specific operations
/// </summary>
public interface IOrderRepository : IRepository<OrderModel>
{
    /// <summary>
    /// Gets orders with customer and order items included
    /// </summary>
    /// <returns>IQueryable of orders with full details</returns>
    IQueryable<OrderModel> GetWithFullDetails();

    /// <summary>
    /// Gets orders by customer ID
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>IQueryable of orders for the specified customer</returns>
    IQueryable<OrderModel> GetByCustomerId(int customerId);

    /// <summary>
    /// Gets orders by date range
    /// </summary>
    /// <param name="fromDate">Start date</param>
    /// <param name="toDate">End date</param>
    /// <returns>IQueryable of orders in the date range</returns>
    IQueryable<OrderModel> GetByDateRange(DateTime fromDate, DateTime toDate);

    /// <summary>
    /// Gets orders with their total value
    /// </summary>
    /// <returns>IQueryable of orders with calculated totals</returns>
    IQueryable<OrderModel> GetWithTotals();

    /// <summary>
    /// Gets recent orders
    /// </summary>
    /// <param name="days">Number of days to look back</param>
    /// <returns>IQueryable of recent orders</returns>
    IQueryable<OrderModel> GetRecentOrders(int days);

    /// <summary>
    /// Gets orders ordered by date
    /// </summary>
    /// <param name="ascending">Sort direction</param>
    /// <returns>IQueryable of orders ordered by date</returns>
    IQueryable<OrderModel> GetOrderedByDate(bool ascending = false);

    /// <summary>
    /// Gets order summaries for reporting
    /// </summary>
    /// <returns>Order summaries with key metrics</returns>
    Task<IEnumerable<object>> GetOrderSummariesAsync();
}