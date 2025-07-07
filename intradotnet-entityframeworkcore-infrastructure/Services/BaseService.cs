using IntraDotNet.EntityFrameworkCore.Infrastructure.Results;

namespace IntraDotNet.EntityFrameworkCore.Infrastructure.Services;

public abstract class BaseService<TEntity> where TEntity : class
{
    public abstract Task<ValueResult<TEntity>> CreateAsync(TEntity entity);
    public abstract Task<ValueResult<TEntity>> UpdateAsync(TEntity entity);
    public abstract Task<ValueResult<TEntity>> DeleteAsync(TEntity entity);
    public abstract Task<ValueResult<IEnumerable<TEntity>>> GetAllAsync();
    public abstract Task<ValueResult<IEnumerable<TEntity>>> FindAsync(Func<TEntity, bool> predicate);
}
