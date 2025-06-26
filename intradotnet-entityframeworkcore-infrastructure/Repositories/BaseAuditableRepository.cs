using System.Data;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using IntraDotNet.EntityFrameworkCore.Infrastructure.Interfaces;

namespace IntraDotNet.EntityFrameworkCore.Infrastructure.Repositories;

/// <summary>
/// Abstract base repository class for handling auditable entities.
/// Implements Repository pattern focused on data access operations.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
public abstract class BaseAuditableRepository<TEntity, TDbContext> : IBaseAuditableRepository<TEntity>
 where TDbContext : DbContext
 where TEntity : class, IAuditable
{
    private readonly TDbContext _context;

    protected BaseAuditableRepository(TDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets the database context.
    /// </summary>
    protected TDbContext Context => _context;

    /// <summary>
    /// Adds includes to the query. Override this method to add includes to the query in each concrete repository.
    /// </summary>
    /// <param name="query">The query to which includes will be added.</param>
    /// <returns>The query with includes added.</returns>
    protected virtual IQueryable<TEntity> AddIncludes(IQueryable<TEntity> query)
    {
        return query;
    }

    /// <summary>
    /// Gets the queryable for the entity.
    /// </summary>
    /// <param name="withIncludes">Whether to include related entities.</param>
    /// <param name="asNoTracking">Whether to track the entity.</param>
    /// <param name="includeDeleted">Whether to include soft deleted entities.</param>
    /// <returns>The queryable for the entity.</returns>
    public virtual IQueryable<TEntity> GetQueryable(bool withIncludes = true, bool asNoTracking = true, bool includeDeleted = false)
    {
        IQueryable<TEntity> query = asNoTracking ? Context.Set<TEntity>().AsNoTracking() : Context.Set<TEntity>();

        // As of the current date (23-Jan-2025) the current version of EF Core does not support the HasQueryFilter method with a name parameter.
        // this means that when using the HasQueryFilter method, the filter will be applied to all queries that are executed on the entity.
        // This is not always the desired behavior, you may have 3 global query filters, the soft delete filtering being one, and you want to ignore the soft delete but keep the others.
        // This logic is commented out here until the feature becomes available in EF Core.
        // In the meantime, we will manually apply a "query filter" here manually.
        /*if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }*/

        if (!includeDeleted)
        {
            // Apply soft delete filter.
            query = query.Where(x => x.DeletedOn == null);
        }

        return withIncludes ? AddIncludes(query) : query;
    }

    /// <summary>
    /// Asynchronously gets an entity that matches the specified identity predicate.
    /// </summary>
    /// <param name="identityPredicate">The predicate to identify the entity.</param>
    /// <param name="withIncludes">Whether to include related entities.</param>
    /// <param name="asNoTracking">Whether to track the entity.</param>
    /// <param name="includeDeleted">Whether to include soft deleted entities.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the entity if found; otherwise, null.</returns>
    public virtual async ValueTask<TEntity?> GetAsync(Expression<Func<TEntity, bool>> identityPredicate, bool withIncludes = true, bool asNoTracking = true, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        return (await FindAsync(identityPredicate, withIncludes, asNoTracking, includeDeleted, cancellationToken)).FirstOrDefault();
    }

    /// <summary>
    /// Asynchronously gets all entities.
    /// </summary>
    /// <param name="withIncludes">Whether to include related entities.</param>
    /// <param name="asNoTracking">Whether to track the entity.</param>
    /// <param name="includeDeleted">Whether to include soft deleted entities.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable of entities.</returns>
    public virtual async ValueTask<IEnumerable<TEntity>> GetAllAsync(bool withIncludes = true, bool asNoTracking = true, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        return await GetQueryable(withIncludes, asNoTracking, includeDeleted).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously finds entities that match the specified predicate.
    /// </summary>
    /// <param name="wherePredicate">The predicate to filter the entities.</param>
    /// <param name="withIncludes">Whether to include related entities.</param>
    /// <param name="asNoTracking">Whether to track the entity.</param>
    /// <param name="includeDeleted">Whether to include soft deleted entities.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable of entities that match the predicate.</returns>
    public virtual async ValueTask<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> wherePredicate, bool withIncludes = true, bool asNoTracking = true, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        return await GetQueryable(withIncludes, asNoTracking, includeDeleted).Where(wherePredicate).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously adds or updates an entity based on the specified identity predicate.
    /// </summary>
    /// <param name="value">The entity to add or update.</param>
    /// <param name="identityPredicate">The predicate to identify the entity.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public virtual async ValueTask AddOrUpdateAsync(TEntity value, Expression<Func<TEntity, bool>> identityPredicate, CancellationToken cancellationToken = default)
    {
        TEntity? existing;
        DbSet<TEntity> dbSet;

        dbSet = Context.Set<TEntity>();
        existing = await dbSet.SingleOrDefaultAsync(identityPredicate, cancellationToken);

        if (existing != null)
        {
            Context.Entry(existing).CurrentValues.SetValues(value);

            // Undelete if row was soft deleted.
            existing.DeletedOn = null;
            existing.DeletedBy = null;

            // Can't set these properties on an update.
            dbSet.Entry(existing).Property(x => x.CreatedOn).IsModified = false;
            dbSet.Entry(existing).Property(x => x.CreatedBy).IsModified = false;
        }
        else
        {
            dbSet.Add(value);

            // Can't set these properties on a new entity.
            dbSet.Entry(value).Property(x => x.LastUpdateOn).IsModified = false;
            dbSet.Entry(value).Property(x => x.LastUpdateBy).IsModified = false;
            dbSet.Entry(value).Property(x => x.DeletedOn).IsModified = false;
            dbSet.Entry(value).Property(x => x.DeletedBy).IsModified = false;
        }
    }

    /// <summary>
    /// Asynchronously deletes an entity that matches the specified identity predicate.
    /// </summary>
    /// <param name="identityPredicate">The predicate to identify the entity to delete.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public virtual async ValueTask DeleteAsync(Expression<Func<TEntity, bool>> identityPredicate, CancellationToken cancellationToken = default)
    {
        TEntity? existing;
        int rowsAffected;
        DbSet<TEntity> dbSet = Context.Set<TEntity>();

        existing = await dbSet.SingleOrDefaultAsync(identityPredicate, cancellationToken);

        if (existing != null)
        {
            if (existing.GetType() is IRowVersion)
            {
                rowsAffected = await dbSet.Where(
                    x => identityPredicate.Compile().Invoke(x)
                    && ((IRowVersion)x).RowVersion == ((IRowVersion)existing).RowVersion
                )
                .ExecuteUpdateAsync(u => u.SetProperty(p => p.DeletedOn, DateTime.UtcNow), cancellationToken);

                if (rowsAffected == 0)
                {
                    throw new DBConcurrencyException("The record has been modified in the database. Please refresh and try again.");
                }
            }
            else
            {
                rowsAffected = await dbSet.Where(identityPredicate)
                .ExecuteUpdateAsync(u => u.SetProperty(p => p.DeletedOn, DateTime.UtcNow), cancellationToken);
            }
        }
    }

    /// <summary>
    /// Adds or updates an entity based on the specified identity predicate.
    /// </summary>
    /// <param name="value">The entity to add or update.</param>
    /// <param name="identityPredicate">The predicate to identify the entity.</param>
    public void AddOrUpdate(TEntity value, Expression<Func<TEntity, bool>> identityPredicate)
    {
        AddOrUpdateAsync(value, identityPredicate).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Deletes an entity that matches the specified identity predicate.
    /// </summary>
    /// <param name="identityPredicate">The predicate to identify the entity to delete.</param>
    public void Delete(Expression<Func<TEntity, bool>> identityPredicate)
    {
        DeleteAsync(identityPredicate).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Finds entities that match the specified predicate.
    /// </summary>
    /// <param name="wherePredicate">The predicate to filter the entities.</param>
    /// <param name="withIncludes">Whether to include related entities.</param>
    /// <param name="asNoTracking">Whether to track the entity.</param>
    /// <param name="includeDeleted">Whether to include soft deleted entities.</param>
    /// <returns>An enumerable of entities that match the predicate.</returns>
    public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> wherePredicate, bool withIncludes = true, bool asNoTracking = true, bool includeDeleted = false)
    {
        return FindAsync(wherePredicate, withIncludes, asNoTracking, includeDeleted).Result;
    }

    /// <summary>
    /// Gets all entities.
    /// </summary>
    /// <param name="withIncludes">Whether to include related entities.</param>
    /// <param name="asNoTracking">Whether to track the entity.</param>
    /// <param name="includeDeleted">Whether to include soft deleted entities.</param>
    /// <returns>An enumerable of all entities.</returns>
    public IEnumerable<TEntity> GetAll(bool withIncludes = true, bool asNoTracking = true, bool includeDeleted = false)
    {
        return GetAllAsync(withIncludes, asNoTracking, includeDeleted).Result;
    }

    /// <summary>
    /// Gets an entity that matches the specified identity predicate.
    /// </summary>
    /// <param name="identityPredicate">The predicate to identify the entity.</param>
    /// <param name="withIncludes">Whether to include related entities.</param>
    /// <param name="asNoTracking">Whether to track the entity.</param>
    /// <param name="includeDeleted">Whether to include soft deleted entities.</param>
    /// <returns>The entity that matches the predicate, or null if no entity is found.</returns>
    public TEntity? Get(Expression<Func<TEntity, bool>> identityPredicate, bool withIncludes = true, bool asNoTracking = true, bool includeDeleted = false)
    {
        return GetAsync(identityPredicate, withIncludes, asNoTracking, includeDeleted).Result;
    }
}