using System;

namespace DiffHumanizer.Annotations
{
    /// <summary>
    /// Marks a property that contains the human-readable name or identifier of an entity.
    /// This value is used in comparison reports to help identify the entity being compared.
    /// The property value should typically be a string representation (e.g., entity name, display name, or ID).
    /// </summary>
    /// <remarks>
    /// Only one property per entity should be marked with this attribute.
    /// The property should return a string value; non-string return values will be converted to strings.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class HumanizerEntityNameAttribute : Attribute
    {
    }
}
