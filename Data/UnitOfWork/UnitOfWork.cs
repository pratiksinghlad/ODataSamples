using System.Collections.Concurrent;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ODataSamples.Data.Context;
using ODataSamples.Data.Repositories.Interfaces;
using ODataSamples.Data.Repositories.Implementations;

namespace ODataSamples.Data.UnitOfWork;

/// <summary>
/// Unit of Work implementation for coordinating repository operations and transactions
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly ConcurrentDictionary<Type, object> _repositories;
    private bool _disposed;

    // Specific repositories
    private IProductRepository? _productRepository;
    private ICustomerRepository? _customerRepository;
    private IOrderRepository? _orderRepository;

    /// <summary>
    /// Initializes a new instance of the UnitOfWork class
    /// </summary>
    /// <param name="context">The database context</param>
    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _repositories = new ConcurrentDictionary<Type, object>();
    }

    /// <inheritdoc/>
    public IProductRepository Products =>
        _productRepository ??= new ProductRepository(_context);

    /// <inheritdoc/>
    public ICustomerRepository Customers =>
        _customerRepository ??= new CustomerRepository(_context);

    /// <inheritdoc/>
    public IOrderRepository Orders =>
        _orderRepository ??= new OrderRepository(_context);

    /// <inheritdoc/>
    public IRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        var type = typeof(TEntity);

        return (IRepository<TEntity>)_repositories.GetOrAdd(type, _ =>
            new Repository<TEntity>(_context));
    }

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync()
    {
        try
        {
            return await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Handle concurrency conflicts
            throw new InvalidOperationException(
                "A concurrency conflict occurred while saving changes. " +
                "The record may have been modified by another user.", ex);
        }
        catch (DbUpdateException ex)
        {
            // Handle database update errors
            throw new InvalidOperationException(
                "An error occurred while saving changes to the database.", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InvalidOperationException(
                "A concurrency conflict occurred while saving changes.", ex);
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException(
                "An error occurred while saving changes to the database.", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _context.Database.BeginTransactionAsync();
    }

    /// <inheritdoc/>
    public async Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel)
    {
        return await _context.Database.BeginTransactionAsync(isolationLevel);
    }

    /// <inheritdoc/>
    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        using var transaction = await BeginTransactionAsync();
        try
        {
            var result = await operation();
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task ExecuteInTransactionAsync(Func<Task> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        using var transaction = await BeginTransactionAsync();
        try
        {
            await operation();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <inheritdoc/>
    public void DetachAllEntities()
    {
        var changedEntriesCopy = _context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added ||
                       e.State == EntityState.Modified ||
                       e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in changedEntriesCopy)
        {
            entry.State = EntityState.Detached;
        }
    }

    /// <inheritdoc/>
    public string GetChangeTrackerDebugView()
    {
        var entries = _context.ChangeTracker.Entries().ToList();
        if (!entries.Any())
            return "No tracked entities.";

        var debugInfo = new List<string>
        {
            $"Tracked entities: {entries.Count}"
        };

        var groupedEntries = entries.GroupBy(e => e.State);
        foreach (var group in groupedEntries)
        {
            debugInfo.Add($"  {group.Key}: {group.Count()}");
        }

        return string.Join(Environment.NewLine, debugInfo);
    }

    /// <summary>
    /// Dispose of resources
    /// </summary>
    /// <param name="disposing">Whether disposing</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _context.Dispose();
            _repositories.Clear();
        }
        _disposed = true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}