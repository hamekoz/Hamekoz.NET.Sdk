namespace Hamekoz.Api.Models;

/// <summary>
/// Base class for entities that track creation and modification timestamps.
/// </summary>
public abstract class AuditableEntity : Entity
{
    /// <summary>
    /// The UTC date and time when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The UTC date and time when the entity was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
