
using Microsoft.EntityFrameworkCore;
using RoyalCode.OperationHint.Abstractions;

namespace RoyalCode.OperationHint.Tests.UseExamples;

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