namespace ODataDemo.Model;

public class OrderModel
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }

    // Foreign Key
    public int CustomerId { get; set; }

    // Navigation
    public virtual CustomerModel Customer { get; set; }

    // Navigation to OrderItems
    public virtual ICollection<OrderItemModel> OrderItems { get; set; }
}