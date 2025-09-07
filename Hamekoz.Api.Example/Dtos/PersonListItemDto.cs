using Hamekoz.Api.Dtos;

namespace Hamekoz.Api.Example.Dtos;

public class PersonListItemDto : IListItemDto
{
    public int Id { get; set; }

    public required string Name { get; set; }
}

