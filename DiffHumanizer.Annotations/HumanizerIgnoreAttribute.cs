using System;

namespace DiffHumanizer.Annotations
{
    /// <summary>
    /// Marks a property to be excluded from difference comparison reports.
    /// Properties with this attribute will not be analyzed for changes and will not appear in the output.
    /// </summary>
    /// <remarks>
    /// This is useful for excluding internal tracking fields, computed properties, or sensitive data from comparison output.
    /// Note: If a Display attribute is also applied to the same property, the Display attribute is ignored when this attribute is present.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class HumanizerIgnoreAttribute : Attribute
    {
    }
}
