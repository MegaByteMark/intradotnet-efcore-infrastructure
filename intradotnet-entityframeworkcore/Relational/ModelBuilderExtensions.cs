using IntraDotNet.EntityFrameworkCore.Interfaces;
using IntraDotNet.EntityFrameworkCore.ValueGenerators;
using Microsoft.EntityFrameworkCore;

namespace IntraDotNet.EntityFrameworkCore.Relational;

public static class ModelBuilderExtensions
{
    public static ModelBuilder UseAuditable<TEntity>(this ModelBuilder modelBuilder) where TEntity : class, IAuditable
    {
        modelBuilder.UseCreateAudit<TEntity>();
        modelBuilder.UseUpdateAudit<TEntity>();
        modelBuilder.UseSoftDelete<TEntity>();

        return modelBuilder;
    }

    public static ModelBuilder UseCreateAudit<TEntity>(this ModelBuilder modelBuilder) where TEntity : class, ICreateAuditable
    {
        modelBuilder.Entity<TEntity>().Property(x => x.CreatedOn).ValueGeneratedOnAdd().HasValueGenerator<CurrentDateTimeValueGenerator>();

        return modelBuilder;
    }

    public static ModelBuilder UseUpdateAudit<TEntity>(this ModelBuilder modelBuilder) where TEntity : class, IUpdateAuditable
    {
        modelBuilder.Entity<TEntity>().Property(x => x.LastUpdateOn).ValueGeneratedOnUpdate().HasValueGenerator<CurrentDateTimeValueGenerator>();

        return modelBuilder;
    }

    /// <summary>
    /// Enables soft delete for the entity.
    /// WARNING: This method will NOT apply a global query filter to the entity to filter out soft deleted records. 
    /// This is dependent on the calling client to filter out soft deleted records until EF Core adds support for named query filters.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="modelBuilder">The model builder being extended.</param>
    /// <returns>The model builder with soft delete enabled for the entity.</returns>
    public static ModelBuilder UseSoftDelete<TEntity>(this ModelBuilder modelBuilder) where TEntity : class, ISoftDeleteAuditable
    {
        modelBuilder.Entity<TEntity>().HasIndex(x => x.DeletedOn).HasFilter("DeletedOn IS NOT NULL");

        // As of the current date (23-Jan-2025) the current version of EF Core does not support the HasQueryFilter method with a name parameter.
        // this means that when using the HasQueryFilter method, the filter will be applied to all queries that are executed on the entity.
        // This is not always the desired behavior, you may have 3 global query filters, the soft delete filtering being one, and you want to ignore the soft delete 
        // but maintain the other filters, this is not currently possible.
        // There is an open PR to add this feature to EF Core, but it has not been merged yet. https://github.com/dotnet/efcore/pull/35104
        // Therefore, to work around this limitation, this code will be commented out until that feature is in place, and the calling client will be responsible for 
        // filtering out the soft deleted records, or alternatively, by inheriting from the BaseAuditableRepository class for their repository, we will manually add the query filter as part of GetQueryable until the EF Core fix is in place.
        /* 
            modelBuilder.Entity<TEntity>().HasQueryFilter(x => x.DeletedOn == null);
        */

        return modelBuilder;
    }

    public static ModelBuilder UseOptimisticConcurrency<TEntity>(this ModelBuilder modelBuilder) where TEntity : class, IRowVersion
    {
        modelBuilder.Entity<TEntity>().Property(x => x.RowVersion).IsRowVersion();

        return modelBuilder;
    }
}
