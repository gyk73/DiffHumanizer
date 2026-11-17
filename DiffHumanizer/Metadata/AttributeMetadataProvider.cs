using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using DiffHumanizer.Abstractions;
using DiffHumanizer.Annotations;

namespace DiffHumanizer.Metadata
{
    /// <summary>
    /// Implementation of IEntityMetadataProvider that uses .NET attributes to provide metadata.
    /// Supports DiffHumanizer-specific attributes and standard DataAnnotations.Display attribute.
    /// </summary>
    public class AttributeMetadataProvider : IEntityMetadataProvider
    {
        // Cache reflection results for performance
        private static readonly Dictionary<Type, PropertyInfo[]> PropertyCache = new Dictionary<Type, PropertyInfo[]>();

        public string GetEntityName(object entity)
        {
            if (entity == null)
            {
                return null;
            }

            var typeInfo = entity.GetType().GetTypeInfo();
            var props = GetCachedProperties(entity.GetType());

            // First, check for HumanizerEntityNameAttribute
            foreach (var propInfo in props)
            {
                var nameAttr = propInfo.GetCustomAttribute<HumanizerEntityNameAttribute>();
                if (nameAttr != null)
                {
                    var value = propInfo.GetValue(entity);
                    return value?.ToString();
                }
            }

            // Fall back to Display attribute on the entity type
            var displayAttr = typeInfo.GetCustomAttribute<DisplayAttribute>();
            if (displayAttr != null)
            {
                return displayAttr.Name;
            }

            // Fall back to type name
            return entity.GetType().Name;
        }

        public string GetEntityDescription(object entity)
        {
            if (entity == null)
            {
                return null;
            }

            var props = GetCachedProperties(entity.GetType());

            foreach (var propInfo in props)
            {
                var descrAttr = propInfo.GetCustomAttribute<HumanizerEntityDescriptionAttribute>();
                if (descrAttr != null)
                {
                    var value = propInfo.GetValue(entity);
                    if (value == null)
                    {
                        return null;
                    }

                    if (propInfo.PropertyType == typeof(string))
                    {
                        return (string)value;
                    }
                    else
                    {
                        return value.ToString();
                    }
                }
            }

            return null;
        }

        public bool ShouldIgnoreEntity(object entity)
        {
            if (entity == null)
            {
                return false;
            }

            var props = GetCachedProperties(entity.GetType());

            foreach (var propInfo in props)
            {
                var ignoreAttr = propInfo.GetCustomAttribute<HumanizerIgnoreEntityAttribute>();
                if (ignoreAttr != null)
                {
                    var value = propInfo.GetValue(entity);

                    if (propInfo.PropertyType == typeof(bool))
                    {
                        return (bool)value;
                    }
                    else if (propInfo.PropertyType == typeof(int))
                    {
                        return (int)value > 0;
                    }
                    else if (propInfo.PropertyType == typeof(string))
                    {
                        return !string.IsNullOrEmpty((string)value);
                    }
                }
            }

            return false;
        }

        public bool ShouldIgnoreProperty(PropertyInfo property)
        {
            if (property == null)
            {
                return false;
            }

            return property.GetCustomAttribute<HumanizerIgnoreAttribute>() != null;
        }

        public string GetPropertyDisplayName(PropertyInfo property)
        {
            if (property == null)
            {
                return null;
            }

            var displayAttr = property.GetCustomAttribute<DisplayAttribute>();
            if (displayAttr != null)
            {
                return displayAttr.Name;
            }

            return null;
        }

        public string GetCompositeKey(object entity)
        {
            if (entity == null)
            {
                return null;
            }

            string result = null;
            var props = GetCachedProperties(entity.GetType());

            foreach (var prop in props)
            {
                var keyAttr = prop.GetCustomAttribute<KeyAttribute>();
                if (keyAttr != null)
                {
                    var value = prop.GetValue(entity);
                    if (value == null)
                    {
                        return null;
                    }

                    if (!string.IsNullOrEmpty(result))
                    {
                        result += "|";
                    }

                    result += value.ToString();
                }
            }

            return result;
        }

        private PropertyInfo[] GetCachedProperties(Type type)
        {
            if (!PropertyCache.TryGetValue(type, out var props))
            {
                props = type.GetProperties();
                PropertyCache[type] = props;
            }

            return props;
        }
    }
}
