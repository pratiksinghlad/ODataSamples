using System.ComponentModel.DataAnnotations;

namespace ODataDemo.Model;

/// <summary>
/// Represents a customer in the system
/// </summary>
public class CustomerModel
{
    /// <summary>
    /// Gets or sets the unique identifier for the customer
    /// </summary>
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the customer
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// Gets or sets the city where the customer is located
    /// </summary>
    public required string City { get; set; }

    /// <summary>
    /// Gets or sets the collection of orders placed by this customer
    /// </summary>
    public virtual ICollection<OrderModel> Orders { get; set; } = [];
}