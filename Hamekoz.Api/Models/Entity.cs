using System.Text.Json.Serialization;

namespace Hamekoz.Api.Models;

public abstract class Entity
{
    [JsonPropertyOrder(1)]
    public int Id { get; init; }
}
