using RoyalCode.Entities;

namespace RoyalCode.OperationHint.Tests;

public class SimpleEntity : Entity<int>
{
    public SimpleEntity()
    {
        // new random int
        Id = new Random().Next();
    }

    public string Name { get; set; } = null!;
}
