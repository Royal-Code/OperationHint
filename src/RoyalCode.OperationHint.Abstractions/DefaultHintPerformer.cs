
namespace RoyalCode.OperationHint.Abstractions;

/// <summary>
/// <para>
///     A default implementation of <see cref="IHintPerformer"/> with <see cref="IHintsContainer"/> support.
/// </para>
/// <para>
///     This implementation will perform the hint handlers for the given query.
/// </para>
/// </summary>
public sealed class DefaultHintPerformer : IHintPerformer, IHintsContainer
{
    private readonly IHintHandlerRegistry registry;
    private List<InternalHintHandler>? hints;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultHintPerformer"/> class.
    /// </summary>
    /// <param name="registry">The hint handler registry.</param>
    /// <exception cref="ArgumentNullException">
    ///     Case <paramref name="registry"/> is null.
    /// </exception>
    public DefaultHintPerformer(IHintHandlerRegistry registry)
    {
        this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    /// <inheritdoc />
    public void AddHint<THint>(THint hint) where THint : Enum
    {
        hints ??= [];
        hints.Add(new InternalHintHandler<THint>(hint));
    }

    /// <inheritdoc />
    public TQuery Perform<TQuery>(TQuery query) where TQuery : class
    {
        if (hints is null || registry.IsEmpty)
        {
            return query;
        }

        foreach (var hint in hints)
        {
            query = hint.Handle(query, registry);
        }

        return query;
    }

    /// <inheritdoc />
    public void Perform<TEntity, TSource>(TEntity entity, TSource source)
        where TEntity : class
        where TSource : class
    {
        if (hints is null || registry.IsEmpty)
        {
            return;
        }

        foreach (var hint in hints)
        {
            hint.Handle(entity, source, registry);
        }
    }

    /// <inheritdoc />
    public Task PerformAsync<TEntity, TSource>(TEntity entity, TSource source)
        where TEntity : class
        where TSource : class
    {
        if (hints is null || registry.IsEmpty)
        {
            return Task.CompletedTask;
        }

        return PerformAsync(entity, source, hints);
    }

    private async Task PerformAsync<TEntity, TSource>(TEntity entity, TSource source, List<InternalHintHandler> hints)
        where TEntity : class
        where TSource : class
    {
        foreach (var hint in hints)
        {
            await hint.HandleAsync(entity, source, registry);
        }
    }

    private abstract class InternalHintHandler
    {
        internal abstract TQuery Handle<TQuery>(TQuery query, IHintHandlerRegistry registry)
            where TQuery : class;

        internal abstract void Handle<TEntity, TSource>(TEntity entity, TSource source, IHintHandlerRegistry registry)
            where TEntity : class
            where TSource : class;

        internal abstract Task HandleAsync<TEntity, TSource>(TEntity entity, TSource source, IHintHandlerRegistry registry)
            where TEntity : class
            where TSource : class;
    }

    private sealed class InternalHintHandler<TEnum>(TEnum hint) : InternalHintHandler
        where TEnum : Enum
    {
        internal override TQuery Handle<TQuery>(TQuery query, IHintHandlerRegistry registry)
            where TQuery : class
        {
            var handlers = registry.GetQueryHandlers<TQuery, TEnum>();
            foreach (var handler in handlers)
            {
                query = handler.Handle(query, hint);
            }
            return query;
        }

        internal override void Handle<TEntity, TSource>(TEntity entity, TSource source, IHintHandlerRegistry registry)
        {
            var handlers = registry.GetEntityHandlers<TEntity, TSource, TEnum>();
            foreach (var handler in handlers)
            {
                handler.Handle(entity, source, hint);
            }
        }

        internal override async Task HandleAsync<TEntity, TSource>(TEntity entity, TSource source, IHintHandlerRegistry registry)
        {
            var handlers = registry.GetEntityHandlers<TEntity, TSource, TEnum>();
            foreach (var handler in handlers)
            {
                await handler.HandleAsync(entity, source, hint);
            }
        }
    }
}
