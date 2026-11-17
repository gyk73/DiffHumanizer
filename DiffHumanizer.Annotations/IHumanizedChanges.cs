namespace DiffHumanizer.Annotations
{
    /// <summary>
    /// Provides custom humanization logic for property differences in an entity.
    /// Implement this interface to provide custom descriptions of changes for specific entity types.
    /// </summary>
    /// <typeparam name="T">The entity type to provide custom humanization for</typeparam>
    /// <remarks>
    /// This interface allows entities to provide custom descriptions of how they have changed.
    /// The GetHumanizedChanges method is called during comparison to obtain a human-readable description.
    /// Note: The compareto parameter may be null in certain scenarios (e.g., entity creation or deletion).
    /// Implementations should handle null values gracefully.
    /// </remarks>
    public interface IHumanizedChanges<T> where T : class
    {
        /// <summary>
        /// Gets a humanized description of changes between this instance and another instance.
        /// </summary>
        /// <param name="compareto">The instance to compare against. May be null during creation or deletion scenarios.</param>
        /// <returns>A human-readable string describing the changes, or null if there are no changes to report</returns>
        string GetHumanizedChanges(T compareto);
    }
}
