using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ODataSamples.Data.Context;
using ODataSamples.Data.Repositories.Interfaces;

namespace ODataSamples.Data.Repositories.Implementations;

/// <summary>
/// Generic repository implementation providing CRUD operations and OData support
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <summary>
/// Generic repository implementation providing CRUD operations and OData support
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TKey">The type of the entity key</typeparam>

public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    /// <summary>
    /// Initializes a new instance of the Repository class
    /// </summary>
    /// <param name="context">The database context</param>
    public Repository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<TEntity>();
    }

    /// <inheritdoc/>
    public virtual IQueryable<TEntity> GetAll()
    {
        return _dbSet.AsNoTracking();
    }

    /// <inheritdoc/>
    public virtual IQueryable<TEntity> GetWhere(Expression<Func<TEntity, bool>> filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return _dbSet.AsNoTracking().Where(filter);
    }

    /// <inheritdoc/>
    /// <inheritdoc/>

    public virtual async Task<TEntity?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    /// <inheritdoc/>
    public virtual async Task<TEntity?> GetByIdAsync(int id, params Expression<Func<TEntity, object>>[] includeProperties)
    {
        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        // Include related entities
        foreach (var includeProperty in includeProperties)
        {
            query = query.Include(includeProperty);
        }

        // Find entity by key
        return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
    }

    /// <inheritdoc/>
    public virtual IQueryable<TEntity> GetWithInclude(params Expression<Func<TEntity, object>>[] includeProperties)
    {
        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        foreach (var includeProperty in includeProperties)
        {
            query = query.Include(includeProperty);
        }

        return query;
    }

    /// <inheritdoc/>
    public virtual IQueryable<TEntity> GetWhereWithInclude(
        Expression<Func<TEntity, bool>> filter,
        params Expression<Func<TEntity, object>>[] includeProperties)
    {
        ArgumentNullException.ThrowIfNull(filter);

        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        foreach (var includeProperty in includeProperties)
        {
            query = query.Include(includeProperty);
        }

        return query.Where(filter);
    }

    /// <inheritdoc/>
    public virtual async Task<TEntity> AddAsync(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var entityEntry = await _dbSet.AddAsync(entity);
        return entityEntry.Entity;
    }

    /// <inheritdoc/>
    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        await _dbSet.AddRangeAsync(entities);
    }

    /// <inheritdoc/>
    public virtual void Update(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _dbSet.Update(entity);
    }

    /// <inheritdoc/>
    public virtual void UpdateRange(IEnumerable<TEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        _dbSet.UpdateRange(entities);
    }

    /// <inheritdoc/>
    public virtual void Delete(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _dbSet.Remove(entity);
    }

    /// <inheritdoc/>
    public virtual async Task<bool> DeleteByIdAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity is null)
            return false;

        Delete(entity);
        return true;
    }

    /// <inheritdoc/>
    public virtual void DeleteRange(IEnumerable<TEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        _dbSet.RemoveRange(entities);
    }

    /// <inheritdoc/>
    public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return await _dbSet.AnyAsync(filter);
    }

    /// <inheritdoc/>
    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? filter = null)
    {
        if (filter is null)
            return await _dbSet.CountAsync();

        return await _dbSet.CountAsync(filter);
    }

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<TEntity>> GetPagedAsync<TKey>(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, TKey>> orderBy,
        bool ascending = true)
    {
        ArgumentNullException.ThrowIfNull(orderBy);

        if (pageNumber < 1)
            throw new ArgumentException("Page number must be greater than 0", nameof(pageNumber));

        if (pageSize < 1)
            throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));

        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        query = ascending
            ? query.OrderBy(orderBy)
            : query.OrderByDescending(orderBy);

        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public virtual IQueryable<TEntity> GetAdvanced<TKey>(
        Expression<Func<TEntity, bool>>? filter = null,
        Expression<Func<TEntity, TKey>>? orderBy = null,
        bool ascending = true,
        int? skip = null,
        int? take = null,
        params Expression<Func<TEntity, object>>[] includeProperties)
    {
        IQueryable<TEntity> query = _dbSet.AsNoTracking();

        // Apply includes
        foreach (var includeProperty in includeProperties)
        {
            query = query.Include(includeProperty);
        }

        // Apply filter
        if (filter is not null)
        {
            query = query.Where(filter);
        }

        // Apply ordering
        if (orderBy is not null)
        {
            query = ascending
                ? query.OrderBy(orderBy)
                : query.OrderByDescending(orderBy);
        }

        // Apply pagination
        if (skip.HasValue)
        {
            query = query.Skip(skip.Value);
        }

        if (take.HasValue)
        {
            query = query.Take(take.Value);
        }

        return query;
    }
}