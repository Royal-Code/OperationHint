using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace RoyalCode.OperationHint.EntityFramework.Internals;

internal sealed class QueryableIncludes<TEntity> : Includes<TEntity> where TEntity : class
{
    public QueryableIncludes(IQueryable<TEntity> query)
    {
        Query = query;
    }

    public IQueryable<TEntity> Query { get; private set; }

    public override Includes<TEntity> IncludeReference<TProperty>(Expression<Func<TEntity, TProperty?>> expression)
        where TProperty : class
    {
        Query = Query.Include(expression);
        return this;
    }

    public override Includes<TEntity> IncludeCollection<TProperty>(Expression<Func<TEntity, IEnumerable<TProperty>>> expression) where TProperty : class
    {
        Query = Query.Include(expression);
        return this;
    }
}
