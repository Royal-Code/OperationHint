# OperationHint

Operation Hint is an application layer pattern for informing the persistence layer in a decoupled way about the current operation to be performed.

This library offers a flexible solution for adding hints to the data repositories in your application,
inspired by EFCore's Query Hints.

These hints can be easily applied to query operations to modify the repository's behavior,
giving you greater control over data access.

The most basic use would be to apply the `Include` to the queries according to the needs of the operation. 
With Operation Hint, the application layer doesn't need to understand how relationships work or the technology of the persistence layer.

Operation Hint should be used with any repository and/or unit of work implementation.
The repository or unit of work interfaces should have the option of informing the operation using an `enum`.
In the implementations, the library components can be called to apply the modifications to the queries.

## NuGet Packages

```sh
dotnet add package RoyalCode.OperationHint.Abstractions
```

```sh
dotnet add package RoyalCode.OperationHint.EntityFramework
```

## Examples of implementation and use

Let's take two interfaces as an example, one for a repository and one for a work unit:

```cs
public interface IRepository<T>
    where T : class
{
    public T? Find(object id);

    public IQueryable<T> Query();
}
```

```cs
public interface IUnitOfWork
{
    public void AddHint(Enum hint);

    public void SaveChanges();
}
```

This implementation is very brief, just to illustrate the use of this library's components.

Now let's look at the implementation of the work unit:

```cs
using Microsoft.EntityFrameworkCore;
using RoyalCode.OperationHint.Abstractions;

public class DefaultUnitOfWork<TDb> : IUnitOfWork
    where TDb : DbContext
{
    private readonly TDb db;
    private readonly IHintsContainer hintsContainer;

    public DefaultUnitOfWork(TDb db, IHintsContainer hintsContainer)
    {
        this.db = db;
        this.hintsContainer = hintsContainer;
    }

    public void AddHint(Enum hint)
    {
        hintsContainer.AddHint(hint);
    }

    public void SaveChanges()
    {
        db.SaveChanges();
    }
}
```

And the implementation of the repository:

```cs
using Microsoft.EntityFrameworkCore;
using RoyalCode.OperationHint.Abstractions;

public class DefaultRepository<TEntity, TDb> : IRepository<TEntity>
    where TDb : DbContext
    where TEntity : class
{
    private readonly TDb db;
    private readonly IHintPerformer hintPerformer;

    public DefaultRepository(TDb db, IHintPerformer hintPerformer)
    {
        this.db = db;
        this.hintPerformer = hintPerformer;
    }

    public TEntity? Find(object id)
    {
        var entity = db.Set<TEntity>().Find(id);
        if (entity is not null)
            hintPerformer.Perform<TEntity, DbContext>(entity, db);
        return entity;
    }

    public IQueryable<TEntity> Query()
    {
        IQueryable<TEntity> query = db.Set<TEntity>();
        query = hintPerformer.Perform(query);
        return query;
    }
}
```

Finally, you need to configure the hints handler.
This can be done via extension methods for the `IServiceCollection`.

To show an example, I'll take a part of the implementation of the unit tests, see below:

```cs
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
```

Explanations:
- `services` is an object of `IServiceCollection`.
- `ConfigureOperationHints` is the extension method for configuring hint handlers.
- `registry` is an object of `IHintHandlerRegistry`, where hints handlers are registered.
- `AddIncludesHandler<TEntity, THint>` is a method for registering a hint handler. The generic types are the entity to be handled and the type of the hint to be entered.
- `(hint, includes)` are the parameters of the function that handles the hint. `hint` is the value of the `enum` (for example, entered in the unit of work). `includes` is an object of `Includes<TEntity>`, specifically for adding `Include` to EFCore queries.

