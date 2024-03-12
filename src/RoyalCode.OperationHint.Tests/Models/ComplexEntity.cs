using RoyalCode.Entities;

namespace RoyalCode.OperationHint.Tests.Models;

public class ComplexEntity : Entity<int>
{
    public ComplexEntity()
    {
        // new random int
        Id = new Random().Next();
    }

    public string Name { get; set; } = null!;

    public SimpleEntity SingleRelation { get; set; } = null!;

    public ICollection<SimpleEntity> MultipleRelation { get; set; } = null!;
}