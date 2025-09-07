using Hamekoz.Api.Dtos;

namespace Hamekoz.Api.Example.Dtos;

public class PersonCreateDto : ICreateDto
{
    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public string? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }
}
