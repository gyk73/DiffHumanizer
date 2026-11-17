using System;

namespace DiffHumanizer.Annotations
{
    /// <summary>
    /// Marks a property that determines whether an entity should be excluded from comparison processing.
    /// The attribute works with different property types to evaluate whether the entity should be ignored:
    /// - For bool properties: entity is ignored when the property value is true
    /// - For int properties: entity is ignored when the property value is greater than 0
    /// - For string properties: entity is ignored when the property value is non-empty
    /// </summary>
    /// <remarks>
    /// This attribute is typically used with nested collection items to mark entities that should not appear in comparison reports.
    /// The attribute behavior depends on the property type:
    /// - Boolean: Use to mark deletion flags (true = ignore/deleted)
    /// - Integer: Use to mark soft-delete indicators (value > 0 = ignore)
    /// - String: Use to mark reason fields (non-empty = ignore with reason)
    /// Only one property per entity should be marked with this attribute.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class HumanizerIgnoreEntityAttribute : Attribute
    {
    }
}
