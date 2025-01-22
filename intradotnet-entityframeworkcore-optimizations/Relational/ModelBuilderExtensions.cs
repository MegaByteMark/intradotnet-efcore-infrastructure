using IntraDotNet.EntityFrameworkCore.Optimizations.Interfaces;
using IntraDotNet.EntityFrameworkCore.Optimizations.ValueGenerators;
using Microsoft.EntityFrameworkCore;

namespace IntraDotNet.EntityFrameworkCore.Optimizations.Relational;

public static class ModelBuilderExtensions
{
    public static ModelBuilder EnableAuditable<TEntity>(this ModelBuilder modelBuilder) where TEntity : class, IAuditable
    {
        modelBuilder.EnableCreateAudit<TEntity>();
        modelBuilder.EnableUpdateAudit<TEntity>();
        modelBuilder.EnableSoftDelete<TEntity>();

        return modelBuilder;
    }

    public static ModelBuilder EnableCreateAudit<TEntity>(this ModelBuilder modelBuilder) where TEntity : class, ICreateAuditable
    {
        modelBuilder.Entity<TEntity>().Property(x => x.CreatedOn).ValueGeneratedOnAdd().HasValueGenerator<CurrentDateTimeValueGenerator>();

        return modelBuilder;
    }

    public static ModelBuilder EnableUpdateAudit<TEntity>(this ModelBuilder modelBuilder) where TEntity : class, IUpdateAuditable
    {
        modelBuilder.Entity<TEntity>().Property(x => x.LastUpdateOn).ValueGeneratedOnUpdate().HasValueGenerator<CurrentDateTimeValueGenerator>();

        return modelBuilder;
    }

    public static ModelBuilder EnableSoftDelete<TEntity>(this ModelBuilder modelBuilder) where TEntity : class, ISoftDeleteAuditable
    {
        modelBuilder.Entity<TEntity>().HasIndex(x => x.DeletedOn).HasFilter("[DeletedOn] IS NOT NULL");
        modelBuilder.Entity<TEntity>().HasQueryFilter(x => x.DeletedOn == null);

        return modelBuilder;
    }

    public static ModelBuilder EnableOptimisticConcurrency<TEntity>(this ModelBuilder modelBuilder) where TEntity : class, IRowVersion
    {
        modelBuilder.Entity<TEntity>().Property(x => x.RowVersion).IsRowVersion();

        return modelBuilder;
    }
}
