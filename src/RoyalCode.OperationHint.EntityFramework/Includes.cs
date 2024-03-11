using RoyalCode.OperationHint.Abstractions;
using System.Linq.Expressions;

namespace RoyalCode.OperationHint.EntityFramework;

/// <summary>
/// <para>
///     Abstract class for hint handlers for Entity Framework.
/// </para>
/// <para>
///     It is created to make it easier to implement both <see cref="IHintQueryHandler{TQuery,THint}"/> 
///     and <see cref="IHintEntityHandler{TEntity,TSource,THint}"/>.
/// </para>
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public abstract class Includes<TEntity> where TEntity : class
{
    /// <summary>
    /// Include a property that is a reference type, e.g. a navigation property.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="expression">The expression that selects the property.</param>
    /// <returns>The same instance of <see cref="Includes{TEntity}"/> for chaining.</returns>
    public abstract Includes<TEntity> IncludeReference<TProperty>(Expression<Func<TEntity, TProperty?>> expression)
        where TProperty : class;

    /// <summary>
    /// Include a property that is a collection of reference types, e.g. a navigation collection.
    /// </summary>
    /// <typeparam name="TProperty">The generic type of the collection.</typeparam>
    /// <param name="expression">The expression that selects the collection.</param>
    /// <returns>The same instance of <see cref="Includes{TEntity}"/> for chaining.</returns>
    public abstract Includes<TEntity> IncludeCollection<TProperty>(Expression<Func<TEntity, IEnumerable<TProperty>>> expression)
        where TProperty : class;
}
