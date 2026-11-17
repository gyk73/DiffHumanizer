using System;
using System.Reflection;

namespace DiffHumanizer.Abstractions
{
    /// <summary>
    /// Provides abstraction for reading entity metadata used in difference comparison.
    /// This allows multiple implementations (attribute-based, configuration-based, convention-based, etc.)
    /// to provide metadata without modifying the core comparison engine.
    /// </summary>
    public interface IEntityMetadataProvider
    {
        /// <summary>
        /// Gets the human-readable name or identifier for an entity instance.
        /// </summary>
        /// <param name="entity">The entity instance to get the name for</param>
        /// <returns>The entity name, or null if no name is available</returns>
        string GetEntityName(object entity);

        /// <summary>
        /// Gets an optional description for an entity instance.
        /// </summary>
        /// <param name="entity">The entity instance to get the description for</param>
        /// <returns>The entity description, or null if no description is available</returns>
        string GetEntityDescription(object entity);

        /// <summary>
        /// Determines whether an entity should be ignored during comparison processing.
        /// </summary>
        /// <param name="entity">The entity instance to evaluate</param>
        /// <returns>true if the entity should be ignored; otherwise false</returns>
        bool ShouldIgnoreEntity(object entity);

        /// <summary>
        /// Determines whether a specific property should be ignored during difference comparison.
        /// </summary>
        /// <param name="property">The property to evaluate</param>
        /// <returns>true if the property should be ignored; otherwise false</returns>
        bool ShouldIgnoreProperty(PropertyInfo property);

        /// <summary>
        /// Gets the display name for a property.
        /// </summary>
        /// <param name="property">The property to get the display name for</param>
        /// <returns>The property display name, or null if no display name is defined</returns>
        string GetPropertyDisplayName(PropertyInfo property);

        /// <summary>
        /// Extracts the composite key value(s) from an entity instance.
        /// Used to match items in collections during deep comparison.
        /// </summary>
        /// <param name="entity">The entity instance to extract the key from</param>
        /// <returns>A string representation of the composite key, or null if no key can be extracted</returns>
        string GetCompositeKey(object entity);
    }
}
