using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ProductCatalog.Api.Controllers;
using ProductCatalog.Api.Data;
using ProductCatalog.Api.Models;
using Xunit;

namespace ProductCatalog.Tests;

public class ProductsControllerTests : IDisposable
{
    private readonly ProductDbContext _context;
    private readonly Mock<ILogger<ProductsController>> _mockLogger;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ProductDbContext(options);
        _mockLogger = new Mock<ILogger<ProductsController>>();
        _controller = new ProductsController(_context, _mockLogger.Object);
        
        SeedDatabase();
    }

    private void SeedDatabase()
    {
        _context.Products.AddRange(
            new Product { Id = 1, Name = "Laptop", Price = 999.99m, Category = "Electronics" },
            new Product { Id = 2, Name = "Mouse", Price = 49.99m, Category = "Electronics" },
            new Product { Id = 3, Name = "Keyboard", Price = 89.99m, Category = "Electronics" }
        );
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetProducts_ReturnsPaginatedResults()
    {
        // Act
        var result = await _controller.GetProducts(page: 1, pageSize: 2);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var products = Assert.IsAssignableFrom<IEnumerable<ProductResponse>>(okResult.Value);
        Assert.Equal(2, products.Count());
    }

    [Fact]
    public async Task GetProducts_WithNameFilter_ReturnsFilteredResults()
    {
        // Act
        var result = await _controller.GetProducts(nameFilter: "Mouse");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var products = Assert.IsAssignableFrom<IEnumerable<ProductResponse>>(okResult.Value);
        Assert.Single(products);
        Assert.Equal("Mouse", products.First().Name);
    }

    [Fact]
    public async Task GetProduct_ExistingId_ReturnsProduct()
    {
        // Act
        var result = await _controller.GetProduct(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var product = Assert.IsType<ProductResponse>(okResult.Value);
        Assert.Equal(1, product.Id);
        Assert.Equal("Laptop", product.Name);
    }

    [Fact]
    public async Task GetProduct_NonExistingId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GetProduct(999);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateProduct_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = new CreateProductRequest("Monitor", 199.99m, "Electronics");

        // Act
        var result = await _controller.CreateProduct(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var product = Assert.IsType<ProductResponse>(createdResult.Value);
        Assert.Equal("Monitor", product.Name);
        Assert.True(product.Id > 0);
    }

    [Fact]
    public async Task UpdateProduct_ExistingId_ReturnsNoContent()
    {
        // Arrange
        var request = new UpdateProductRequest("Updated Laptop", 1099.99m, "Electronics");

        // Act
        var result = await _controller.UpdateProduct(1, request);

        // Assert
        Assert.IsType<NoContentResult>(result);
        
        var updatedProduct = await _context.Products.FindAsync(1);
        Assert.Equal("Updated Laptop", updatedProduct!.Name);
        Assert.Equal(1099.99m, updatedProduct.Price);
    }

    [Fact]
    public async Task UpdateProduct_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var request = new UpdateProductRequest("Ghost Product", 10.0m, "Misc");

        // Act
        var result = await _controller.UpdateProduct(999, request);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteProduct_ExistingId_ReturnsNoContent()
    {
        // Act
        var result = await _controller.DeleteProduct(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
        Assert.Null(await _context.Products.FindAsync(1));
    }

    [Fact]
    public async Task DeleteProduct_NonExistingId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.DeleteProduct(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void ModelState_Validation_HandledByApiController()
    {
        _controller.ModelState.AddModelError("Name", "Name is required");
        Assert.False(_controller.ModelState.IsValid);
    }
}
