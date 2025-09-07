using System.ComponentModel.DataAnnotations;

namespace ODataDemo.Model;

/// <summary>
/// Represents an order in the system
/// </summary>
public class OrderModel
{
    /// <summary>
    /// Gets or sets the unique identifier for the order
    /// </summary>
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the date when the order was placed
    /// </summary>
    public DateTime OrderDate { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the customer who placed this order
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the customer who placed this order
    /// </summary>
    public virtual CustomerModel? Customer { get; set; }

    /// <summary>
    /// Gets or sets the collection of items in this order
    /// </summary>
    public virtual ICollection<OrderItemModel> OrderItems { get; set; } = [];
}