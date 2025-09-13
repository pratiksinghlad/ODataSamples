using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ODataDemo.Model;

/// <summary>
/// Represents an item within an order
/// </summary>
[Table("OrderItems")]
public class OrderItemModel
{
    /// <summary>
    /// Gets or sets the unique identifier for the order item
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the product in this order item
    /// </summary>
    [Required]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Product name must be between 1 and 255 characters")]
    [Column(TypeName = "varchar(255)")]
    public required string ProductName { get; set; }
    
    /// <summary>
    /// Gets or sets the price of the item
    /// </summary>
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the order this item belongs to
    /// </summary>
    [Required]
    [ForeignKey(nameof(Order))]
    public int OrderId { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the order item was created
    /// </summary>
    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp when the order item was last updated
    /// </summary>
    [Column(TypeName = "datetime")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the order this item belongs to
    /// </summary>
    [ForeignKey(nameof(OrderId))]
    [InverseProperty(nameof(OrderModel.OrderItems))]
    public virtual OrderModel? Order { get; set; }
}