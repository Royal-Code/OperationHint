using Microsoft.EntityFrameworkCore;
using RoyalCode.OperationHint.Abstractions;
using RoyalCode.OperationHint.EntityFramework;
using RoyalCode.OperationHint.EntityFramework.Internals;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions methods for add operation hints to the service collection.
/// </summary>
public static class OperationHintServiceCollectionExtensions
{
    /// <summary>
    /// Add base services for operation hints.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="lifetime">The services lifetime, by default is scoped.</param>
    /// <returns>The same instance of <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddOperationHints(this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        if (services.Any(d => d.ServiceType == typeof(DefaultHintPerformer)))
            return services;

        services.Add(ServiceDescriptor.Describe(
            typeof(DefaultHintPerformer),
            typeof(DefaultHintPerformer),
            lifetime));

        services.Add(ServiceDescriptor.Describe(
            typeof(IHintPerformer),
            sp => sp.GetService<DefaultHintPerformer>()!,
            lifetime));

        services.Add(ServiceDescriptor.Describe(
            typeof(IHintsContainer),
            sp => sp.GetService<DefaultHintPerformer>()!,
            lifetime));

        return services;
    }

    /// <summary>
    /// <para>
    ///     Internal method to get or add the hint handler registry.
    /// </para>
    /// <para>
    ///     The hint handler registry is a singleton service instance.
    /// </para>
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The hint handler registry.</returns>
    public static IHintHandlerRegistry GetOrAddHintHandlerRegistry(this IServiceCollection services)
    {
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IHintHandlerRegistry)
            && d.ImplementationType is not null
            && d.ImplementationType.IsAssignableTo(typeof(DefaultHintHandlerRegistry)));

        if (descriptor is not null)
        {
            return (IHintHandlerRegistry)descriptor.ImplementationInstance!;
        }

        var registry = new DefaultHintHandlerRegistry();
        services.AddSingleton<IHintHandlerRegistry>(registry);
        return registry;
    }

    /// <summary>
    /// <para>
    ///     Add a hint handler for entity framework that handle the hint through an action,
    ///     using the includes class.
    /// </para>
    /// <para>
    ///     The hint handler can handle one entity type (<typeparamref name="TEntity"/>) 
    ///     and one hint type (<typeparamref name="THint"/>).
    /// </para>
    /// </summary>
    /// <typeparam name="TEntity">The entity type to handle.</typeparam>
    /// <typeparam name="THint">The hint type to handle.</typeparam>
    /// <param name="registry">The hint handler registry.</param>
    /// <param name="action">The action to handle the hint.</param>
    /// <returns>The same instance of <paramref name="registry"/> for chaining.</returns>s
    public static IHintHandlerRegistry AddIncludesHandler<TEntity, THint>(this IHintHandlerRegistry registry,
        Action<THint, Includes<TEntity>> action)
        where TEntity : class
        where THint : Enum
    {
        var handler = new EntityFrameworkHintHandler<TEntity, THint>(action);
        registry.Add<IQueryable<TEntity>, THint>(handler);
        registry.Add<TEntity, DbContext, THint>(handler);
        return registry;
    }

    /// <summary>
    /// <para>
    ///     Add operation hints for entity framework that handle the hint through an action,
    ///     using the includes class.
    /// </para>
    /// </summary>
    /// <typeparam name="THint">The hint type to handle.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The hint handler builder.</returns>
    public static IHintHandlerBuilder<THint> AddIncludesHintHandler<THint>(this IServiceCollection services)
        where THint : Enum
    {
        services.AddOperationHints();
        var registry = services.GetOrAddHintHandlerRegistry();
        return new HintHandlerBuilder<THint>(registry);
    }
}