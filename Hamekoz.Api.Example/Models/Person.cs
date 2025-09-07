using Hamekoz.Api.Models;

namespace Hamekoz.Api.Example.Models;

public class Person : Entity
{
    public int Id { get; init; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public Gender Gender { get; set; }

    public DateOnly DateOfBirth { get; set; }
}

public enum Gender
{
    PreferNotToSay,
    Female,
    Male,
    NonBinary,
    Other,
}
