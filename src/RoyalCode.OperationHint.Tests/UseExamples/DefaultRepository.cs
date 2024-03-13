
using Microsoft.EntityFrameworkCore;
using RoyalCode.OperationHint.Abstractions;

namespace RoyalCode.OperationHint.Tests.UseExamples;

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

        if (hintPerformer is not null && entity is not null)
            hintPerformer.Perform<TEntity, DbContext>(entity, db);

        return entity;
    }

    public IQueryable<TEntity> Query()
    {
        IQueryable<TEntity> query = db.Set<TEntity>();

        if (hintPerformer is not null)
            query = hintPerformer.Perform(query);

        return query;
    }
}