using Microsoft.EntityFrameworkCore;
using RoyalCode.OperationHint.Abstractions;
using RoyalCode.OperationHint.EntityFramework.Internals;

namespace RoyalCode.OperationHint.EntityFramework;

/// <summary>
/// Base class for hint handlers for Entity Framework.
/// </summary>
/// <typeparam name="TEntity">The entity type to handle.</typeparam>
/// <typeparam name="THint">The hint type to handle.</typeparam>
public abstract class EntityFrameworkHintHandlerBase<TEntity, THint> : IHintQueryHandler<IQueryable<TEntity>, THint>, IHintEntityHandler<TEntity, DbContext, THint>
    where TEntity : class
    where THint : Enum
{
    /// <inheritdoc />
    public virtual IQueryable<TEntity> Handle(IQueryable<TEntity> query, THint hint)
    {
        var includes = new QueryableIncludes<TEntity>(query);
        Handle(hint, includes);
        return includes.Query;
    }

    /// <inheritdoc />
    public virtual void Handle(TEntity entity, DbContext source, THint hint)
    {
        var includes = new EntryIncludes<TEntity>(source.Entry(entity));
        Handle(hint, includes);
    }

    /// <inheritdoc />
    public virtual Task HandleAsync(TEntity entity, DbContext source, THint hint)
    {
        var includes = new EntryIncludesAsync<TEntity>(source.Entry(entity));
        Handle(hint, includes);
        return includes.Task ?? Task.CompletedTask;
    }

    /// <summary>
    /// Action to handle the hint and apply the includes.
    /// </summary>
    /// <param name="hint">The current hint.</param>
    /// <param name="includes">The classe to inform about the includes.</param>
    protected abstract void Handle(THint hint, Includes<TEntity> includes);
}


