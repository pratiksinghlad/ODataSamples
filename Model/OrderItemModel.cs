using System.ComponentModel.DataAnnotations;

namespace ODataDemo.Model;

/// <summary>
/// Represents an item within an order
/// </summary>
public class OrderItemModel
{
    /// <summary>
    /// Gets or sets the unique identifier for the order item
    /// </summary>
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the product in this order item
    /// </summary>
    public required string ProductName { get; set; }
    
    /// <summary>
    /// Gets or sets the price of the item
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the order this item belongs to
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Gets or sets the order this item belongs to
    /// </summary>
    public virtual OrderModel? Order { get; set; }
}