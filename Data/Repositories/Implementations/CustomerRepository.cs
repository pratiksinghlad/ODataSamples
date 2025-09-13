using Microsoft.EntityFrameworkCore;
using ODataDemo.Model;
using ODataSamples.Data.Context;
using ODataSamples.Data.Repositories.Interfaces;

namespace ODataSamples.Data.Repositories.Implementations;

/// <summary>
/// Repository implementation for Customer entity
/// </summary>
public class CustomerRepository : Repository<CustomerModel>, ICustomerRepository
{
    /// <summary>
    /// Initializes a new instance of the CustomerRepository class
    /// </summary>
    /// <param name="context">The database context</param>
    public CustomerRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public IQueryable<CustomerModel> GetByCity(string city)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        return GetWhere(c => c.City.Equals(city, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc/>
    public IQueryable<CustomerModel> GetWithOrders()
    {
        return GetWithInclude(c => c.Orders);
    }

    /// <inheritdoc/>
    public IQueryable<CustomerModel> GetByNamePattern(string namePattern)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(namePattern);
        return GetWhere(c => c.Name.Contains(namePattern));
    }

    /// <inheritdoc/>
    public IQueryable<CustomerModel> GetWithOrderStatistics()
    {
        return _dbSet
            .AsNoTracking()
            .Include(c => c.Orders)
            .ThenInclude(o => o.OrderItems);
    }

    /// <inheritdoc/>
    public IQueryable<CustomerModel> GetByOrderDateRange(DateTime fromDate, DateTime toDate)
    {
        return _dbSet
            .AsNoTracking()
            .Include(c => c.Orders)
            .Where(c => c.Orders.Any(o => o.OrderDate >= fromDate && o.OrderDate <= toDate));
    }
}