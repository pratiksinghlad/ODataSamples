using Microsoft.EntityFrameworkCore;
using ODataDemo.Model;
using ODataSamples.Data.Context;

namespace ODataSamples.Data.Seeding;

/// <summary>
/// Data seeder for initial database population
/// </summary>
public class DataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DataSeeder> _logger;

    /// <summary>
    /// Initializes a new instance of the DataSeeder
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="logger">Logger</param>
    public DataSeeder(ApplicationDbContext context, ILogger<DataSeeder> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Seeds the database with initial data
    /// </summary>
    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting database seeding...");

            // Check if data already exists
            if (await _context.Products.AnyAsync())
            {
                _logger.LogInformation("Database already contains data. Skipping seeding.");
                return;
            }

            await SeedProductsAsync();
            await SeedCustomersAsync();
            await SeedOrdersAsync();

            await _context.SaveChangesAsync();
            _logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private async Task SeedProductsAsync()
    {
        var products = new List<ProductModel>
        {
            new()
            {
                Name = "Laptop Pro 15\"",
                Price = 1299.99m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Smartphone X",
                Price = 699.99m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Tablet Air",
                Price = 399.99m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Wireless Headphones",
                Price = 249.99m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Smart Watch",
                Price = 299.99m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Desktop Monitor 27\"",
                Price = 349.99m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Mechanical Keyboard",
                Price = 129.99m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Gaming Mouse",
                Price = 79.99m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        await _context.Products.AddRangeAsync(products);
        _logger.LogInformation("Seeded {Count} products", products.Count);
    }

    private async Task SeedCustomersAsync()
    {
        var customers = new List<CustomerModel>
        {
            new()
            {
                Name = "Alice Johnson",
                City = "Seattle",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Bob Smith",
                City = "Portland",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Charlie Brown",
                City = "Seattle",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Diana Prince",
                City = "San Francisco",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Edward Norton",
                City = "Los Angeles",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        await _context.Customers.AddRangeAsync(customers);
        _logger.LogInformation("Seeded {Count} customers", customers.Count);
    }

    private async Task SeedOrdersAsync()
    {
        // First save customers and products to get their IDs
        await _context.SaveChangesAsync();

        var customers = await _context.Customers.Take(3).ToListAsync();
        var products = await _context.Products.ToListAsync();

        var orders = new List<OrderModel>();

        // Order 1 for Alice Johnson
        var order1 = new OrderModel
        {
            OrderDate = DateTime.UtcNow.AddDays(-15),
            CustomerId = customers[0].Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            OrderItems = new List<OrderItemModel>
            {
                new()
                {
                    ProductName = products[0].Name, // Laptop Pro 15"
                    Price = products[0].Price,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    ProductName = products[6].Name, // Mechanical Keyboard
                    Price = products[6].Price,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    ProductName = products[7].Name, // Gaming Mouse
                    Price = products[7].Price,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            }
        };

        // Order 2 for Bob Smith
        var order2 = new OrderModel
        {
            OrderDate = DateTime.UtcNow.AddDays(-10),
            CustomerId = customers[1].Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            OrderItems = new List<OrderItemModel>
            {
                new()
                {
                    ProductName = products[1].Name, // Smartphone X
                    Price = products[1].Price,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    ProductName = products[3].Name, // Wireless Headphones
                    Price = products[3].Price,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            }
        };

        // Order 3 for Charlie Brown
        var order3 = new OrderModel
        {
            OrderDate = DateTime.UtcNow.AddDays(-5),
            CustomerId = customers[2].Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            OrderItems = new List<OrderItemModel>
            {
                new()
                {
                    ProductName = products[2].Name, // Tablet Air
                    Price = products[2].Price,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    ProductName = products[4].Name, // Smart Watch
                    Price = products[4].Price,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    ProductName = products[5].Name, // Desktop Monitor 27"
                    Price = products[5].Price,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            }
        };

        orders.AddRange([order1, order2, order3]);

        await _context.Orders.AddRangeAsync(orders);
        _logger.LogInformation("Seeded {Count} orders with order items", orders.Count);
    }
}