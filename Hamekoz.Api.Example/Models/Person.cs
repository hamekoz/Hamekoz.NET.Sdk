using Hamekoz.Api.Models;

namespace Hamekoz.Api.Example.Models;

public class Person : IEntity
{
    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public Gender Gender { get; set; }
}

public enum Gender
{
    PreferNotToSay,
    Female,
    Male,
    NonBinary,
    Other,
}