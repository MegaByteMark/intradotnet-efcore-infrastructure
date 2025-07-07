using IntraDotNet.EntityFrameworkCore.Infrastructure.Results;

namespace IntraDotNet.EntityFrameworkCore.Infrastructure.Services;

public abstract class BaseValidatableService<TEntity> : BaseService<TEntity> where TEntity : class
{
    public BaseValidatableService() : base()
    {
    }

    public override async Task<ValueResult<TEntity>> CreateAsync(TEntity entity)
    {
        // 1. Validate first (with business context)
        ValueResult<bool> validationResult = await ValidateAsync(entity);

        if (validationResult.IsFailure)
        {
            // 2. If validation fails, return the error(s)
            return ValueResult<TEntity>.Failure(validationResult.AggregateErrors!);
        }

        // 3. If validation passes, proceed with creation
        return await CreateInternalAsync(entity);
    }

    protected abstract Task<ValueResult<TEntity>> CreateInternalAsync(TEntity entity);

    public override async Task<ValueResult<TEntity>> UpdateAsync(TEntity entity)
    {
        // 1. Validate first (with business context)
        ValueResult<bool> validationResult = await ValidateAsync(entity);

        if (validationResult.IsFailure)
        {
            // 2. If validation fails, return the error(s)
            return ValueResult<TEntity>.Failure(validationResult.AggregateErrors!);
        }

        // 3. If validation passes, proceed with update
        return await UpdateInternalAsync(entity);
    }

    protected abstract Task<ValueResult<TEntity>> UpdateInternalAsync(TEntity entity);
    
    public abstract Task<ValueResult<bool>> ValidateAsync(TEntity entity);
}