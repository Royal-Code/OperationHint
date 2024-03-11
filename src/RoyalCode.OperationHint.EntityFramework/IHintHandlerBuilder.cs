
namespace RoyalCode.OperationHint.EntityFramework;

/// <summary>
/// <para>
///     A builder for hint handlers for entity framework repositories.
/// </para>
/// </summary>
/// <typeparam name="THint">The hint type to handle.</typeparam>
public interface IHintHandlerBuilder<THint>
    where THint : Enum
{
    /// <summary>
    /// Add a includes hint handler for entity framework that handle the hint through an action,
    /// using the includes class.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to handle.</typeparam>
    /// <param name="action">The action to handle the hint.</param>
    /// <returns>The same instance of <paramref name="registry"/> for chaining.</returns>
    IHintHandlerBuilder<THint> Add<TEntity>(Action<THint, Includes<TEntity>> action)
        where TEntity : class;
}
