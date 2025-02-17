namespace ODataDemo.Model;

public class OrderModel
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }

    // Navigation to Customer
    public int CustomerId { get; set; }

    public CustomerModel Customer { get; set; }

    // Navigation to OrderItems
    public ICollection<OrderItem> OrderItems { get; set; }
}