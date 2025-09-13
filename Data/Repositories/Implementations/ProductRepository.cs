using Microsoft.EntityFrameworkCore;
using ODataDemo.Model;
using ODataSamples.Data.Context;
using ODataSamples.Data.Repositories.Interfaces;

namespace ODataSamples.Data.Repositories.Implementations;

/// <summary>
/// Repository implementation for Product entity
/// </summary>
public class ProductRepository : Repository<ProductModel>, IProductRepository
{
    /// <summary>
    /// Initializes a new instance of the ProductRepository class
    /// </summary>
    /// <param name="context">The database context</param>
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <inheritdoc/>
    public IQueryable<ProductModel> GetByPriceRange(decimal minPrice, decimal maxPrice)
    {
        return GetWhere(p => p.Price >= minPrice && p.Price <= maxPrice);
    }

    /// <inheritdoc/>
    public IQueryable<ProductModel> GetByNamePattern(string namePattern)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(namePattern);
        return GetWhere(p => p.Name.Contains(namePattern));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ProductModel>> GetMostExpensiveAsync(int count)
    {
        if (count <= 0)
            throw new ArgumentException("Count must be greater than 0", nameof(count));

        return await _dbSet
            .AsNoTracking()
            .OrderByDescending(p => p.Price)
            .Take(count)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public IQueryable<ProductModel> GetOrderedByPrice(bool ascending = true)
    {
        return ascending
            ? _dbSet.AsNoTracking().OrderBy(p => p.Price)
            : _dbSet.AsNoTracking().OrderByDescending(p => p.Price);
    }
}