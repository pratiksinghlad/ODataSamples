using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ODataDemo.Model;

/// <summary>
/// Represents a customer in the system
/// </summary>
[Table("Customers")]
public class CustomerModel
{
    /// <summary>
    /// Gets or sets the unique identifier for the customer
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the customer
    /// </summary>
    [Required]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Customer name must be between 1 and 255 characters")]
    [Column(TypeName = "varchar(255)")]
    public required string Name { get; set; }
    
    /// <summary>
    /// Gets or sets the city where the customer is located
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "City must be between 1 and 100 characters")]
    [Column(TypeName = "varchar(100)")]
    public required string City { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the customer was created
    /// </summary>
    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp when the customer was last updated
    /// </summary>
    [Column(TypeName = "datetime")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the collection of orders placed by this customer
    /// </summary>
    [InverseProperty(nameof(OrderModel.Customer))]
    public virtual ICollection<OrderModel> Orders { get; set; } = [];
}