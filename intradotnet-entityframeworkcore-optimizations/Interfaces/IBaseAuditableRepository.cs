using System.Linq.Expressions;

namespace IntraDotNet.EntityFrameworkCore.Optimizations.Interfaces;

public interface IBaseAuditableRepository<T> where T : class, IAuditable
{
    ValueTask AddOrUpdateAsync(T value, Expression<Func<T, bool>> identityPredicate);
    void AddOrUpdate(T value, Expression<Func<T, bool>> identityPredicate);
    ValueTask DeleteAsync(Expression<Func<T, bool>> identityPredicate);
    void Delete(Expression<Func<T, bool>> identityPredicate);
    ValueTask<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> wherePredicate);
    IEnumerable<T> Find(Expression<Func<T, bool>> wherePredicate);
    ValueTask<IEnumerable<T>> GetAllAsync();
    IEnumerable<T> GetAll();
    ValueTask<T?> GetAsync(Expression<Func<T, bool>> identityPredicate);
    T? Get(Expression<Func<T, bool>> identityPredicate);
    IQueryable<T> GetQueryable(bool WithIncludes = true);
    ValueTask SaveChangesAsync();
    void SaveChanges();
}
