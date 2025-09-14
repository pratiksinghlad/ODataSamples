using ODataDemo.Model;

namespace ODataSamples.Data.Repositories.Interfaces;

/// <summary>
/// Repository interface for Product entity with specific operations
/// </summary>
public interface IProductRepository : IRepository<ProductModel>
{
    /// <summary>
    /// Gets products by price range
    /// </summary>
    /// <param name="minPrice">Minimum price</param>
    /// <param name="maxPrice">Maximum price</param>
    /// <returns>IQueryable of products in price range</returns>
    IQueryable<ProductModel> GetByPriceRange(decimal minPrice, decimal maxPrice);

    /// <summary>
    /// Gets products by name pattern
    /// </summary>
    /// <param name="namePattern">Name pattern to search</param>
    /// <returns>IQueryable of products matching name pattern</returns>
    IQueryable<ProductModel> GetByNamePattern(string namePattern);

    /// <summary>
    /// Gets the most expensive products
    /// </summary>
    /// <param name="count">Number of products to return</param>
    /// <returns>Most expensive products</returns>
    Task<IEnumerable<ProductModel>> GetMostExpensiveAsync(int count);

    /// <summary>
    /// Gets products ordered by price
    /// </summary>
    /// <param name="ascending">Sort direction</param>
    /// <returns>IQueryable of products ordered by price</returns>
    IQueryable<ProductModel> GetOrderedByPrice(bool ascending = true);
}