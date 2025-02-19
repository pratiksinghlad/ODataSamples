namespace ODataDemo.Model;

public class CustomerModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string City { get; set; }

    // Navigation back to Orders
    public virtual ICollection<OrderModel> Orders { get; set; }
}