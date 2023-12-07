namespace RoyalCode.OperationHint.Abstractions;

/// <summary>
/// <para>
///     A handler for a hint.
/// </para>
/// <para>
///     The hint handler can apply some logic to the query based on the hint.
/// </para>
/// </summary>
/// <typeparam name="TQuery">The type of the query.</typeparam>
/// <typeparam name="THint">The type of the hint.</typeparam>
public interface IHintQueryHandler<TQuery, in THint>
    where TQuery : class
    where THint : Enum
{
    /// <summary>
    /// <para>
    ///     Handle the hint with the given query.
    /// </para>
    /// <para>
    ///     For a given hint, represented by the enum value, the handler can apply some logic to the query.
    /// </para>
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="hint">The hint to handle.</param>
    /// <returns>The query after handling the hint.</returns>
    TQuery Handle(TQuery query, THint hint);
}
