using System;

namespace DiffHumanizer.Formatting
{
    /// <summary>
    /// Default implementation of IValueFormatter that provides standard value-to-string conversion.
    /// </summary>
    /// <remarks>
    /// This formatter handles:
    /// - Null values (reference types): formatted as "null"
    /// - Null values (nullable value types): formatted as "null"
    /// - Boolean values (both bool and bool?): formatted using config.TrueValue or config.FalseValue
    /// - All other values: formatted using ToString()
    /// 
    /// Custom formatters can extend or replace this behavior by implementing IValueFormatter
    /// and providing different formatting logic for specific types.
    /// </remarks>
    public class DefaultValueFormatter : Abstractions.IValueFormatter
    {
        private readonly DifferenceHumanizerConfiguration _config;

        /// <summary>
        /// Initializes a new instance of DefaultValueFormatter with the specified configuration.
        /// </summary>
        /// <param name="config">
        /// The configuration containing TrueValue and FalseValue for boolean formatting.
        /// Typically obtained from DifferenceHumanizerConfiguration.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if config is null.</exception>
        public DefaultValueFormatter(DifferenceHumanizerConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Converts the specified value to a human-readable string representation.
        /// </summary>
        /// <remarks>
        /// Formatting rules applied in order:
        /// 1. If value is null and type is reference type, return "null"
        /// 2. If value is null and type is nullable value type, return "null"
        /// 3. If type is nullable bool and value is null, return "null"
        /// 4. If type is nullable bool and value is not null, return config.TrueValue or config.FalseValue
        /// 5. If type is bool, return config.TrueValue or config.FalseValue
        /// 6. Otherwise, return value.ToString()
        /// </remarks>
        /// <param name="valueType">
        /// The type of the value being formatted. Must not be null.
        /// </param>
        /// <param name="value">The value to format. May be null.</param>
        /// <returns>
        /// The formatted string representation of the value. Never returns null;
        /// the string "null" is returned for null values.
        /// </returns>
        public string Format(Type valueType, object value)
        {
            if (valueType == null)
            {
                throw new ArgumentNullException(nameof(valueType));
            }

            // Null reference types
            if (!valueType.IsValueType && value == null)
            {
                return "null";
            }

            // Null nullable value types
            if (Nullable.GetUnderlyingType(valueType) != null && value == null)
            {
                return "null";
            }

            // Nullable boolean
            if (Nullable.GetUnderlyingType(valueType) == typeof(bool))
            {
                return ((bool?)value).Value ? _config.TrueValue : _config.FalseValue;
            }

            // Non-nullable boolean
            if (valueType == typeof(bool))
            {
                return (bool)value ? _config.TrueValue : _config.FalseValue;
            }

            return value.ToString();
        }
    }
}
