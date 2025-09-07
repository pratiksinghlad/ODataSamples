using ODataDemo.Model;

namespace ODataSamples.Data;

/// <summary>
/// Provides in-memory data for demonstration purposes
/// </summary>
public static class InMemoryData
{
    /// <summary>
    /// Gets the collection of products
    /// </summary>
    public static readonly List<ProductModel> Products = 
    [
        new() { Id = 1, Name = "Laptop", Price = 999.99m },
        new() { Id = 2, Name = "Smartphone", Price = 499.99m },
        new() { Id = 3, Name = "Tablet", Price = 299.99m },
    ];

    /// <summary>
    /// Gets the collection of customers
    /// </summary>
    public static readonly List<CustomerModel> Customers = 
    [
        new() { Id = 1, Name = "Alice", City = "Seattle" },
        new() { Id = 2, Name = "Bob", City = "Portland" },
        new() { Id = 3, Name = "Charlie", City = "Seattle" }
    ];

    /// <summary>
    /// Gets the collection of orders
    /// </summary>
    public static readonly List<OrderModel> Orders = 
    [
        new()
        {
            Id = 101,
            OrderDate = new DateTime(2025, 1, 15),
            CustomerId = 1, // Link to Alice
            OrderItems = 
            [
                new() { Id = 1001, ProductName = "Laptop", Price = 999.99m, OrderId = 101 },
                new() { Id = 1002, ProductName = "Mouse", Price = 19.99m, OrderId = 101 }
            ]
        },
        new()
        {
            Id = 102,
            OrderDate = new DateTime(2025, 1, 20),
            CustomerId = 2, // Link to Bob
            OrderItems = 
            [
                new() { Id = 1003, ProductName = "Keyboard", Price = 49.99m, OrderId = 102 }
            ]
        },
        new()
        {
            Id = 103,
            OrderDate = new DateTime(2025, 2, 10),
            CustomerId = 1, // Link to Alice
            OrderItems = 
            [
                new() { Id = 1004, ProductName = "Monitor", Price = 199.99m, OrderId = 103 },
                new() { Id = 1005, ProductName = "HDMI Cable", Price = 10.99m, OrderId = 103 }
            ]
        }
    ];

    /// <summary>
    /// Gets the collection of order items
    /// </summary>
    public static readonly List<OrderItemModel> OrderItems = 
    [
        new() { Id = 1001, ProductName = "Laptop", Price = 999.99m, OrderId = 101 },
        new() { Id = 1002, ProductName = "Mouse", Price = 19.99m, OrderId = 101 },
        new() { Id = 1003, ProductName = "Keyboard", Price = 49.99m, OrderId = 102 },
        new() { Id = 1004, ProductName = "Monitor", Price = 199.99m, OrderId = 103 },
        new() { Id = 1005, ProductName = "HDMI Cable", Price = 10.99m, OrderId = 103 },
    ];

    /// <summary>
    /// Static constructor to establish navigation properties for OData support
    /// </summary>
    static InMemoryData()
    {
        // Link navigation properties for OData $expand functionality
        LinkNavigationProperties();
    }

    /// <summary>
    /// Links the navigation properties between entities
    /// </summary>
    private static void LinkNavigationProperties()
    {
        // Link each Order to its Customer
        foreach (var order in Orders)
        {
            order.Customer = Customers.FirstOrDefault(c => c.Id == order.CustomerId);
        }

        // Link each Customer to their Orders
        foreach (var customer in Customers)
        {
            customer.Orders = Orders.Where(o => o.CustomerId == customer.Id).ToList();
        }

        // Link each OrderItem to its Order
        foreach (var item in OrderItems)
        {
            item.Order = Orders.FirstOrDefault(o => o.Id == item.OrderId);
        }

        // Ensure each Order's OrderItems collection is properly populated
        foreach (var order in Orders)
        {
            if (order.OrderItems == null || order.OrderItems.Count == 0)
            {
                order.OrderItems = OrderItems.Where(i => i.OrderId == order.Id).ToList();
            }
        }
    }
}