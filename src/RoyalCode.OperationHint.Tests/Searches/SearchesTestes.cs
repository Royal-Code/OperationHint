
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RoyalCode.OperationHint.Abstractions;
using RoyalCode.Searches.Abstractions;

namespace RoyalCode.OperationHint.Tests.Searches;

public class SearchesTestes
{
    private static IServiceProvider CreateServiceProvider()
    {
        var services = Utils.AddWorkContextWithIncludes(new ServiceCollection(), builder =>
        {
            builder.ConfigureSearches(conf =>
            {
                conf.Add<ComplexEntity>();
            });
        });
        return services.BuildServiceProvider();
    }


    [Fact]
    public void Must_Includes_SingleRelation_When_TestSingleRelationHintAdded()
    {
        // Arrange
        var provider = CreateServiceProvider();
        Utils.InitializeDatabase(provider);
        Utils.InitializeDatabase(provider);

        // Act
        using var scope = provider.CreateScope();

        var container = scope.ServiceProvider.GetRequiredService<IHintsContainer>();
        container.AddHint(TestHints.TestSingleRelation);

        var search = scope.ServiceProvider.GetRequiredService<IAllEntities<ComplexEntity>>();
        search.FilterBy(new ComplexFilter());
        var list = search.Collect();

        // Assert
        list.Should().NotBeEmpty();
        list.Should().OnlyContain(x => x.SingleRelation != null);
        list.Should().OnlyContain(x => x.MultipleRelation == null);
    }

    [Fact]
    public void Must_Includes_MultipleRelation_When_TestMultipleRelationHintAdded()
    {
        // Arrange
        var provider = CreateServiceProvider();
        Utils.InitializeDatabase(provider);
        Utils.InitializeDatabase(provider);

        // Act
        using var scope = provider.CreateScope();

        var container = scope.ServiceProvider.GetRequiredService<IHintsContainer>();
        container.AddHint(TestHints.TestMultipleRelation);

        var search = scope.ServiceProvider.GetRequiredService<IAllEntities<ComplexEntity>>();
        search.FilterBy(new ComplexFilter());
        var list = search.Collect();

        // Assert
        list.Should().NotBeEmpty();
        list.Should().OnlyContain(x => x.SingleRelation == null);
        list.Should().OnlyContain(x => x.MultipleRelation != null);
    }

    [Fact]
    public void Must_Includes_AllRelations_When_TestAllRelationsHintAdded()
    {
        // Arrange
        var provider = CreateServiceProvider();
        Utils.InitializeDatabase(provider);
        Utils.InitializeDatabase(provider);

        // Act
        using var scope = provider.CreateScope();

        var container = scope.ServiceProvider.GetRequiredService<IHintsContainer>();
        container.AddHint(TestHints.TestAllRelations);

        var search = scope.ServiceProvider.GetRequiredService<IAllEntities<ComplexEntity>>();
        search.FilterBy(new ComplexFilter());
        var list = search.Collect();

        // Assert
        list.Should().NotBeEmpty();
        list.Should().OnlyContain(x => x.SingleRelation != null);
        list.Should().OnlyContain(x => x.MultipleRelation != null);
    }

    [Fact]
    public void Must_Includes_AllRelations_When_TestSingleRelationHintAdded_And_TestMultipleRelationHintAdded()
    {
        // Arrange
        var provider = CreateServiceProvider();
        Utils.InitializeDatabase(provider);
        Utils.InitializeDatabase(provider);

        // Act
        using var scope = provider.CreateScope();

        var container = scope.ServiceProvider.GetRequiredService<IHintsContainer>();
        container.AddHint(TestHints.TestSingleRelation);
        container.AddHint(TestHints.TestMultipleRelation);

        var search = scope.ServiceProvider.GetRequiredService<IAllEntities<ComplexEntity>>();
        search.FilterBy(new ComplexFilter());
        var list = search.Collect();

        // Assert
        list.Should().NotBeEmpty();
        list.Should().OnlyContain(x => x.SingleRelation != null);
        list.Should().OnlyContain(x => x.MultipleRelation != null);
    }

    [Fact]
    public void Must_Includes_AllRelations_When_AllHintsAdded()
    {
        // Arrange
        var provider = CreateServiceProvider();
        Utils.InitializeDatabase(provider);
        Utils.InitializeDatabase(provider);

        // Act
        using var scope = provider.CreateScope();

        var container = scope.ServiceProvider.GetRequiredService<IHintsContainer>();
        container.AddHint(TestHints.TestSingleRelation);
        container.AddHint(TestHints.TestMultipleRelation);
        container.AddHint(TestHints.TestAllRelations);

        var search = scope.ServiceProvider.GetRequiredService<IAllEntities<ComplexEntity>>();
        search.FilterBy(new ComplexFilter());
        var list = search.Collect();

        // Assert
        list.Should().NotBeEmpty();
        list.Should().OnlyContain(x => x.SingleRelation != null);
        list.Should().OnlyContain(x => x.MultipleRelation != null);
    }
}
