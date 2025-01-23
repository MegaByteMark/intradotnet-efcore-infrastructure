using System.Linq.Expressions;

namespace IntraDotNet.EntityFrameworkCore.Optimizations.Interfaces;

public interface IBaseAuditableRepository<T> where T : class, IAuditable
{
    ValueTask AddOrUpdateAsync(T value, Expression<Func<T, bool>> identityPredicate);
    void AddOrUpdate(T value, Expression<Func<T, bool>> identityPredicate);
    ValueTask DeleteAsync(Expression<Func<T, bool>> identityPredicate);
    void Delete(Expression<Func<T, bool>> identityPredicate);
    ValueTask<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> wherePredicate, bool withIncludes = true, bool asNoTracking = true, bool includeDeleted = false);
    IEnumerable<T> Find(Expression<Func<T, bool>> wherePredicate, bool withIncludes = true, bool asNoTracking = true, bool includeDeleted = false);
    ValueTask<IEnumerable<T>> GetAllAsync(bool withIncludes = true, bool asNoTracking = true, bool includeDeleted = false);
    IEnumerable<T> GetAll(bool withIncludes = true, bool asNoTracking = true, bool includeDeleted = false);
    ValueTask<T?> GetAsync(Expression<Func<T, bool>> identityPredicate, bool withIncludes = true, bool asNoTracking = true, bool includeDeleted = false);
    T? Get(Expression<Func<T, bool>> identityPredicate, bool withIncludes = true, bool asNoTracking = true, bool includeDeleted = false);
    IQueryable<T> GetQueryable(bool withIncludes = true, bool asNoTracking = true, bool includeDeleted = false);
    ValueTask SaveChangesAsync();
    void SaveChanges();
}
