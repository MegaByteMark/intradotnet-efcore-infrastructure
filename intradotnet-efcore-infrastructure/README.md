# IntraDotNet.EFCore.Infrastructure

A comprehensive .NET library that eliminates boilerplate code when working with Entity Framework Core. This library provides optimisation classes for generating EntityFramework DbContexts, their associated entities, and implementing common functionality like row-based change auditing, soft deleting, optimistic concurrency, repository patterns, and unit of work patterns.

## Features

- **Auditable Entities**: Automatic tracking of creation and update timestamps
- **Soft Delete**: Mark records as deleted without physically removing them from the database
- **Optimistic Concurrency**: Built-in row versioning for concurrent access control
- **Repository Pattern**: Base repository implementations with common CRUD operations
- **Unit of Work Pattern**: Coordinated transaction management across multiple repositories
- **Result Pattern**: Structured error handling and operation results
- **Service Layer**: Base service classes with validation support
- **Model Builder Extensions**: Fluent API extensions for easy EF Core configuration

## Getting Started

### Prerequisites

- .NET 9.0 SDK or later
- Entity Framework Core 9.0 or later
- IntraDotNet.Infrastructure.Core (>=1.0.0)

### Installation

Add the NuGet package to your project:

```bash
dotnet add package IntraDotNet.EFCore.Infrastructure
```

## Usage

### 1. Create Your Entity

```csharp
using IntraDotNet.Insfrastructure.Core;

public class Product : IAuditable, IRowVersion
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    
    // Auditable properties
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset LastUpdateOn { get; set; }
    public DateTimeOffset? DeletedOn { get; set; }
    
    // Optimistic concurrency
    public byte[] RowVersion { get; set; }
}
```

### 2. Configure Your DbContext

```csharp
public class MyDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure auditing, soft delete, and optimistic concurrency
        modelBuilder.UseAuditable<Product>();
        modelBuilder.UseOptimisticConcurrency<Product>();
        
        // Or configure individually:
        // modelBuilder.UseCreateAudit<Product>();
        // modelBuilder.UseUpdateAudit<Product>();
        // modelBuilder.UseSoftDelete<Product>();
        
        base.OnModelCreating(modelBuilder);
    }
}
```

### 3. Create Your Repository

```csharp
using IntraDotNet.EFCore.Infrastructure.Repositories;

public class ProductRepository : BaseAuditableRepository<Product, MyDbContext>
{
    public ProductRepository(IDbContextFactory<MyDbContext> contextFactory) 
        : base(contextFactory)
    {
    }
    
    // Add custom repository methods here
    public async Task<IEnumerable<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        using var context = await GetContextAsync();
        return await GetQueryable(context)
            .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
            .ToListAsync();
    }
}
```

### 4. Use Repository in Your Application

```csharp
// Using repository directly
var repository = new ProductRepository(contextFactory);

// Create a new product
var newProduct = new Product { Name = "Laptop", Price = 999.99m };
await repository.AddOrUpdateAsync(newProduct);

// Get all active products (soft-deleted records are automatically filtered out)
var products = await repository.GetAllAsync();

// Update a product
var product = await repository.GetByIdAsync(1);
if (product != null)
{
    product.Price = 899.99m;
    await repository.AddOrUpdateAsync(product);
}

// Soft delete a product
await repository.DeleteAsync(1);
```

### 5. Using with Services

```csharp
using IntraDotNet.Services;
using IntraDotNet.Results;

public class ProductService : BaseValidatableService<Product>
{
    private readonly ProductRepository _repository;
    
    public ProductService(ProductRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<ValueResult<Product>> CreateProductAsync(string name, decimal price)
    {
        var product = new Product { Name = name, Price = price };
        
        var validationResult = await ValidateAsync(product);
        if (!validationResult.IsSuccess)
            return ValueResult<Product>.Failure(validationResult.Errors);
            
        await _repository.AddAsync(product);
        return ValueResult<Product>.Success(product);
    }
}
```

## Model Builder Extensions

The library provides several extension methods for `ModelBuilder` to easily configure entity behaviors:

### UseAuditable<TEntity>()
Configures an entity with full auditing support (creation, update, and soft delete tracking).

### UseCreateAudit<TEntity>()
Configures automatic creation timestamp tracking using [`CurrentDateTimeValueGenerator`](ValueGenerators/CurrentDateTimeValueGenerator.cs).

### UseUpdateAudit<TEntity>()
Configures automatic update timestamp tracking.

### UseSoftDelete<TEntity>()
Configures soft delete functionality with database indexing for performance.

**Note**: Due to current EF Core limitations with named query filters, soft-deleted records must be filtered manually or by using the provided repository base classes.

### UseOptimisticConcurrency<TEntity>()
Configures row versioning for optimistic concurrency control.

## Value Generators

### CurrentDateTimeValueGenerator
Automatically generates `DateTimeOffset.UtcNow` values for auditable timestamp fields.

## Repository Pattern

The library provides base repository classes:

- `BaseRepository<TEntity, TContext>`: Basic CRUD operations
- `BaseAuditableRepository<TEntity, TContext>`: Includes automatic soft-delete filtering

## Important Notes

### Soft Delete Behavior
Due to current Entity Framework Core limitations with named query filters, the `UseSoftDelete` extension method does **not** automatically apply a global query filter. The provided repository base classes handle this filtering, or you can implement it manually in your queries.

There is an [open pull request](https://github.com/dotnet/efcore/pull/35104) in EF Core to add support for named query filters, which will enable more flexible soft delete filtering in the future.

## License

This project is licensed under the MIT License. See the [LICENSE](../LICENSE) file for details.

## Contributing

Contributions are welcome! Please submit a pull request on GitHub.

1. Fork the repository
2. Create a new branch (`git checkout -b feature-branch`)
3. Make your changes
4. Commit your changes (`git commit -am 'Add new feature'`)
5. Push to the branch (`git push origin feature-branch`)
6. Create a new Pull Request