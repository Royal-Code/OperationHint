
using System.Runtime.CompilerServices;

namespace RoyalCode.OperationHint.Abstractions;

/// <summary>
/// <para>
///     Default implementation of <see cref="IHintHandlerRegistry"/>.
/// </para>
/// <para>
///     Accepts <see cref="IHintQueryHandler{TQuery,THint}"/> and <see cref="IHintEntityHandler{TEntity,TSource,THint}"/>.
/// </para>
/// </summary>
public sealed class DefaultHintHandlerRegistry : IHintHandlerRegistry
{
    private readonly List<object> handlers = [];

    /// <inheritdoc />
    public bool IsEmpty => handlers.Count == 0;

    /// <inheritdoc />
    public IHintHandlerRegistry Add<TQuery, THint>(IHintQueryHandler<TQuery, THint> handler)
        where TQuery : class
        where THint : Enum
    {
        if (!handlers.Contains(handler))
            handlers.Add(handler);
        return this;
    }

    /// <inheritdoc />
    public IHintHandlerRegistry Add<TEntity, TSource, THint>(IHintEntityHandler<TEntity, TSource, THint> handler)
        where TEntity : class
        where TSource : class
        where THint : Enum
    {
        if (!handlers.Contains(handler))
            handlers.Add(handler);
        return this;
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<IHintEntityHandler<TEntity, TSource, THint>> GetEntityHandlers<TEntity, TSource, THint>()
        where TEntity : class
        where TSource : class
        where THint : Enum
    {
        return handlers.OfType<IHintEntityHandler<TEntity, TSource, THint>>();
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<IHintQueryHandler<TQuery, THint>> GetQueryHandlers<TQuery, THint>()
        where TQuery : class
        where THint : Enum
    {
        return handlers.OfType<IHintQueryHandler<TQuery, THint>>();
    }
}
