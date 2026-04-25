using System.ComponentModel.DataAnnotations;

namespace ProductCatalog.Api.Models;

public class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public required string Category { get; set; }
}

public record CreateProductRequest(
    [Required] [MaxLength(100)] string Name,
    [Range(0.01, 10000.00)] decimal Price,
    [Required] [MaxLength(50)] string Category
);

public record UpdateProductRequest(
    [Required] [MaxLength(100)] string Name,
    [Range(0.01, 10000.00)] decimal Price,
    [Required] [MaxLength(50)] string Category
);

public record ProductResponse(
    int Id,
    string Name,
    decimal Price,
    string Category
);
