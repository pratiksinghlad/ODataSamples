using System.Linq.Expressions;

namespace ODataSamples.Data.Repositories.Interfaces;

/// <summary>
/// Generic repository interface providing CRUD operations and OData support
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
public interface IRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Gets all entities as IQueryable for OData support
    /// </summary>
    /// <returns>IQueryable of entities</returns>
    IQueryable<TEntity> GetAll();

    /// <summary>
    /// Gets entities with filtering support
    /// </summary>
    /// <param name="filter">Filter expression</param>
    /// <returns>IQueryable of filtered entities</returns>
    IQueryable<TEntity> GetWhere(Expression<Func<TEntity, bool>> filter);

    /// <summary>
    /// Gets an entity by its identifier
    /// </summary>
    /// <param name="id">The entity identifier</param>
    /// <returns>The entity if found, otherwise null</returns>
    Task<TEntity?> GetByIdAsync(int id);

    /// <summary>
    /// Gets an entity by its identifier with related entities
    /// </summary>
    /// <param name="id">The entity identifier</param>
    /// <param name="includeProperties">Properties to include</param>
    /// <returns>The entity if found, otherwise null</returns>
    Task<TEntity?> GetByIdAsync(int id, params Expression<Func<TEntity, object>>[] includeProperties);

    /// <summary>
    /// Gets entities with included related entities
    /// </summary>
    /// <param name="includeProperties">Properties to include</param>
    /// <returns>IQueryable of entities with included properties</returns>
    IQueryable<TEntity> GetWithInclude(params Expression<Func<TEntity, object>>[] includeProperties);

    /// <summary>
    /// Gets filtered entities with included related entities
    /// </summary>
    /// <param name="filter">Filter expression</param>
    /// <param name="includeProperties">Properties to include</param>
    /// <returns>IQueryable of filtered entities with included properties</returns>
    IQueryable<TEntity> GetWhereWithInclude(
        Expression<Func<TEntity, bool>> filter,
        params Expression<Func<TEntity, object>>[] includeProperties);

    /// <summary>
    /// Adds a new entity
    /// </summary>
    /// <param name="entity">The entity to add</param>
    /// <returns>The added entity</returns>
    Task<TEntity> AddAsync(TEntity entity);

    /// <summary>
    /// Adds multiple entities
    /// </summary>
    /// <param name="entities">The entities to add</param>
    Task AddRangeAsync(IEnumerable<TEntity> entities);

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="entity">The entity to update</param>
    void Update(TEntity entity);

    /// <summary>
    /// Updates multiple entities
    /// </summary>
    /// <param name="entities">The entities to update</param>
    void UpdateRange(IEnumerable<TEntity> entities);

    /// <summary>
    /// Deletes an entity
    /// </summary>
    /// <param name="entity">The entity to delete</param>
    void Delete(TEntity entity);

    /// <summary>
    /// Deletes an entity by its identifier
    /// </summary>
    /// <param name="id">The entity identifier</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteByIdAsync(int id);

    /// <summary>
    /// Deletes multiple entities
    /// </summary>
    /// <param name="entities">The entities to delete</param>
    void DeleteRange(IEnumerable<TEntity> entities);

    /// <summary>
    /// Checks if any entity matches the filter
    /// </summary>
    /// <param name="filter">Filter expression</param>
    /// <returns>True if any entity matches, otherwise false</returns>
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter);

    /// <summary>
    /// Gets the count of entities matching the filter
    /// </summary>
    /// <param name="filter">Filter expression (optional)</param>
    /// <returns>Count of entities</returns>
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? filter = null);

    /// <summary>
    /// Gets entities with pagination support
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="orderBy">Order by expression</param>
    /// <param name="ascending">Sort direction</param>
    /// <returns>Paginated entities</returns>
    Task<IEnumerable<TEntity>> GetPagedAsync<TKey>(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, TKey>> orderBy,
        bool ascending = true);

    /// <summary>
    /// Gets entities with advanced filtering, sorting, and pagination
    /// </summary>
    /// <param name="filter">Filter expression (optional)</param>
    /// <param name="orderBy">Order by expression (optional)</param>
    /// <param name="ascending">Sort direction</param>
    /// <param name="skip">Number of entities to skip</param>
    /// <param name="take">Number of entities to take</param>
    /// <param name="includeProperties">Properties to include</param>
    /// <returns>Filtered, sorted, and paginated entities</returns>
    IQueryable<TEntity> GetAdvanced<TKey>(
        Expression<Func<TEntity, bool>>? filter = null,
        Expression<Func<TEntity, TKey>>? orderBy = null,
        bool ascending = true,
        int? skip = null,
        int? take = null,
        params Expression<Func<TEntity, object>>[] includeProperties);
}