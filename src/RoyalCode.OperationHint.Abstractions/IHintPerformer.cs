

namespace RoyalCode.OperationHint.Abstractions;

/// <summary>
/// Interface for hint performers.
/// </summary>
public interface IHintPerformer
{
    /// <summary>
    /// Performs the hint handlers for the given query.
    /// </summary>
    /// <typeparam name="TQuery">The query type.</typeparam>
    /// <param name="query">The query to perform the hint handlers for.</param>
    /// <returns>A query with the hint handlers performed.</returns>
    TQuery Perform<TQuery>(TQuery query)
        where TQuery : class;

    /// <summary>
    /// Performs the hint handlers for the given entity and source.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <param name="entity">The entity to perform the hint handlers for.</param>
    /// <param name="source">The source where the entity came from.</param>
    void Perform<TEntity, TSource>(TEntity entity, TSource source)
        where TEntity : class
        where TSource : class;

    /// <summary>
    /// Performs the hint handlers for the given query.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <param name="entity">The entity to perform the hint handlers for.</param>
    /// <param name="source">The source where the entity came from.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task PerformAsync<TEntity, TSource>(TEntity entity, TSource source)
        where TEntity : class
        where TSource : class;
}
