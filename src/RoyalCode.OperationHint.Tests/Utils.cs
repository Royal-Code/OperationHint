using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RoyalCode.OperationHint.Tests.Models;
using System.Data.Common;

namespace RoyalCode.OperationHint.Tests;

internal static class Utils
{
    public static TServices AddLocalDbContext<TServices>(
        TServices services)
        where TServices : IServiceCollection
    {
        DbConnection conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        services.TryAddSingleton(conn);

        services.AddDbContextPool<LocalDbContext>(builder => builder.UseSqlite(conn));

        return services;
    }

    public static TServices AddOperationHintIncludes<TServices>(
        this TServices services)
        where TServices : IServiceCollection
    {
        services.ConfigureOperationHints(registry =>
        {
            registry.AddIncludesHandler<ComplexEntity, TestHints>((hint, includes) =>
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
