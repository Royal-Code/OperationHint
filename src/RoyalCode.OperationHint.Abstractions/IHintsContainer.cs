
namespace RoyalCode.OperationHint.Abstractions;

/// <summary>
/// <para>
///     A container for hints.
/// </para>
/// <para>
///     The hint will be added to the container and will be handled by the hint handlers when the query is created.
/// </para>
/// </summary>
public interface IHintsContainer
{
    /// <summary>
    /// Adds a hint to the container.
    /// </summary>
    /// <typeparam name="THint">
    ///     The type of the hint.
    /// </typeparam>
    /// <param name="hint">
    ///     The hint to add.
    /// </param>
    void AddHint<THint>(THint hint) where THint : Enum;
}
