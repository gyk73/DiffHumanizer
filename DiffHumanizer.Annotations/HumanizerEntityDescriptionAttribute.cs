using System;

namespace DiffHumanizer.Annotations
{
    /// <summary>
    /// Marks a property that contains an optional description of an entity.
    /// This value is used in comparison reports to provide additional context about the entity.
    /// </summary>
    /// <remarks>
    /// Only one property per entity should be marked with this attribute.
    /// The property can return any type; non-string values will be converted to strings using ToString().
    /// If the property returns null, no description will be included in the report.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class HumanizerEntityDescriptionAttribute : Attribute
    {
    }
}
