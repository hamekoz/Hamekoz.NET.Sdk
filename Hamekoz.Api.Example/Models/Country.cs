using Hamekoz.Api.Dtos;
using Hamekoz.Api.Models;

namespace Hamekoz.Api.Example.Models;

public class Country : Entity, IFullDto
{
    public required string Name { get; set; }

}
