using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ODataDemo.Model;

/// <summary>
/// Represents an order in the system
/// </summary>
[Table("Orders")]
public class OrderModel
{
    /// <summary>
    /// Gets or sets the unique identifier for the order
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the date when the order was placed
    /// </summary>
    [Required]
    [Column(TypeName = "datetime")]
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the identifier of the customer who placed this order
    /// </summary>
    [Required]
    [ForeignKey(nameof(Customer))]
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the order was created
    /// </summary>
    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp when the order was last updated
    /// </summary>
    [Column(TypeName = "datetime")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the customer who placed this order
    /// </summary>
    [ForeignKey(nameof(CustomerId))]
    [InverseProperty(nameof(CustomerModel.Orders))]
    public virtual CustomerModel? Customer { get; set; }

    /// <summary>
    /// Gets or sets the collection of items in this order
    /// </summary>
    [InverseProperty(nameof(OrderItemModel.Order))]
    public virtual ICollection<OrderItemModel> OrderItems { get; set; } = [];

    /// <summary>
    /// Gets the total value of the order (calculated property)
    /// </summary>
    [NotMapped]
    public decimal TotalValue => OrderItems?.Sum(item => item.Price) ?? 0m;

    /// <summary>
    /// Gets the number of items in the order (calculated property)
    /// </summary>
    [NotMapped]
    public int ItemCount => OrderItems?.Count ?? 0;
}