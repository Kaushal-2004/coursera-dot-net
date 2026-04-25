using Microsoft.EntityFrameworkCore;
using ProductCatalog.Api.Models;

namespace ProductCatalog.Api.Data;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<User> Users => Set<User>();
}
