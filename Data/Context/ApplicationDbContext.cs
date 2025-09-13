using Microsoft.EntityFrameworkCore;
using ODataDemo.Model;

namespace ODataSamples.Data.Context;

/// <summary>
/// Application database context for OData samples
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the ApplicationDbContext
    /// </summary>
    /// <param name="options">The database context options</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the Products DbSet
    /// </summary>
    public DbSet<ProductModel> Products { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Customers DbSet
    /// </summary>
    public DbSet<CustomerModel> Customers { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Orders DbSet
    /// </summary>
    public DbSet<OrderModel> Orders { get; set; } = null!;

    /// <summary>
    /// Gets or sets the OrderItems DbSet
    /// </summary>
    public DbSet<OrderItemModel> OrderItems { get; set; } = null!;

    /// <summary>
    /// Configure entity relationships and constraints using Fluent API
    /// </summary>
    /// <param name="modelBuilder">The model builder</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Product entity
        modelBuilder.Entity<ProductModel>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.Price)
                .HasPrecision(10, 2);

            // Add index for better performance on name searches
            entity.HasIndex(e => e.Name);
        });

        // Configure Customer entity
        modelBuilder.Entity<CustomerModel>(entity =>
        {
            entity.ToTable("Customers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.City)
                .IsRequired()
                .HasMaxLength(100);

            // Configure one-to-many relationship with Orders
            entity.HasMany(e => e.Orders)
                .WithOne(e => e.Customer)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading deletes for consistency

            // Add indexes for better performance
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.City);
        });

        // Configure Order entity
        modelBuilder.Entity<OrderModel>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderDate)
                .IsRequired();
            entity.Property(e => e.CustomerId)
                .IsRequired();

            // Configure relationship with Customer
            entity.HasOne(e => e.Customer)
                .WithMany(e => e.Orders)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading deletes

            // Configure one-to-many relationship with OrderItems
            entity.HasMany(e => e.OrderItems)
                .WithOne(e => e.Order)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Add indexes for better performance
            entity.HasIndex(e => e.OrderDate);
            entity.HasIndex(e => e.CustomerId);
        });

        // Configure OrderItem entity
        modelBuilder.Entity<OrderItemModel>(entity =>
        {
            entity.ToTable("OrderItems");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductName)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .IsRequired();
            entity.Property(e => e.OrderId)
                .IsRequired();

            // Configure relationship with Order
            entity.HasOne(e => e.Order)
                .WithMany(e => e.OrderItems)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Add indexes for better performance
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.ProductName);
        });

        // Global query filters for soft deletes (if needed in future)
        // modelBuilder.Entity<ProductModel>().HasQueryFilter(e => !e.IsDeleted);
    }

    /// <summary>
    /// Override SaveChanges to add audit fields automatically
    /// </summary>
    /// <returns>Number of affected records</returns>
    public override int SaveChanges()
    {
        AddAuditInfo();
        return base.SaveChanges();
    }

    /// <summary>
    /// Override SaveChangesAsync to add audit fields automatically
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of affected records</returns>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddAuditInfo();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Add audit information to entities (if audit fields are added in future)
    /// </summary>
    private void AddAuditInfo()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            // Future implementation for audit fields like CreatedAt, UpdatedAt
            // if (entry.Entity is IAuditable auditable)
            // {
            //     if (entry.State == EntityState.Added)
            //         auditable.CreatedAt = DateTime.UtcNow;
            //     auditable.UpdatedAt = DateTime.UtcNow;
            // }
        }
    }
}