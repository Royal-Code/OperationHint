
namespace RoyalCode.OperationHint.Abstractions;

/// <summary>
/// A registry for hint handlers.
/// </summary>
public interface IHintHandlerRegistry
{
    /// <summary>
    /// Adds a hint handler to the registry.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="THint">The type of the hint.</typeparam>
    /// <param name="handler">The hint handler to add.</param>
    /// <returns>
    ///     The same instance of the registry for chaining.
    /// </returns>
    IHintHandlerRegistry Add<TQuery, THint>(IHintQueryHandler<TQuery, THint> handler)
        where TQuery : class
        where THint : Enum;

    /// <summary>
    /// Adds a hint handler to the registry.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="THint">The type of the hint.</typeparam>
    /// <param name="handler">The hint handler to add.</param>
    /// <returns>
    ///     The same instance of the registry for chaining.
    /// </returns>
    IHintHandlerRegistry Add<TEntity, TSource, THint>(IHintEntityHandler<TEntity, TSource, THint> handler)
        where TEntity : class
        where TSource : class
        where THint : Enum;

    /// <summary>
    /// Get all hint handlers for the given entity type and hint type.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="THint">The type of the hint.</typeparam>
    /// <returns>The hint handlers for the given entity type and hint type.</returns>
    IEnumerable<IHintQueryHandler<TQuery, THint>> GetQueryHandlers<TQuery, THint>()
        where TQuery : class
        where THint : Enum;

    /// <summary>
    /// Get all hint handlers for the given entity type, source type and hint type.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="THint">The type of the hint.</typeparam>
    /// <returns>The hint handlers for the given entity type, source type and hint type.</returns>
    IEnumerable<IHintEntityHandler<TEntity, TSource, THint>> GetEntityHandlers<TEntity, TSource, THint>()
        where TEntity : class
        where TSource : class
        where THint : Enum;

    /// <summary>
    /// Checks if the registry is empty.
    /// </summary>
    bool IsEmpty { get; }
}
