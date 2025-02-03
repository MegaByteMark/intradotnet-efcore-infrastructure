using System.Linq.Expressions;

namespace IntraDotNet.EntityFrameworkCore.Optimizations.Interfaces;

public interface IBaseAuditableRepository<T> where T : class, IAuditable
{
    ValueTask AddOrUpdateAsync(T value, Expression<Func<T, bool>> identityPredicate, CancellationToken cancellationToken = default);
    void AddOrUpdate(T value, Expression<Func<T, bool>> identityPredicate);
    ValueTask DeleteAsync(Expression<Func<T, bool>> identityPredicate, CancellationToken cancellationToken = default);
    void Delete(Expression<Func<T, bool>> identityPredicate);
    ValueTask<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> wherePredicate, bool withIncludes = true, bool asNoTracking = true, bool includeDeleted = false, CancellationToken cancellationToken = default);
    IEnumerable<T> Find(Expression<Func<T, bool>> wherePredicate, bool withIncludes = true, bool asNoTracking = true, bool includeDeleted = false);
    ValueTask<IEnumerable<T>> GetAllAsync(bool withIncludes = true, bool asNoTracking = true, bool includeDeleted = false, CancellationToken cancellationToken = default);
    IEnumerable<T> GetAll(bool withIncludes = true, bool asNoTracking = true, bool includeDeleted = false);
    ValueTask<T?> GetAsync(Expression<Func<T, bool>> identityPredicate, bool withIncludes = true, bool asNoTracking = true, bool includeDeleted = false, CancellationToken cancellationToken = default);
    T? Get(Expression<Func<T, bool>> identityPredicate, bool withIncludes = true, bool asNoTracking = true, bool includeDeleted = false);
    IQueryable<T> GetQueryable(bool withIncludes = true, bool asNoTracking = true, bool includeDeleted = false);
    ValueTask SaveChangesAsync(CancellationToken cancellationToken = default);
    void SaveChanges();
}
