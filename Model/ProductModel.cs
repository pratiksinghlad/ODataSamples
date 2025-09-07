using System.ComponentModel.DataAnnotations;

namespace ODataDemo.Model;

/// <summary>
/// Represents a product in the system
/// </summary>
public class ProductModel
{
    /// <summary>
    /// Gets or sets the unique identifier for the product
    /// </summary>
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the product
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// Gets or sets the price of the product
    /// </summary>
    public decimal Price { get; set; }
}