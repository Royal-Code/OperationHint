
namespace RoyalCode.OperationHint.Abstractions;

/// <summary>
/// <para>
///     A handler for a hint.
/// </para>
/// <para>
///     The hint handler can apply some logic to the entity based on the hint.
/// </para>
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TSource">The type of the source.</typeparam>
/// <typeparam name="THint">The type of the hint.</typeparam>
public interface IHintEntityHandler<in TEntity, in TSource, in THint>
    where TEntity: class
    where TSource: class
    where THint: Enum
{
    /// <summary>
    /// <para>
    ///     Handle the hint with the given entity.
    /// </para>
    /// </summary>
    /// <param name="entity">The entity to handle.</param>
    /// <param name="source">The source, where the entity came from.</param>
    /// <param name="hint">The hint to handle.</param>
    void Handle(TEntity entity, TSource source, THint hint);

    /// <summary>
    /// <para>
    ///     Handle the hint with the given entity.
    /// </para>
    /// </summary>
    /// <param name="entity">The entity to handle.</param>
    /// <param name="source">The source, where the entity came from.</param>
    /// <param name="hint">The hint to handle.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task HandleAsync(TEntity entity, TSource source, THint hint);
}