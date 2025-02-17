namespace ODataDemo.Model;

public class OrderItem
{
    public int Id { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }

    // Navigation back to Order
    public int OrderId { get; set; }

    public OrderModel Order { get; set; }
}