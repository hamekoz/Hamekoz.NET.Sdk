using System.Text.Json.Serialization;

namespace Hamekoz.Api.Models;

public class IEntity
{
    [JsonPropertyOrder(1)]
    public required int Id { get; init; }
}
