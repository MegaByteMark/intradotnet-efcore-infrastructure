using System.Data;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using IntraDotNet.EntityFrameworkCore.Optimizations.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace IntraDotNet.EntityFrameworkCore.Optimizations.Repositories;

/// <summary>
/// Abstract base repository class for handling auditable entities.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
public abstract class BaseAuditableRepository<TEntity, TDbContext>(IDbContextFactory<TDbContext> contextFactory) : IBaseAuditableRepository<TEntity>
 where TDbContext : DbContext
 where TEntity : class, IAuditable
{
    /// <summary>
    /// The database context.
    /// </summary>
    protected readonly TDbContext _context = contextFactory.CreateDbContext();

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
        IQueryable<TEntity> query = asNoTracking ? _context.Set<TEntity>().AsNoTracking() : _context.Set<TEntity>();

        // As of the current date (23-Jan-2025) the current version of EF Core does not support the HasQueryFilter method with a name parameter.
        // this means that when using the HasQueryFilter method, the filter will be applied to all queries that are executed on the entity.
        // This is not always the desired behavior, you may have 3 global query filters, the soft delete filtering being one, and you want to ignore the soft delete but keept the others.
        // This logical is commented out here until the feature becomes available in EF Core.
        // In the meantime, we will manually apply a "query filter" herer manually.
        /*if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }*/

        if(!includeDeleted)
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
    /// <returns>A task that represents the asynchronous operation. The task result contains the entity if found; otherwise, null.</returns>
    public virtual async ValueTask<TEntity?> GetAsync(Expression<Func<TEntity, bool>> identityPredicate, bool withIncludes = true, bool asNoTracking = true, bool includeDeleted = false)
    {
        return (await FindAsync(identityPredicate, withIncludes, asNoTracking, includeDeleted)).FirstOrDefault();
    }

    /// <summary>
    /// Asynchronously gets an entity from the specified database context that matches the specified identity predicate.
    /// </summary>
    /// <param name="dbContext">The database context to use.</param>
    /// <param name="identityPredicate">The predicate to identify the entity.</param>
    /// <param name="withIncludes">Whether to include related entities.</param>
    /// <param name="asNoTracking">Whether to track the entity.</param>
    /// <param name="includeDeleted">Whether to include soft deleted entities.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the entity if found; otherwise, null.</returns>
    public virtual async ValueTask<TEntity?> GetAsync(TDbContext dbContext, Expression<Func<TEntity, bool>> identityPredicate, bool withIncludes = true, bool asNoTracking = true, bool includeDeleted = false)
    {
        return (await FindAsync(dbContext, identityPredicate, withIncludes, asNoTracking, includeDeleted)).FirstOrDefault();
    }

    /// <summary>
    /// Asynchronously gets all entities.
    /// </summary>
    /// <param name="withIncludes">Whether to include related entities.</param>
    /// <param name="asNoTracking">Whether to track the entity.</param>
    /// <param name="includeDeleted">Whether to include soft deleted entities.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable of entities.</returns>
    public virtual async ValueTask<IEnumerable<TEntity>> GetAllAsync(bool withIncludes = true, bool asNoTracking = true, bool includeDeleted = false)
    {
        return await GetAllAsync(_context, withIncludes, asNoTracking, includeDeleted);
    }

    /// <summary>
    /// Asynchronously gets all entities from the specified database context.
    /// </summary>
    /// <param name="dbContext">The database context to use.</param>
    /// <param name="withIncludes">Whether to include related entities.</param>
    /// <param name="asNoTracking">Whether to track the entity.</param>
    /// <param name="includeDeleted">Whether to include soft deleted entities.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable of entities.</returns>
    public virtual async ValueTask<IEnumerable<TEntity>> GetAllAsync(TDbContext dbContext, bool withIncludes = true, bool asNoTracking = true, bool includeDeleted = false)
    {
        return await GetQueryable(withIncludes, asNoTracking, includeDeleted).ToListAsync();
    }

    /// <summary>
    /// Asynchronously finds entities that match the specified predicate.
    /// </summary>
    /// <param name="wherePredicate">The predicate to filter the entities.</param>
    /// <param name="withIncludes">Whether to include related entities.</param>
    /// <param name="asNoTracking">Whether to track the entity.</param>
    /// <param name="includeDeleted">Whether to include soft deleted entities.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable of entities that match the predicate.</returns>
    public virtual async ValueTask<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> wherePredicate, bool withIncludes = true, bool asNoTracking = true, bool includeDeleted = false)
    {
        return await FindAsync(_context, wherePredicate, withIncludes, asNoTracking, includeDeleted);
    }

    /// <summary>
    /// Asynchronously finds entities that match the specified predicate from the specified database context.
    /// </summary>
    /// <param name="dbContext">The database context to use.</param>
    /// <param name="wherePredicate">The predicate to filter the entities.</param>
    /// <param name="withIncludes">Whether to include related entities.</param>
    /// <param name="asNoTracking">Whether to track the entity.</param>
    /// <param name="includeDeleted">Whether to include soft deleted entities.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable of entities that match the predicate.</returns>
    public virtual async ValueTask<IEnumerable<TEntity>> FindAsync(TDbContext dbContext, Expression<Func<TEntity, bool>> wherePredicate, bool withIncludes = true, bool asNoTracking = true, bool includeDeleted = false)
    {
        return await GetQueryable(withIncludes, asNoTracking, includeDeleted).Where(wherePredicate).ToListAsync();
    }

    /// <summary>
    /// Asynchronously adds or updates an entity based on the specified identity predicate.
    /// </summary>
    /// <param name="value">The entity to add or update.</param>
    /// <param name="identityPredicate">The predicate to identify the entity.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public virtual async ValueTask AddOrUpdateAsync(TEntity value, Expression<Func<TEntity, bool>> identityPredicate)
    {
        await AddOrUpdateAsync(_context, value, identityPredicate);
    }

    /// <summary>
    /// Asynchronously adds or updates an entity based on the specified identity predicate in the specified database context.
    /// </summary>
    /// <param name="dbContext">The database context to use.</param>
    /// <param name="value">The entity to add or update.</param>
    /// <param name="identityPredicate">The predicate to identify the entity.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public virtual async ValueTask AddOrUpdateAsync(TDbContext dbContext, TEntity value, Expression<Func<TEntity, bool>> identityPredicate)
    {
        TEntity? existing;
        DbSet<TEntity> dbSet;

        dbSet = dbContext.Set<TEntity>();
        existing = await dbSet.SingleOrDefaultAsync(identityPredicate);

        if (existing != null)
        {
            dbContext.Entry(existing).CurrentValues.SetValues(value);

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
    /// <returns>A task that represents the asynchronous operation.</returns>
    public virtual async ValueTask DeleteAsync(Expression<Func<TEntity, bool>> identityPredicate)
    {
        await DeleteAsync(_context, identityPredicate);
    }

    /// <summary>
    /// Asynchronously deletes an entity that matches the specified identity predicate in the specified database context.
    /// </summary>
    /// <param name="dbContext">The database context to use.</param>
    /// <param name="identityPredicate">The predicate to identify the entity to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="DBConcurrencyException">Thrown if the record has been modified in the database.</exception>
    public virtual async ValueTask DeleteAsync(TDbContext dbContext, Expression<Func<TEntity, bool>> identityPredicate)
    {
        TEntity? existing;
        int rowsAffected;
        DbSet<TEntity> dbSet = dbContext.Set<TEntity>();

        existing = await dbSet.SingleOrDefaultAsync(identityPredicate);

        if (existing != null)
        {
            if (existing.GetType() is IRowVersion)
            {
                rowsAffected = await dbSet.Where(
                    x => identityPredicate.Compile().Invoke(x)
                    && ((IRowVersion)x).RowVersion == ((IRowVersion)existing).RowVersion
                )
                .ExecuteUpdateAsync(u => u.SetProperty(p => p.DeletedOn, DateTime.UtcNow));

                if (rowsAffected == 0)
                {
                    throw new DBConcurrencyException("The record has been modified in the database. Please refresh and try again.");
                }
            }
            else
            {
                rowsAffected = await dbSet.Where(identityPredicate)
                .ExecuteUpdateAsync(u => u.SetProperty(p => p.DeletedOn, DateTime.UtcNow));
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

    /// <summary>
    /// Asynchronously saves all changes made in this context to the database.
    /// </summary>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    public async ValueTask SaveChangesAsync()
    {
        await SaveChangesAsync(null);
    }

    /// <summary>
    /// Asynchronously saves all changes made in this context to the database.
    /// </summary>
    /// <param name="handleConcurrencyConflictForProperty">A method to handle concurrency conflicts for a property. First parameter is the property being inspected, the second parameter is the proposed value and the third parameter is the current database value.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    public async ValueTask SaveChangesAsync(Func<Microsoft.EntityFrameworkCore.Metadata.IProperty, object?, object?, object?>? handleConcurrencyConflictForProperty)
    {
        bool success = false;
        PropertyValues? proposedValues, databaseValues;
        object? proposedValue, databaseValue;

        while (!success)
        {
            try
            {
                await _context.SaveChangesAsync();
                success = true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                success = false;

                foreach (var entry in ex.Entries)
                {
                    if (entry.Entity is TEntity)
                    {
                        proposedValues = entry.CurrentValues;
                        databaseValues = await entry.GetDatabaseValuesAsync();

                        if (databaseValues == null)
                        {
                            throw new NotSupportedException("The entity has been deleted in the database.");
                        }
                        else
                        {
                            foreach (Microsoft.EntityFrameworkCore.Metadata.IProperty property in proposedValues.Properties)
                            {
                                proposedValue = proposedValues[property];
                                databaseValue = databaseValues[property];

                                // Decide how to handle the concurrency conflict.
                                //Call the user defined method to handle the conflict
                                if (handleConcurrencyConflictForProperty != null)
                                {
                                    proposedValues[property] = handleConcurrencyConflictForProperty(property, proposedValue, databaseValue);
                                }
                                else
                                {
                                    // Use proposed values to handle the conflict.
                                    proposedValues[property] = proposedValue;
                                }
                            }

                            // Refresh original values to bypass next concurrency check.
                            entry.OriginalValues.SetValues(databaseValues);
                        }
                    }
                    else
                    {
                        throw new NotSupportedException($"The entity type {entry.Entity.GetType().Name} is not supported for concurrency conflicts.");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    public void SaveChanges()
    {
        SaveChangesAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="handleConcurrencyConflictForProperty">A method to handle concurrency conflicts for a property. First parameter is the property being inspected, the second parameter is the proposed value and the third parameter is the current database value.</param>
    /// 
    public void SaveChanges(Func<Microsoft.EntityFrameworkCore.Metadata.IProperty, object?, object?, object?> handleConcurrencyConflictForProperty)
    {
        SaveChangesAsync(handleConcurrencyConflictForProperty).GetAwaiter().GetResult();
    }
}