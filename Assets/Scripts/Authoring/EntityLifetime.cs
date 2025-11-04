using Unity.Entities;

/// <summary>
/// Component that tracks when an entity should be destroyed based on elapsed time.
/// Used for entities that have a limited lifetime (like projectiles).
/// </summary>
public struct EntityLifetime : IComponentData
{
    /// <summary>
    /// The time (in seconds) when this entity should be destroyed.
    /// Compared against SystemAPI.Time.ElapsedTime.
    /// </summary>
    public double DestroyAtTime;
}