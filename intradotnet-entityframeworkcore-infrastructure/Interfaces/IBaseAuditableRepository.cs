using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace IntraDotNet.EntityFrameworkCore.Infrastructure.Interfaces;

public interface IBaseAuditableRepository<TEntity> where TEntity : class, IAuditable
{
    IQueryable<TEntity> GetQueryable(bool withIncludes = true, bool asNoTracking = true, bool includeDeleted = false);
    ValueTask<TEntity?> GetAsync(Expression<Func<TEntity, bool>> identityPredicate, bool withIncludes = true, bool asNoTracking = true, bool includeDeleted = false, CancellationToken cancellationToken = default);
    ValueTask<IEnumerable<TEntity>> GetAllAsync(bool withIncludes = true, bool asNoTracking = true, bool includeDeleted = false, CancellationToken cancellationToken = default);
    ValueTask<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> wherePredicate, bool withIncludes = true, bool asNoTracking = true, bool includeDeleted = false, CancellationToken cancellationToken = default);
    ValueTask AddOrUpdateAsync(TEntity value, Expression<Func<TEntity, bool>> identityPredicate, CancellationToken cancellationToken = default);
    ValueTask DeleteAsync(Expression<Func<TEntity, bool>> identityPredicate, CancellationToken cancellationToken = default);
    void AddOrUpdate(TEntity value, Expression<Func<TEntity, bool>> identityPredicate);
    void Delete(Expression<Func<TEntity, bool>> identityPredicate);
    IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> wherePredicate, bool withIncludes = true, bool asNoTracking = true, bool includeDeleted = false);
    IEnumerable<TEntity> GetAll(bool withIncludes = true, bool asNoTracking = true, bool includeDeleted = false);
    TEntity? Get(Expression<Func<TEntity, bool>> identityPredicate, bool withIncludes = true, bool asNoTracking = true, bool includeDeleted = false);
    ValueTask SaveChangesAsync(CancellationToken cancellationToken = default);
    ValueTask SaveChangesAsync(Func<PropertyValues, PropertyValues, PropertyValues>? handleConcurrencyConflict, CancellationToken cancellationToken = default);
    void SaveChanges();
    void SaveChanges(Func<PropertyValues, PropertyValues, PropertyValues>? handleConcurrencyConflict);
}
