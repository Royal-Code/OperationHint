using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace RoyalCode.OperationHint.EntityFramework.Internals;

internal sealed class EntryIncludesAsync<TEntity> : Includes<TEntity> where TEntity : class
{
    private readonly EntityEntry<TEntity> entry;

    public EntryIncludesAsync(EntityEntry<TEntity> entry)
    {
        this.entry = entry;
    }

    public Task? Task { get; private set; }

    public override Includes<TEntity> IncludeReference<TProperty>(Expression<Func<TEntity, TProperty?>> expression)
        where TProperty : class
    {
        var loadTask = entry.Reference(expression).LoadAsync();

        if (Task is null)
        {
            Task = loadTask;
        }
        else
        {
            Task = Task.ContinueWith(_ => loadTask);
        }

        return this;
    }

    public override Includes<TEntity> IncludeCollection<TProperty>(Expression<Func<TEntity, IEnumerable<TProperty>>> expression) where TProperty : class
    {
        var loadTask = entry.Collection(expression).LoadAsync();

        if (Task is null)
        {
            Task = loadTask;
        }
        else
        {
            Task = Task.ContinueWith(_ => loadTask);
        }

        return this;
    }
}