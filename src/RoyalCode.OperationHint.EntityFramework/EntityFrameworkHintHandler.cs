using System.Runtime.CompilerServices;

namespace RoyalCode.OperationHint.EntityFramework;

/// <summary>
/// <para>
///     Default implementation of <see cref="EntityFrameworkHintHandlerBase{TEntity, THint}"/>
///     handling <see cref="Includes{TEntity}"/> for Entity Framework through action.
/// </para>
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="THint"></typeparam>
public sealed class EntityFrameworkHintHandler<TEntity, THint> : EntityFrameworkHintHandlerBase<TEntity, THint>
    where TEntity : class
    where THint : Enum
{
    private readonly Action<THint, Includes<TEntity>> action;

    /// <summary>
    /// Create a new instance of <see cref="EntityFrameworkHintHandler{TEntity, THint}"/>.
    /// </summary>
    /// <param name="action">The action to handle the hint.</param>
    public EntityFrameworkHintHandler(Action<THint, Includes<TEntity>> action)
    {
        this.action = action;
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void Handle(THint hint, Includes<TEntity> includes) => action(hint, includes);
}


