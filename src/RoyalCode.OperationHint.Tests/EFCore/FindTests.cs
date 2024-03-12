using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RoyalCode.OperationHint.Abstractions;
using RoyalCode.OperationHint.Tests.Models;

namespace RoyalCode.OperationHint.Tests.EFCore;

/// <summary>
/// Teste EF Core implementation of Operation Hint using repositories.
/// </summary>
public class FindTests
{
    private static IServiceProvider CreateServiceProvider()
    {
        var services = Utils.AddLocalDbContext(new ServiceCollection()).AddOperationHintIncludes();
        return services.BuildServiceProvider();
    }

    [Fact]
    public void Must_Not_Includes_When_NoneHintAdded()
    {
        // Arrange
        var provider = CreateServiceProvider();
        Utils.InitializeDatabase(provider);
        var id = Utils.FirstComplex(provider);

        // Act
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LocalDbContext>();
        var entity = db.Set<ComplexEntity>().Find(id);

        var hintPerformer = scope.ServiceProvider.GetRequiredService<IHintPerformer>();
        if (entity is not null)
            hintPerformer.Perform(entity, db);

        // Assert
        entity.Should().NotBeNull();
        entity!.SingleRelation.Should().BeNull();
        entity.MultipleRelation.Should().BeNull();
    }

    [Fact]
    public void Must_Includes_SingleRelation_When_TestSingleRelationHintAdded()
    {
        // Arrange
        var provider = CreateServiceProvider();
        Utils.InitializeDatabase(provider);
        var id = Utils.FirstComplex(provider);

        // Act
        using var scope = provider.CreateScope();

        var container = scope.ServiceProvider.GetRequiredService<IHintsContainer>();
        container.AddHint(TestHints.TestSingleRelation);

        var db = scope.ServiceProvider.GetRequiredService<LocalDbContext>();
        var entity = db.Set<ComplexEntity>().Find(id);

        var hintPerformer = scope.ServiceProvider.GetRequiredService<IHintPerformer>();
        if (entity is not null)
            hintPerformer.Perform(entity, db);

        // Assert
        entity.Should().NotBeNull();
        entity!.SingleRelation.Should().NotBeNull();
        entity.MultipleRelation.Should().BeNull();
    }

    [Fact]
    public void Must_Includes_MultipleRelation_When_TestMultipleRelationHintAdded()
    {
        // Arrange
        var provider = CreateServiceProvider();
        Utils.InitializeDatabase(provider);
        var id = Utils.FirstComplex(provider);

        // Act
        using var scope = provider.CreateScope();

        var container = scope.ServiceProvider.GetRequiredService<IHintsContainer>();
        container.AddHint(TestHints.TestMultipleRelation);

        var db = scope.ServiceProvider.GetRequiredService<LocalDbContext>();
        var entity = db.Set<ComplexEntity>().Find(id);

        var hintPerformer = scope.ServiceProvider.GetRequiredService<IHintPerformer>();
        if (entity is not null)
            hintPerformer.Perform(entity, db);

        // Assert
        entity.Should().NotBeNull();
        entity!.SingleRelation.Should().BeNull();
        entity.MultipleRelation.Should().NotBeNull();
    }

    [Fact]
    public void Must_Includes_AllRelations_When_TestAllRelationsHintAdded()
    {
        // Arrange
        var provider = CreateServiceProvider();
        Utils.InitializeDatabase(provider);
        var id = Utils.FirstComplex(provider);

        // Act
        using var scope = provider.CreateScope();

        var container = scope.ServiceProvider.GetRequiredService<IHintsContainer>();
        container.AddHint(TestHints.TestAllRelations);

        var db = scope.ServiceProvider.GetRequiredService<LocalDbContext>();
        var entity = db.Set<ComplexEntity>().Find(id);

        var hintPerformer = scope.ServiceProvider.GetRequiredService<IHintPerformer>();
        if (entity is not null)
            hintPerformer.Perform(entity, db);

        // Assert
        entity.Should().NotBeNull();
        entity!.SingleRelation.Should().NotBeNull();
        entity.MultipleRelation.Should().NotBeNull();
    }

    [Fact]
    public void Must_Includes_AllRelations_When_TestSingleRelationHintAdded_And_TestMultipleRelationHintAdded()
    {
        // Arrange
        var provider = CreateServiceProvider();
        Utils.InitializeDatabase(provider);
        var id = Utils.FirstComplex(provider);

        // Act
        using var scope = provider.CreateScope();

        var container = scope.ServiceProvider.GetRequiredService<IHintsContainer>();
        container.AddHint(TestHints.TestSingleRelation);
        container.AddHint(TestHints.TestMultipleRelation);

        var db = scope.ServiceProvider.GetRequiredService<LocalDbContext>();
        var entity = db.Set<ComplexEntity>().Find(id);

        var hintPerformer = scope.ServiceProvider.GetRequiredService<IHintPerformer>();
        if (entity is not null)
            hintPerformer.Perform(entity, db);

        // Assert
        entity.Should().NotBeNull();
        entity!.SingleRelation.Should().NotBeNull();
        entity.MultipleRelation.Should().NotBeNull();
    }

    [Fact]
    public void Must_Includes_AllRelations_When_AllHintsAdded()
    {
        // Arrange
        var provider = CreateServiceProvider();
        Utils.InitializeDatabase(provider);
        var id = Utils.FirstComplex(provider);

        // Act
        using var scope = provider.CreateScope();

        var container = scope.ServiceProvider.GetRequiredService<IHintsContainer>();
        container.AddHint(TestHints.TestSingleRelation);
        container.AddHint(TestHints.TestMultipleRelation);
        container.AddHint(TestHints.TestAllRelations);

        var db = scope.ServiceProvider.GetRequiredService<LocalDbContext>();
        var entity = db.Set<ComplexEntity>().Find(id);

        var hintPerformer = scope.ServiceProvider.GetRequiredService<IHintPerformer>();
        if (entity is not null)
            hintPerformer.Perform(entity, db);

        // Assert
        entity.Should().NotBeNull();
        entity!.SingleRelation.Should().NotBeNull();
        entity.MultipleRelation.Should().NotBeNull();
    }
}
