using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RoyalCode.OperationHint.Abstractions;
using RoyalCode.Repositories.Abstractions;

namespace RoyalCode.OperationHint.Tests.Repositories;

/// <summary>
/// Teste EF Core implementation of Operation Hint using repositories.
/// </summary>
public class RepositoriesTests
{
    [Fact]
    public void Must_Not_Includes_When_NoneHintAdded()
    {
        // Arrange
        var services = Utils.AddWorkContextWithIncludes(new ServiceCollection());
        var provider = services.BuildServiceProvider();
        Utils.InitializeDatabase(provider);
        var id = Utils.FirstComplex(provider);

        // Act
        using var scope = provider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<ComplexEntity>>();
        var entity = repository.Find(id);

        // Assert
        entity.Should().NotBeNull();
        entity!.SingleRelation.Should().BeNull();
        entity.MultipleRelation.Should().BeNull();
    }

    [Fact]
    public void Must_Includes_SingleRelation_When_TestSingleRelationHintAdded()
    {
        // Arrange
        var services = Utils.AddWorkContextWithIncludes(new ServiceCollection());
        var provider = services.BuildServiceProvider();
        Utils.InitializeDatabase(provider);
        var id = Utils.FirstComplex(provider);

        // Act
        using var scope = provider.CreateScope();

        var container = scope.ServiceProvider.GetRequiredService<IHintsContainer>();
        container.AddHint(TestHints.TestSingleRelation);

        var repository = scope.ServiceProvider.GetRequiredService<IRepository<ComplexEntity>>();
        var entity = repository.Find(id);

        // Assert
        entity.Should().NotBeNull();
        entity!.SingleRelation.Should().NotBeNull();
        entity.MultipleRelation.Should().BeNull();
    }

    [Fact]
    public void Must_Includes_MultipleRelation_When_TestMultipleRelationHintAdded()
    {
        // Arrange
        var services = Utils.AddWorkContextWithIncludes(new ServiceCollection());
        var provider = services.BuildServiceProvider();
        Utils.InitializeDatabase(provider);
        var id = Utils.FirstComplex(provider);

        // Act
        using var scope = provider.CreateScope();

        var container = scope.ServiceProvider.GetRequiredService<IHintsContainer>();
        container.AddHint(TestHints.TestMultipleRelation);

        var repository = scope.ServiceProvider.GetRequiredService<IRepository<ComplexEntity>>();
        var entity = repository.Find(id);

        // Assert
        entity.Should().NotBeNull();
        entity!.SingleRelation.Should().BeNull();
        entity.MultipleRelation.Should().NotBeNull();
    }

    [Fact]
    public void Must_Includes_AllRelations_When_TestAllRelationsHintAdded()
    {
        // Arrange
        var services = Utils.AddWorkContextWithIncludes(new ServiceCollection());
        var provider = services.BuildServiceProvider();
        Utils.InitializeDatabase(provider);
        var id = Utils.FirstComplex(provider);

        // Act
        using var scope = provider.CreateScope();

        var container = scope.ServiceProvider.GetRequiredService<IHintsContainer>();
        container.AddHint(TestHints.TestAllRelations);

        var repository = scope.ServiceProvider.GetRequiredService<IRepository<ComplexEntity>>();
        var entity = repository.Find(id);

        // Assert
        entity.Should().NotBeNull();
        entity!.SingleRelation.Should().NotBeNull();
        entity.MultipleRelation.Should().NotBeNull();
    }

    [Fact]
    public void Must_Includes_AllRelations_When_TestSingleRelationHintAdded_And_TestMultipleRelationHintAdded()
    {
        // Arrange
        var services = Utils.AddWorkContextWithIncludes(new ServiceCollection());
        var provider = services.BuildServiceProvider();
        Utils.InitializeDatabase(provider);
        var id = Utils.FirstComplex(provider);

        // Act
        using var scope = provider.CreateScope();

        var container = scope.ServiceProvider.GetRequiredService<IHintsContainer>();
        container.AddHint(TestHints.TestSingleRelation);
        container.AddHint(TestHints.TestMultipleRelation);

        var repository = scope.ServiceProvider.GetRequiredService<IRepository<ComplexEntity>>();
        var entity = repository.Find(id);

        // Assert
        entity.Should().NotBeNull();
        entity!.SingleRelation.Should().NotBeNull();
        entity.MultipleRelation.Should().NotBeNull();
    }

    [Fact]
    public void Must_Includes_AllRelations_When_AllHintsAdded()
    {
        // Arrange
        var services = Utils.AddWorkContextWithIncludes(new ServiceCollection());
        var provider = services.BuildServiceProvider();
        Utils.InitializeDatabase(provider);
        var id = Utils.FirstComplex(provider);

        // Act
        using var scope = provider.CreateScope();

        var container = scope.ServiceProvider.GetRequiredService<IHintsContainer>();
        container.AddHint(TestHints.TestSingleRelation);
        container.AddHint(TestHints.TestMultipleRelation);
        container.AddHint(TestHints.TestAllRelations);

        var repository = scope.ServiceProvider.GetRequiredService<IRepository<ComplexEntity>>();
        var entity = repository.Find(id);

        // Assert
        entity.Should().NotBeNull();
        entity!.SingleRelation.Should().NotBeNull();
        entity.MultipleRelation.Should().NotBeNull();
    }
}
