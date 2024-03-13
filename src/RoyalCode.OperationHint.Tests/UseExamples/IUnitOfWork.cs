namespace RoyalCode.OperationHint.Tests.UseExamples;

public interface IUnitOfWork
{
    public void AddHint(Enum hint);

    public void SaveChanges();
}