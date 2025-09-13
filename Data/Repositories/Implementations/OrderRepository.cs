using Microsoft.EntityFrameworkCore;
using ODataDemo.Model;
using ODataSamples.Data.Context;
using ODataSamples.Data.Repositories.Interfaces;

namespace ODataSamples.Data.Repositories.Implementations;

/// <summary>
/// Repository implementation for Order entity
/// </summary>
public class OrderRepository : Repository<OrderModel>, IOrderRepository
{
    /// <summary>
    /// Initializes a new instance of the OrderRepository class
    /// </summary>
    /// <param name="context">The database context</param>
    public OrderRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public IQueryable<OrderModel> GetWithFullDetails()
    {
        return GetWithInclude(o => o.Customer!, o => o.OrderItems!);
    }

    /// <inheritdoc/>
    public IQueryable<OrderModel> GetByCustomerId(int customerId)
    {
        return GetWhere(o => o.CustomerId == customerId);
    }

    /// <inheritdoc/>
    public IQueryable<OrderModel> GetByDateRange(DateTime fromDate, DateTime toDate)
    {
        return GetWhere(o => o.OrderDate >= fromDate && o.OrderDate <= toDate);
    }

    /// <inheritdoc/>
    public IQueryable<OrderModel> GetWithTotals()
    {
        return _dbSet
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .Include(o => o.Customer);
    }

    /// <inheritdoc/>
    public IQueryable<OrderModel> GetRecentOrders(int days)
    {
        if (days <= 0)
            throw new ArgumentException("Days must be greater than 0", nameof(days));

        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return GetWhere(o => o.OrderDate >= cutoffDate);
    }

    /// <inheritdoc/>
    public IQueryable<OrderModel> GetOrderedByDate(bool ascending = false)
    {
        return ascending
            ? _dbSet.AsNoTracking().OrderBy(o => o.OrderDate)
            : _dbSet.AsNoTracking().OrderByDescending(o => o.OrderDate);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<object>> GetOrderSummariesAsync()
    {
        return await _dbSet
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .Select(o => new
            {
                o.Id,
                o.OrderDate,
                o.CustomerId,
                CustomerName = o.Customer != null ? o.Customer.Name : "Unknown",
                ItemCount = o.OrderItems.Count,
                TotalValue = o.OrderItems.Sum(i => i.Price),
                AverageItemPrice = o.OrderItems.Any() ? o.OrderItems.Average(i => i.Price) : 0
            })
            .ToListAsync();
    }
}