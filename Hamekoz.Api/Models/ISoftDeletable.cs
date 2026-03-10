namespace Hamekoz.Api.Models;

/// <summary>
/// Marks an entity as supporting soft deletion.
/// Instead of being physically removed from the database, the entity is flagged as deleted.
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// Indicates whether the entity has been soft-deleted.
    /// </summary>
    bool IsDeleted { get; set; }

    /// <summary>
    /// The UTC date and time when the entity was soft-deleted.
    /// </summary>
    DateTime? DeletedAt { get; set; }
}
