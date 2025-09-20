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
        Console.WriteLine($" CustomerRepository.GetByCity() called with city: {city}");
        Console.WriteLine($"    This is a CUSTOM repository method - used by business logic, not OData");
        
        return GetWhere(c => c.City.Equals(city, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc/>
    public IQueryable<CustomerModel> GetWithOrders()
    {
        Console.WriteLine($" CustomerRepository.GetWithOrders() called");
        Console.WriteLine($"    This is a CUSTOM repository method for including related Orders");
        
        return GetWithInclude(c => c.Orders);
    }

    /// <inheritdoc/>
    public IQueryable<CustomerModel> GetByNamePattern(string namePattern)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(namePattern);
        Console.WriteLine($" CustomerRepository.GetByNamePattern() called with pattern: {namePattern}");
        Console.WriteLine($"    This is a CUSTOM repository method for name-based filtering");
        
        return GetWhere(c => c.Name.Contains(namePattern));
    }

    /// <inheritdoc/>
    public IQueryable<CustomerModel> GetWithOrderStatistics()
    {
        Console.WriteLine($" CustomerRepository.GetWithOrderStatistics() called");
        Console.WriteLine($"    This is a CUSTOM repository method for complex includes (Orders + OrderItems)");
        
        return _dbSet
            .AsNoTracking()
            .Include(c => c.Orders)
            .ThenInclude(o => o.OrderItems);
    }

    /// <inheritdoc/>
    public IQueryable<CustomerModel> GetByOrderDateRange(DateTime fromDate, DateTime toDate)
    {
        Console.WriteLine($" CustomerRepository.GetByOrderDateRange() called from {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}");
        Console.WriteLine($"    This is a CUSTOM repository method for complex date-based filtering");
        
        return _dbSet
            .AsNoTracking()
            .Include(c => c.Orders)
            .Where(c => c.Orders.Any(o => o.OrderDate >= fromDate && o.OrderDate <= toDate));
    }
}
