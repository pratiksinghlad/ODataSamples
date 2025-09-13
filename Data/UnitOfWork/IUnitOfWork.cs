using ODataSamples.Data.Repositories.Interfaces;

namespace ODataSamples.Data.UnitOfWork;

/// <summary>
/// Unit of Work interface for coordinating repository operations and transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets the Product repository
    /// </summary>
    IProductRepository Products { get; }

    /// <summary>
    /// Gets the Customer repository
    /// </summary>
    ICustomerRepository Customers { get; }

    /// <summary>
    /// Gets the Order repository
    /// </summary>
    IOrderRepository Orders { get; }

    /// <summary>
    /// Gets a generic repository for the specified entity type
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <returns>Repository for the entity type</returns>
    IRepository<TEntity> Repository<TEntity>() where TEntity : class;

    /// <summary>
    /// Saves all changes made in this unit of work
    /// </summary>
    /// <returns>Number of affected records</returns>
    Task<int> SaveChangesAsync();

    /// <summary>
    /// Saves all changes made in this unit of work
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of affected records</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Begins a database transaction
    /// </summary>
    /// <returns>Database transaction</returns>
    Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync();

    /// <summary>
    /// Begins a database transaction with specified isolation level
    /// </summary>
    /// <param name="isolationLevel">Transaction isolation level</param>
    /// <returns>Database transaction</returns>
    Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync(
        System.Data.IsolationLevel isolationLevel);

    /// <summary>
    /// Executes a function within a transaction scope
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="operation">Operation to execute</param>
    /// <returns>Operation result</returns>
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation);

    /// <summary>
    /// Executes an action within a transaction scope
    /// </summary>
    /// <param name="operation">Operation to execute</param>
    Task ExecuteInTransactionAsync(Func<Task> operation);

    /// <summary>
    /// Detaches all tracked entities
    /// </summary>
    void DetachAllEntities();

    /// <summary>
    /// Gets the current change tracker state
    /// </summary>
    /// <returns>Information about tracked entities</returns>
    string GetChangeTrackerDebugView();
}