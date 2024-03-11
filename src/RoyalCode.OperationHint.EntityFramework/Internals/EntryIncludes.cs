using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace RoyalCode.OperationHint.EntityFramework.Internals;

internal sealed class EntryIncludes<TEntity> : Includes<TEntity> where TEntity : class
{
    private readonly EntityEntry<TEntity> entry;

    public EntryIncludes(EntityEntry<TEntity> entry)
    {
        this.entry = entry;
    }

    public override Includes<TEntity> IncludeReference<TProperty>(Expression<Func<TEntity, TProperty?>> expression)
        where TProperty : class
    {
        entry.Reference(expression).Load();
        return this;
    }

    public override Includes<TEntity> IncludeCollection<TProperty>(Expression<Func<TEntity, IEnumerable<TProperty>>> expression) where TProperty : class
    {
        entry.Collection(expression).Load();
        return this;
    }
}
