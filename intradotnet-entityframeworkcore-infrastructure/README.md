# IntraDotNet.EntityFrameworkCore.Infrastructure

A comprehensive .NET library that provides optimisation classes and infrastructure to reduce boilerplate code when working with Entity Framework Core. This library implements common patterns including Unit of Work, Repository Pattern, auditing, soft deleting, optimistic concurrency, and validation.

## Features

- **Unit of Work Pattern**: Coordinate multiple repository operations within a single transaction
- **Repository Pattern**: Generic base repositories with common CRUD operations
- **Audit Trail**: Automatic tracking of created/modified dates and users
- **Soft Delete**: Mark entities as deleted without physical removal
- **Optimistic Concurrency**: Row version-based concurrency control
- **Validation**: Integrated entity validation using DataAnnotations and custom logic
- **Lazy Loading**: DbContext factory pattern for efficient resource management

## Getting Started

### Prerequisites

- .NET 9.0 SDK or later
- EntityFrameworkCore 9.0 or later

### Installation

1. Add the NuGet package to your project:
    ```sh
    dotnet add package IntraDotNet.EntityFrameworkCore.Infrastructure
    ```

2. Register services in your DI container, this example uses SqlServer as the persisence layer with EntityFramwork Core, other persistence layers are available:
    ```csharp
    services.AddDbContextFactory<UserDbContext>(options =>
        options.UseSqlServer(connectionString));
    services.AddScoped<UserUnitOfWork>();
    services.AddScoped<UserRepository>();
    ```

## Quick Start

### 1. Create Your Entity

```csharp
namespace Example.Models;

public class User : IAuditable, IRowVersion
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Audit fields (automatically managed)
    public DateTime? Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastUpdateOn { get; set; }
    public string? LastUpdateBy { get; set; }

    // Soft delete fields (automatically managed)
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // Concurrency field (automatically managed)
    public byte[]? RowVersion { get; set; };
}
```

Add the Entity to your DbContext

```csharp
using Example.Models;

namespace Example.DbContext;

public class UserDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbSet<User> Users { get; set; }

    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //code emitted for brevity...

        modelBuilder.UseAuditable<User>();
        modelBuilder.UseOptimisticConcurrency<User>();
    }
```

### 2. Create Your Repository

```csharp
using IntraDotNet.EntityFrameworkCore.Infrastructure.Repositories;

namespace Example.Respositories;

public class UserRepository : BaseAuditableRepository<User, UserDbContext>, IUserRepository
{
    public UserRepository(UserDbContext context) : base(context)
    {
    }

    // All CRUD operations inherited automatically
    // Custom methods can be added here
}
```

### 3. Create Your Unit of Work

```csharp
public class UserUnitOfWork : UnitOfWork<UserDbContext>
{
    public UserUnitOfWork(IDbContextFactory<UserDbContext> contextFactory) 
        : base(contextFactory) { }

    protected override TRepository CreateRepository<TRepository>()
    {
        return typeof(TRepository).Name switch
        {
            nameof(IUserRepository) => (TRepository)(object)new UserRepository(Context),
            //add more repositories here if required...
            _ => throw new NotSupportedException($"Repository type not supported")
        };
    }

    // Easy access to repositories
    public IUserRepository Users => GetRepository<IUserRepository>();
    //add more repositories here if required...
}
```

## Usage Examples

### Basic CRUD Operations
Inject a UnitOfWork object from DI container.

```csharp
public class UserService
{
    private readonly UserUnitOfWork _unitOfWork;

    public UserService(UserUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Create a new user
    public async Task<User> CreateUserAsync(string name, string email)
    {
        var user = new User { Name = name, Email = email };
        _unitOfWork.Users.Add(user);

        await _unitOfWork.SaveChangesAsync(); // Audit fields set automatically
        
        return user;
    }

    // Get all active users (excludes soft-deleted)
    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _unitOfWork.Users.GetAllAsync(includeDeleted: false);
    }

    // Update a user
    public async Task UpdateUserAsync(int userId, string newName)
    {
        var user = await _unitOfWork.Users.FindAsync(u => u.Id == userId);
        if (user.Any())
        {
            var userToUpdate = user.First();
            userToUpdate.Name = newName;
            _unitOfWork.Users.Update(userToUpdate);
            await _unitOfWork.SaveChangesAsync(); // LastModified fields updated automatically
        }
    }

    // Soft delete a user
    public async Task DeleteUserAsync(int userId)
    {
        await _unitOfWork.Users.DeleteAsync(u => u.Id == userId);
        await _unitOfWork.SaveChangesAsync(); // IsDeleted set to true, DeletedDate/By set
    }
}
```

### Advanced Querying

```csharp
public class UserService
{
    // Search users with flexible options
    public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm)
    {
        return await _unitOfWork.Users.FindAsync(
            u => u.Name.Contains(searchTerm) || u.Email.Contains(searchTerm),
            withIncludes: true,      // Include navigation properties
            asNoTracking: true,      // Don't track changes (read-only)
            includeDeleted: false    // Exclude soft-deleted users
        );
    }

    // Get users created in the last 30 days
    public async Task<IEnumerable<User>> GetRecentUsersAsync()
    {
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        return await _unitOfWork.Users.FindAsync(
            u => u.Created >= thirtyDaysAgo,
            asNoTracking: true
        );
    }

    // Find users including deleted ones
    public async Task<IEnumerable<User>> GetAllUsersIncludingDeletedAsync()
    {
        return await _unitOfWork.Users.GetAllAsync(includeDeleted: true);
    }
}
```

### Transaction Management

```csharp
public class OrderService
{
    private readonly MyUnitOfWork _unitOfWork;

    // Transfer orders from one user to another
    public async Task TransferOrdersAsync(int fromUserId, int toUserId)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            // Update all orders
            var orders = await _unitOfWork.Orders.FindAsync(o => o.UserId == fromUserId);
            foreach (var order in orders)
            {
                order.UserId = toUserId;
                _unitOfWork.Orders.Update(order);
            }

            // Deactivate original user
            await _unitOfWork.Users.DeleteAsync(u => u.Id == fromUserId);

            // Save all changes atomically
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // Create order with inventory check
    public async Task<Order> CreateOrderWithInventoryCheckAsync(Order order)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            // Check inventory
            var product = await _unitOfWork.Products.FindAsync(p => p.Id == order.ProductId);
            var productEntity = product.FirstOrDefault();
            
            if (productEntity?.Stock < order.Quantity)
                throw new InvalidOperationException("Insufficient stock");

            // Update inventory
            productEntity.Stock -= order.Quantity;
            _unitOfWork.Products.Update(productEntity);

            // Create order
            _unitOfWork.Orders.Add(order);

            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            
            return order;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```

### Validation and Error Handling

```csharp
public class UserService
{
    // Create user with validation
    public async Task<User> CreateValidatedUserAsync(User user)
    {
        try
        {
            _unitOfWork.Users.Add(user);
            await _unitOfWork.SaveChangesAsync(); // Validation runs automatically
            return user;
        }
        catch (ValidationException ex)
        {
            throw new BusinessException($"User creation failed: {ex.Message}");
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new BusinessException("Another user modified this record. Please refresh and try again.");
        }
    }

    // Bulk operations with validation
    public async Task<List<User>> CreateMultipleUsersAsync(List<User> users)
    {
        var createdUsers = new List<User>();
        
        using var transaction = await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            foreach (var user in users)
            {
                _unitOfWork.Users.Add(user);
            }

            await _unitOfWork.SaveChangesAsync(); // All users validated at once
            await transaction.CommitAsync();
            
            return users;
        }
        catch (ValidationException ex)
        {
            await transaction.RollbackAsync();
            throw new BusinessException($"Batch user creation failed: {ex.Message}");
        }
    }
}
```

### Working with Concurrency

```csharp
public class ProductService
{
    // Update with optimistic concurrency handling
    public async Task<Product> UpdateProductPriceAsync(int productId, decimal newPrice, byte[] rowVersion)
    {
        try
        {
            var products = await _unitOfWork.Products.FindAsync(p => p.Id == productId);
            var product = products.FirstOrDefault();
            
            if (product == null)
                throw new NotFoundException("Product not found");

            // Check row version for concurrency
            if (!product.RowVersion.SequenceEqual(rowVersion))
                throw new ConcurrencyException("Product was modified by another user");

            product.Price = newPrice;
            _unitOfWork.Products.Update(product);
            
            await _unitOfWork.SaveChangesAsync(); // Row version updated automatically
            return product;
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException("Concurrency conflict detected. Please refresh and try again.");
        }
    }
}
```

## Key Benefits

- **Automatic Auditing**: Created/Modified timestamps and user tracking
- **Safe Deletions**: Soft delete prevents accidental data loss
- **Concurrency Protection**: Built-in optimistic concurrency control
- **Transaction Coordination**: Multiple operations in single transaction
- **Validation Integration**: Automatic entity validation
- **Flexible Querying**: Include/exclude deleted, tracking options
- **Reduced Boilerplate**: Common patterns implemented once

## Contributing

Contributions are welcome! Please submit a pull request on GitHub.

1. Fork the repository
2. Create a new branch (`git checkout -b feature-branch`)
3. Make your changes
4. Commit your changes (`git commit -am 'Add new feature'`)
5. Push to the branch (`git push origin feature-branch`)
6. Create a new Pull Request

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
