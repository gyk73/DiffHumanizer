using System;

namespace DiffHumanizer.Abstractions
{
    /// <summary>
    /// Defines the contract for formatting object values to human-readable strings.
    /// This abstraction enables extensibility by allowing custom formatters for specific types or domains.
    /// </summary>
    /// <remarks>
    /// Implementations of this interface are responsible for converting values of various types
    /// to their string representations suitable for human consumption. The default implementation
    /// (DefaultValueFormatter) handles null values, booleans, and standard ToString conversion,
    /// using configuration for boolean representation (TrueValue/FalseValue).
    /// 
    /// Custom implementations can be provided to:
    /// - Format specific types (DateTime, Currency, etc.) with domain-specific conventions
    /// - Apply localization for boolean or null representations
    /// - Implement custom type-based formatting rules
    /// </remarks>
    public interface IValueFormatter
    {
        /// <summary>
        /// Converts the specified value to a human-readable string representation.
        /// </summary>
        /// <param name="valueType">The type of the value being formatted. Must not be null.</param>
        /// <param name="value">The value to format. May be null for reference types and nullable value types.</param>
        /// <returns>
        /// The formatted string representation of the value. Should never return null;
        /// return "null" for null values instead.
        /// </returns>
        /// <remarks>
        /// The formatter must handle:
        /// - Null reference type values (return "null")
        /// - Null nullable value type values (return "null")
        /// - Boolean and nullable boolean values (typically using configuration)
        /// - All other values (typically via ToString())
        /// </remarks>
        string Format(Type valueType, object value);
    }
}
