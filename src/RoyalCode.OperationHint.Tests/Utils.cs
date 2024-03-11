using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RoyalCode.Persistence.EntityFramework.UnitOfWork;
using System.Data.Common;

namespace RoyalCode.OperationHint.Tests;

internal static class Utils
{
    public static TServices AddWorkContext<TServices>(
        TServices services,
        Action<IUnitOfWorkBuilder<LocalDbContext>>? configureBuilder = null)
        where TServices : IServiceCollection
    {
        DbConnection conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        services.TryAddSingleton(conn);
        var builder = services.AddWorkContext<LocalDbContext>()
            .ConfigureDbContextPool(builder => builder.UseSqlite(conn))
            .ConfigureRepositories(c =>
            {
                c.Add<SimpleEntity>();
                c.Add<ComplexEntity>();
            });

        configureBuilder?.Invoke(builder);

        return services;
    }

    public static TServices AddWorkContextWithIncludes<TServices>(
        TServices services,
        Action<IUnitOfWorkBuilder<LocalDbContext>>? configureBuilder = null)
        where TServices : IServiceCollection
    {
        AddWorkContext(services, builder =>
        {
            builder.ConfigureOperationHints(regitry =>
            {
                regitry.AddIncludesHandler<ComplexEntity, TestHints>((hint, includes) =>
                {
                    switch (hint)
                    {
                        case TestHints.TestSingleRelation:
                            includes.IncludeReference(e => e.SingleRelation);
                            break;
                        case TestHints.TestMultipleRelation:
                            includes.IncludeCollection(e => e.MultipleRelation);
                            break;
                        case TestHints.TestAllRelations:
                            includes
                                .IncludeReference(e => e.SingleRelation)
                                .IncludeCollection(e => e.MultipleRelation);
                            break;
                    }
                });
            });

            configureBuilder?.Invoke(builder);
        });

        return services;
    }

    public static void InitializeDatabase(IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LocalDbContext>();

        context.Database.EnsureCreated();

        context.ComplexEntities.Add(new ComplexEntity
        {
            Name = "ComplexEntity",
            SingleRelation = new SimpleEntity
            {
                Name = "SingleRelation"
            },
            MultipleRelation = new List<SimpleEntity>
            {
                new() {
                    Name = "MultipleRelation1"
                },
                new() {
                    Name = "MultipleRelation2"
                }
            }
        });

        context.SaveChanges();
    }

    public static int FirstComplex(IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LocalDbContext>();
        return context.ComplexEntities.First().Id;
    }
}
