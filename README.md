# IntraDotNet EntityFrameworkCore Optimizations

Optimization classes to remove boilerplate when generating EntityFramework DbContexts, their associated entities and implementing common functionality like row based change auditing, soft deleting and optimistic concurrency.

## Getting Started

### Prerequisites

- .NET 9.0 SDK or later
- EntityFrameworkCore 9.0 or later

### Installation

1. Add the NuGet package to your project:
    ```sh
    dotnet add package IntraDotNet.EntityFrameworkCore.Optimizations
    ```

### Usage

1. Create your DbContext class inheriting from `DbContext`:
    ```csharp
    public class MyDbContext : DbContext
    {
        public DbSet<MyEntity> MyEntities { get; set; }
    }
    ```

2. Create your repository class inheriting from `BaseAuditableRepository`:
    ```csharp
    public class MyEntityRepository : BaseAuditableRepository<MyEntity, MyDbContext>
    {
        public MyEntityRepository(IDbContextFactory<MyDbContext> contextFactory) : base(contextFactory)
        {
        }
    }
    ```

3. Use the repository in your application:
    ```csharp
    var repository = new MyEntityRepository(contextFactory);
    var entities = await repository.GetAllAsync();
    ```

## Contributing

Contributions are welcome! Please submit a pull request on GitHub.

1. Fork the repository.
2. Create a new branch (`git checkout -b feature-branch`).
3. Make your changes.
4. Commit your changes (`git commit -am 'Add new feature'`).
5. Push to the branch (`git push origin feature-branch`).
6. Create a new Pull Request.

## License

This project is licensed under the MIT License. See the [LICENSE](http://_vscodecontentref_/1) file for details.
