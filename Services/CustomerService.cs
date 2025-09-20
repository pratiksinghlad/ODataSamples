using ODataDemo.Model;
using ODataSamples.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace ODataSamples.Services;

/// <summary>
/// Service interface for customer business logic operations
/// </summary>
public interface ICustomerService
{
    /// <summary>
    /// Gets customer statistics for a specific city
    /// This demonstrates when custom repository methods are actually useful
    /// </summary>
    Task<CustomerCityStatisticsDto> GetCityStatisticsAsync(string city);

    /// <summary>
    /// Gets customers with complex business rules applied
    /// This shows when you need custom repository methods vs OData
    /// </summary>
    Task<List<CustomerSummaryDto>> GetActiveCustomersWithRecentOrdersAsync();

    /// <summary>
    /// Gets customer dashboard data using multiple repository methods
    /// </summary>
    Task<CustomerDashboardDto> GetCustomerDashboardAsync(int customerId);

    /// <summary>
    /// Performs a complex business operation that requires custom repository methods
    /// </summary>
    Task<CustomerAnalysisDto> AnalyzeCustomerBehaviorAsync(string city, DateTime fromDate, DateTime toDate);
}

/// <summary>
/// Customer service implementing business logic that actually uses custom repository methods
/// This demonstrates when repository methods are called vs when OData handles filtering
/// </summary>
public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;

    public CustomerService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Gets customer statistics for a specific city
    /// 🎯 This ACTUALLY calls GetByCity() repository method
    /// </summary>
    public async Task<CustomerCityStatisticsDto> GetCityStatisticsAsync(string city)
    {
        Console.WriteLine($"🏢 CustomerService.GetCityStatisticsAsync() called for city: {city}");
        
        // This will call the custom repository method GetByCity()
        var customers = await _unitOfWork.Customers
            .GetByCity(city) // 🎯 CUSTOM REPOSITORY METHOD CALLED HERE!
            .Include(c => c.Orders)
            .ToListAsync();

        return new CustomerCityStatisticsDto
        {
            City = city,
            TotalCustomers = customers.Count,
            CustomersWithOrders = customers.Count(c => c.Orders.Any()),
            TotalOrders = customers.SelectMany(c => c.Orders).Count(),
            AverageOrdersPerCustomer = customers.Any() 
                ? (double)customers.SelectMany(c => c.Orders).Count() / customers.Count 
                : 0
        };
    }

    /// <summary>
    /// Gets customers with complex business rules
    /// 🎯 This uses multiple custom repository methods
    /// </summary>
    public async Task<List<CustomerSummaryDto>> GetActiveCustomersWithRecentOrdersAsync()
    {
        Console.WriteLine($"🏢 CustomerService.GetActiveCustomersWithRecentOrdersAsync() called");
        
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        
        // This calls GetByOrderDateRange() custom repository method
        var recentCustomers = await _unitOfWork.Customers
            .GetByOrderDateRange(thirtyDaysAgo, DateTime.UtcNow) // 🎯 CUSTOM METHOD!
            .Select(c => new CustomerSummaryDto
            {
                Id = c.Id,
                Name = c.Name,
                City = c.City,
                RecentOrderCount = c.Orders.Count(o => o.OrderDate >= thirtyDaysAgo),
                LastOrderDate = c.Orders.Max(o => o.OrderDate)
            })
            .ToListAsync();

        return recentCustomers;
    }

    /// <summary>
    /// Gets comprehensive customer dashboard data
    /// 🎯 This uses GetWithOrderStatistics() custom repository method
    /// </summary>
    public async Task<CustomerDashboardDto> GetCustomerDashboardAsync(int customerId)
    {
        Console.WriteLine($"🏢 CustomerService.GetCustomerDashboardAsync() called for customer: {customerId}");
        
        // This calls GetWithOrderStatistics() to get customer with full order details
        var customer = await _unitOfWork.Customers
            .GetWithOrderStatistics() // 🎯 CUSTOM METHOD WITH COMPLEX INCLUDES!
            .FirstOrDefaultAsync(c => c.Id == customerId);

        if (customer == null)
            throw new ArgumentException($"Customer with ID {customerId} not found");

        return new CustomerDashboardDto
        {
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            City = customer.City,
            TotalOrders = customer.Orders.Count,
            TotalOrderItems = customer.Orders.SelectMany(o => o.OrderItems).Count(),
            TotalSpent = customer.Orders.SelectMany(o => o.OrderItems).Sum(oi => oi.Price),
            AverageOrderValue = customer.Orders.Any() 
                ? customer.Orders.SelectMany(o => o.OrderItems).Sum(oi => oi.Price) / customer.Orders.Count 
                : 0,
            FirstOrderDate = customer.Orders.Any() ? customer.Orders.Min(o => o.OrderDate) : (DateTime?)null,
            LastOrderDate = customer.Orders.Any() ? customer.Orders.Max(o => o.OrderDate) : (DateTime?)null
        };
    }

    /// <summary>
    /// Performs complex analysis combining multiple repository methods
    /// 🎯 This shows real business logic that needs custom repository methods
    /// </summary>
    public async Task<CustomerAnalysisDto> AnalyzeCustomerBehaviorAsync(string city, DateTime fromDate, DateTime toDate)
    {
        Console.WriteLine($"🏢 CustomerService.AnalyzeCustomerBehaviorAsync() called for {city} from {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}");
        
        // Step 1: Get all customers in the city (custom method)
        var cityCustomers = await _unitOfWork.Customers
            .GetByCity(city) // 🎯 CUSTOM METHOD!
            .CountAsync();

        // Step 2: Get customers with orders in date range (custom method)
        var activeCustomers = await _unitOfWork.Customers
            .GetByOrderDateRange(fromDate, toDate) // 🎯 CUSTOM METHOD!
            .Where(c => c.City.Equals(city, StringComparison.OrdinalIgnoreCase))
            .ToListAsync();

        // Step 3: Get detailed statistics (custom method)
        var detailedCustomers = await _unitOfWork.Customers
            .GetWithOrderStatistics() // 🎯 CUSTOM METHOD!
            .Where(c => c.City.Equals(city, StringComparison.OrdinalIgnoreCase))
            .Where(c => c.Orders.Any(o => o.OrderDate >= fromDate && o.OrderDate <= toDate))
            .ToListAsync();

        return new CustomerAnalysisDto
        {
            City = city,
            AnalysisPeriod = $"{fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}",
            TotalCustomersInCity = cityCustomers,
            ActiveCustomersInPeriod = activeCustomers.Count,
            ActivityRate = cityCustomers > 0 ? (double)activeCustomers.Count / cityCustomers * 100 : 0,
            TotalOrdersInPeriod = detailedCustomers.SelectMany(c => c.Orders)
                .Count(o => o.OrderDate >= fromDate && o.OrderDate <= toDate),
            TotalRevenueInPeriod = detailedCustomers.SelectMany(c => c.Orders)
                .Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate)
                .SelectMany(o => o.OrderItems)
                .Sum(oi => oi.Price),
            TopCustomers = detailedCustomers
                .Select(c => new TopCustomerDto
                {
                    Name = c.Name,
                    OrderCount = c.Orders.Count(o => o.OrderDate >= fromDate && o.OrderDate <= toDate),
                    TotalSpent = c.Orders
                        .Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate)
                        .SelectMany(o => o.OrderItems)
                        .Sum(oi => oi.Price)
                })
                .OrderByDescending(tc => tc.TotalSpent)
                .Take(5)
                .ToList()
        };
    }
}

#region DTOs for Business Logic Results

/// <summary>
/// DTO for customer city statistics
/// </summary>
public class CustomerCityStatisticsDto
{
    public string City { get; set; } = string.Empty;
    public int TotalCustomers { get; set; }
    public int CustomersWithOrders { get; set; }
    public int TotalOrders { get; set; }
    public double AverageOrdersPerCustomer { get; set; }
}

/// <summary>
/// DTO for customer summary information
/// </summary>
public class CustomerSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int RecentOrderCount { get; set; }
    public DateTime LastOrderDate { get; set; }
}

/// <summary>
/// DTO for customer dashboard information
/// </summary>
public class CustomerDashboardDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int TotalOrders { get; set; }
    public int TotalOrderItems { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal AverageOrderValue { get; set; }
    public DateTime? FirstOrderDate { get; set; }
    public DateTime? LastOrderDate { get; set; }
}

/// <summary>
/// DTO for customer behavior analysis
/// </summary>
public class CustomerAnalysisDto
{
    public string City { get; set; } = string.Empty;
    public string AnalysisPeriod { get; set; } = string.Empty;
    public int TotalCustomersInCity { get; set; }
    public int ActiveCustomersInPeriod { get; set; }
    public double ActivityRate { get; set; }
    public int TotalOrdersInPeriod { get; set; }
    public decimal TotalRevenueInPeriod { get; set; }
    public List<TopCustomerDto> TopCustomers { get; set; } = new();
}

/// <summary>
/// DTO for top customer information
/// </summary>
public class TopCustomerDto
{
    public string Name { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal TotalSpent { get; set; }
}

#endregion