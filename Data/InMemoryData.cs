using ODataDemo.Model;

namespace ODataSamples.Data;

/// <summary>
/// In-memory data for demonstration.
/// </summary>
public static class InMemoryData
{
    public static readonly List<ProductModel> Products = new List<ProductModel>
    {
        new ProductModel { Id = 1, Name = "Laptop", Price = 999.99m },
        new ProductModel { Id = 2, Name = "Smartphone", Price = 499.99m },
        new ProductModel { Id = 3, Name = "Tablet", Price = 299.99m },
    };

    public static readonly List<CustomerModel> Customers = new List<CustomerModel>
    {
        new CustomerModel { Id = 1, Name = "Alice", City = "Seattle" },
        new CustomerModel { Id = 2, Name = "Bob", City = "Portland" },
        new CustomerModel { Id = 3, Name = "Charlie", City = "Seattle" }
    };

    public static readonly List<OrderModel> Orders = new List<OrderModel>
    {
        new OrderModel
        {
            Id = 101,
            OrderDate = new DateTime(2025, 1, 15),
            CustomerId = 1, // Link to Alice
            OrderItems = new List<OrderItemModel>
            {
                new OrderItemModel { Id = 1001, ProductName = "Laptop", Price = 999.99m, OrderId = 101 },
                new OrderItemModel { Id = 1002, ProductName = "Mouse", Price = 19.99m, OrderId = 101 }
            }
        },
        new OrderModel
        {
            Id = 102,
            OrderDate = new DateTime(2025, 1, 20),
            CustomerId = 2, // Link to Bob
            OrderItems = new List<OrderItemModel>
            {
                new OrderItemModel { Id = 1003, ProductName = "Keyboard", Price = 49.99m, OrderId = 102 }
            }
        },
        new OrderModel
        {
            Id = 103,
            OrderDate = new DateTime(2025, 2, 10),
            CustomerId = 1, // Link to Alice
            OrderItems = new List<OrderItemModel>
            {
                new OrderItemModel { Id = 1004, ProductName = "Monitor", Price = 199.99m, OrderId = 103 },
                new OrderItemModel { Id = 1005, ProductName = "HDMI Cable", Price = 10.99m, OrderId = 103 }
            }
        }
    };

    public static readonly List<OrderItemModel> OrderItems = new List<OrderItemModel>
    {
        new OrderItemModel { Id = 1001, ProductName = "Laptop", Price = 999.99m, OrderId = 101 },
        new OrderItemModel { Id = 1002, ProductName = "Mouse", Price = 19.99m, OrderId = 101 },
        new OrderItemModel { Id = 1003, ProductName = "Keyboard", Price = 49.99m, OrderId = 102 },
        new OrderItemModel { Id = 1004, ProductName = "Monitor", Price = 199.99m, OrderId = 103 },
        new OrderItemModel { Id = 1005, ProductName = "HDMI Cable", Price = 10.99m, OrderId = 103 },
    };

    static InMemoryData()
    {
        // OPTIONAL: Link navigation properties so $expand works in-memory
        // Link each Order -> Customer
        foreach (var order in Orders)
        {
            order.Customer = Customers.FirstOrDefault(c => c.Id == order.CustomerId) ?? new CustomerModel();
        }

        // Link each Customer -> Orders
        foreach (var customer in Customers)
        {
            customer.Orders = Orders.Where(o => o.CustomerId == customer.Id).ToList();
        }

        // Link each OrderItem -> Order
        foreach (var item in OrderItems)
        {
            item.Order = Orders.FirstOrDefault(o => o.Id == item.OrderId) ?? new OrderModel();
        }

        // Ensure each Order's OrderItems is set
        foreach (var order in Orders)
        {
            if (order.OrderItems == null || !order.OrderItems.Any())
            {
                order.OrderItems = OrderItems.Where(i => i.OrderId == order.Id).ToList();
            }
        }
    }
}