using Microsoft.Extensions.DependencyInjection;
using RoyalCode.OperationHint.Abstractions;

namespace RoyalCode.OperationHint.EntityFramework.Internals;

internal sealed class HintHandlerBuilder<THint> : IHintHandlerBuilder<THint>
    where THint : Enum
{
    private readonly IHintHandlerRegistry registry;

    internal HintHandlerBuilder(IHintHandlerRegistry registry)
    {
        this.registry = registry;
    }

    public IHintHandlerBuilder<THint> Add<TEntity>(Action<THint, Includes<TEntity>> action)
        where TEntity : class
    {
        registry.AddIncludesHandler(action);
        return this;
    }
}
