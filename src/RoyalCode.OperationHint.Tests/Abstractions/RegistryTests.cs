
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RoyalCode.OperationHint.Abstractions;

namespace RoyalCode.OperationHint.Tests.Abstractions;

public class RegistryTests
{
    [Fact]
    public void Must_AddOnlyOneInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        Utils.AddWorkContext(services, builder =>
        {
            builder.ConfigureOperationHints(regitry =>
            {
                // Act
                var handler = new SomeHandler();
                regitry.Add<IQueryable<SimpleEntity>, SomeHint>(handler);
                regitry.Add<IQueryable<SimpleEntity>, SomeHint>(handler);
                regitry.Add<SimpleEntity, DbContext, SomeHint>(handler);
                regitry.Add<SimpleEntity, DbContext, SomeHint>(handler);
            });
        });

        var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<IHintHandlerRegistry>();

        // Assert
        registry.GetQueryHandlers<IQueryable<SimpleEntity>, SomeHint>().Should().HaveCount(1);
        registry.GetEntityHandlers<SimpleEntity, DbContext, SomeHint>().Should().HaveCount(1);
    }
}

file enum SomeHint
{
    DoSomething,

    DoSomethingElse
}

file class SomeHandler : IHintEntityHandler<SimpleEntity, DbContext, SomeHint>, IHintQueryHandler<IQueryable<SimpleEntity>, SomeHint>
{
    public void Handle(SimpleEntity entity, DbContext source, SomeHint hint)
    {
        throw new NotImplementedException();
    }

    public IQueryable<SimpleEntity> Handle(IQueryable<SimpleEntity> query, SomeHint hint)
    {
        throw new NotImplementedException();
    }

    public Task HandleAsync(SimpleEntity entity, DbContext source, SomeHint hint)
    {
        throw new NotImplementedException();
    }
}