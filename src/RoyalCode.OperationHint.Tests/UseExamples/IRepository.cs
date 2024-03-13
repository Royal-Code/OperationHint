namespace RoyalCode.OperationHint.Tests.UseExamples;

public interface IRepository<T>
    where T : class
{
    public T? Find(object id);

    public IQueryable<T> Query();
}
