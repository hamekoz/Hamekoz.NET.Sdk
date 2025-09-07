using Hamekoz.Api.Dtos;

namespace Hamekoz.Api.Example.Dtos;

public class PersonUpdateDto : IUpdateDto
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }
}

