using Hamekoz.Api.Dtos;

namespace Hamekoz.Api.Example.Dtos;

public class PersonDetailDto : IDetailDto
{
    public int Id { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Gender { get; set; }

    public string? DateOfBirth { get; set; }

    public int Age { get; set; }
}

