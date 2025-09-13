using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ODataDemo.Model;

/// <summary>
/// Represents a product in the system
/// </summary>
[Table("Products")]
public class ProductModel
{
    /// <summary>
    /// Gets or sets the unique identifier for the product
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the product
    /// </summary>
    [Required]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Product name must be between 1 and 255 characters")]
    [Column(TypeName = "varchar(255)")]
    public required string Name { get; set; }
    
    /// <summary>
    /// Gets or sets the price of the product
    /// </summary>
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the product was created
    /// </summary>
    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp when the product was last updated
    /// </summary>
    [Column(TypeName = "datetime")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}