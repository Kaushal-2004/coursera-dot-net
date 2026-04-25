using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductCatalog.Api.Data;
using ProductCatalog.Api.Models;

namespace ProductCatalog.Api.Controllers;

// Handles product-related HTTP requests.
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly ProductDbContext _dbContext;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(ProductDbContext dbContext, ILogger<ProductsController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    // Returns a paginated list of products. Can optionally filter by product name.
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? nameFilter = null)
    {
        _logger.LogInformation("Getting products page {Page} with size {PageSize}", page, pageSize);

        IQueryable<Product> productsQuery = _dbContext.Products.AsQueryable();

        // If the user provided a name filter, apply it here
        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            productsQuery = productsQuery.Where(product => product.Name.Contains(nameFilter));
        }

        int skipAmount = (page - 1) * pageSize;

        // Fetch from DB and map to the response model
        List<ProductResponse> pagedProducts = await productsQuery
            .Skip(skipAmount)
            .Take(pageSize)
            .Select(product => new ProductResponse(product.Id, product.Name, product.Price, product.Category))
            .ToListAsync();

        return Ok(pagedProducts);
    }

    // Fetches a single product by its unique ID.
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetProduct(int id)
    {
        _logger.LogInformation("Getting product with ID {Id}", id);

        Product? existingProduct = await _dbContext.Products.FindAsync(id);

        // If product not found, return 404 Not Found.
        if (existingProduct is null)
        {
            _logger.LogWarning("Product with ID {Id} not found", id);
            return NotFound();
        }

        ProductResponse response = new ProductResponse(
            existingProduct.Id, 
            existingProduct.Name, 
            existingProduct.Price, 
            existingProduct.Category);

        return Ok(response);
    }

    // Creates a new product in the catalog.
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductResponse>> CreateProduct([FromBody] CreateProductRequest request)
    {
        _logger.LogInformation("Creating new product with name {Name}", request.Name);

        Product newProduct = new Product
        {
            Name = request.Name,
            Price = request.Price,
            Category = request.Category
        };

        _dbContext.Products.Add(newProduct);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Created product with ID {Id}", newProduct.Id);

        ProductResponse response = new ProductResponse(
            newProduct.Id, 
            newProduct.Name, 
            newProduct.Price, 
            newProduct.Category);

        return CreatedAtAction(nameof(GetProduct), new { id = newProduct.Id }, response);
    }

    // Updates an existing product using the provided ID.
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
    {
        _logger.LogInformation("Updating product with ID {Id}", id);

        Product? existingProduct = await _dbContext.Products.FindAsync(id);
        
        // Return 404 if trying to update a product that doesn't exist
        if (existingProduct is null)
        {
            _logger.LogWarning("Product with ID {Id} not found for update", id);
            return NotFound();
        }

        // Apply changes
        existingProduct.Name = request.Name;
        existingProduct.Price = request.Price;
        existingProduct.Category = request.Category;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Updated product with ID {Id}", id);
        return NoContent();
    }

    // Removes a product from the database.
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        _logger.LogInformation("Deleting product with ID {Id}", id);

        Product? productToDelete = await _dbContext.Products.FindAsync(id);
        
        // Return 404 if the product is already gone
        if (productToDelete is null)
        {
            _logger.LogWarning("Product with ID {Id} not found for deletion", id);
            return NotFound();
        }

        _dbContext.Products.Remove(productToDelete);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Deleted product with ID {Id}", id);
        return NoContent();
    }
}
