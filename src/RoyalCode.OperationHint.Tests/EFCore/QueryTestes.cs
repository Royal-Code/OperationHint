
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RoyalCode.OperationHint.Abstractions;
using RoyalCode.OperationHint.Tests.Models;

namespace RoyalCode.OperationHint.Tests.EFCore;

public class QueryTestes
{
    private static IServiceProvider CreateServiceProvider()
    {
        var services = Utils.AddLocalDbContext(new ServiceCollection()).AddOperationHintIncludes();
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

        var db = scope.ServiceProvider.GetRequiredService<LocalDbContext>();
        IQueryable<ComplexEntity> query = db.Set<ComplexEntity>();

        var hintPerformer = scope.ServiceProvider.GetRequiredService<IHintPerformer>();
        query = hintPerformer.Perform(query);

        var list = query.ToList();

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

        var db = scope.ServiceProvider.GetRequiredService<LocalDbContext>();
        IQueryable<ComplexEntity> query = db.Set<ComplexEntity>();

        var hintPerformer = scope.ServiceProvider.GetRequiredService<IHintPerformer>();
        query = hintPerformer.Perform(query);

        var list = query.ToList();

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

        var db = scope.ServiceProvider.GetRequiredService<LocalDbContext>();
        IQueryable<ComplexEntity> query = db.Set<ComplexEntity>();

        var hintPerformer = scope.ServiceProvider.GetRequiredService<IHintPerformer>();
        query = hintPerformer.Perform(query);

        var list = query.ToList();

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

        var db = scope.ServiceProvider.GetRequiredService<LocalDbContext>();
        IQueryable<ComplexEntity> query = db.Set<ComplexEntity>();

        var hintPerformer = scope.ServiceProvider.GetRequiredService<IHintPerformer>();
        query = hintPerformer.Perform(query);

        var list = query.ToList();

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

        var db = scope.ServiceProvider.GetRequiredService<LocalDbContext>();
        IQueryable<ComplexEntity> query = db.Set<ComplexEntity>();

        var hintPerformer = scope.ServiceProvider.GetRequiredService<IHintPerformer>();
        query = hintPerformer.Perform(query);

        var list = query.ToList();

        // Assert
        list.Should().NotBeEmpty();
        list.Should().OnlyContain(x => x.SingleRelation != null);
        list.Should().OnlyContain(x => x.MultipleRelation != null);
    }
}
