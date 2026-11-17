using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using DiffHumanizer.Annotations;
using DiffHumanizer.Abstractions;
using DiffHumanizer.Metadata;
using DiffHumanizer.Formatting;

namespace DiffHumanizer
{
    public class DifferenceHumanizer
    {
        private readonly DifferenceHumanizerConfiguration config;
        private readonly Dictionary<string, string> ignorePairs;
        private readonly bool processAllFields;
        private readonly IEntityMetadataProvider metadataProvider;
        private readonly IValueFormatter valueFormatter;

        public bool TrimStrings { get; set; }
        public bool ReturnNullWhenNoChanges { get; set; }

        /// <summary>
        /// Initializes a new instance of DifferenceHumanizer with dependency injection support.
        /// </summary>
        /// <remarks>
        /// This constructor supports optional dependency injection for extensibility:
        /// - If no metadataProvider is specified, uses AttributeMetadataProvider (reads .NET attributes)
        /// - If no valueFormatter is specified, uses DefaultValueFormatter (standard value-to-string conversion)
        /// 
        /// This design follows the Dependency Inversion Principle, allowing custom implementations
        /// of IEntityMetadataProvider and IValueFormatter to be substituted without modifying
        /// the core DifferenceHumanizer logic.
        /// </remarks>
        /// <param name="config">
        /// The configuration object containing humanization preferences (operation names, boolean representations, etc.).
        /// This parameter is required.
        /// </param>
        /// <param name="ignorePairs">
        /// Optional dictionary of value pairs to ignore during comparison.
        /// When both old and new values match a pair in this dictionary, the difference is skipped.
        /// </param>
        /// <param name="processAllFields">
        /// If true, processes all fields including those without [Display] attribute.
        /// If false (default), only processes fields with [Display] attribute.
        /// </param>
        /// <param name="metadataProvider">
        /// Optional custom metadata provider for entity metadata (name, description, ignore rules).
        /// If null, uses AttributeMetadataProvider by default.
        /// </param>
        /// <param name="valueFormatter">
        /// Optional custom value formatter for object-to-string conversion.
        /// If null, uses DefaultValueFormatter by default.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if config is null.</exception>
        public DifferenceHumanizer(DifferenceHumanizerConfiguration config, Dictionary<string, string> ignorePairs = null, bool processAllFields = false, IEntityMetadataProvider metadataProvider = null, IValueFormatter valueFormatter = null)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.ignorePairs = ignorePairs;
            this.processAllFields = processAllFields;
            this.metadataProvider = metadataProvider != null ? metadataProvider : (IEntityMetadataProvider)new AttributeMetadataProvider();
            this.valueFormatter = valueFormatter != null ? valueFormatter : new DefaultValueFormatter(config);
        }

        public string HumanizedOperationToString(DifferenceHumanizerOperation operation)
        {
            if (operation == DifferenceHumanizerOperation.New)
            {
                return config.NewItemOperation;
            }
            else
            if (operation == DifferenceHumanizerOperation.Delete)
            {
                return config.DeletedItemOperation;
            }
            else
            {
                return config.ModifiedItemOperation;
            }
        }

        public DifferenceHumanizerOperation GetHumanizedOperation(object object1, object object2, bool isDeletion)
        {
            var operation = DifferenceHumanizerOperation.Modify;
            if (object1 == null)
            {
                operation = DifferenceHumanizerOperation.New;
            }
            else
            if (object2 == null)
            {
                operation = DifferenceHumanizerOperation.Delete;
            }
            else
            {
                var id1 = GetItemKey(object1);
                var id2 = GetItemKey(object2);
                if (!string.IsNullOrEmpty(id1) && id1 == "0")
                {
                    id1 = null;
                }
                if (!string.IsNullOrEmpty(id2) && id2 == "0")
                {
                    id2 = null;
                }

                if (string.IsNullOrEmpty(id1))
                {
                    if (string.IsNullOrEmpty(id2))
                    {
                        operation = DifferenceHumanizerOperation.Modify;
                    }
                    else
                    {
                        operation = DifferenceHumanizerOperation.New;
                    }
                }
                else
                if (string.IsNullOrEmpty(id2))
                {
                    operation = DifferenceHumanizerOperation.Delete;
                }
            }

            if (isDeletion && operation != DifferenceHumanizerOperation.Modify)
            {
                if (operation == DifferenceHumanizerOperation.New)
                {
                    operation = DifferenceHumanizerOperation.Delete;
                }
                else
                {
                    operation = DifferenceHumanizerOperation.New;
                }
            }

            return operation;
        }

        public string GetHumanizedEntityName(object obj)
        {
            return metadataProvider.GetEntityName(obj);
        }

        public string GetHumanizedEntityDescription(object obj)
        {
            return metadataProvider.GetEntityDescription(obj);
        }

        public bool IsEntityShouldBeIgnored(object obj)
        {
            return metadataProvider.ShouldIgnoreEntity(obj);
        }

        private string GetItemKey(object item)
        {
            string result = null;
            var props = item.GetType().GetProperties();
            foreach (var prop in props)
            {
                if (prop.GetCustomAttribute<KeyAttribute>() != null)
                {
                    var value = prop.GetValue(item);
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

        private string GetValueString(object value, Type type)
        {
            return valueFormatter.Format(type, value);
        }

        // For objects whose implement interface IHumanizedChanges<T>
        public string GetCustomHumanizedPropertyDifferences<T>(T object1, T object2) where T : class 
        {
            var obj = object1 ?? object2;
            if ((obj as IHumanizedChanges<T>) == null)
            {
                throw new InvalidOperationException("Object must implement interface IHumanizedChanges<T>!");
            }

            var result = new StringBuilder();
            result.Append(GetHumanizedEntityName(object2));
            var descr = GetHumanizedEntityDescription(object2);
            if (!string.IsNullOrEmpty(descr))
            {
                result.Append(" \"");
                result.Append(descr);
                result.Append("\"");
            }
            result.Append(" : ");

            if (object1 != null)
            {
                result.Append((object1 as IHumanizedChanges<T>).GetHumanizedChanges(object2));
            }
            else
            {
                result.Append((object2 as IHumanizedChanges<T>).GetHumanizedChanges(object1));
            }
            return result.ToString();
        }
        
        public string GetHumanizedPropertyDifferences(object object1, object object2)
        {
            var obj = object1 ?? object2;
            if (obj == null)
            {
                return null;
            }

            var operation = GetHumanizedOperation(object1, object2, false);

            var builder = new StringBuilder();

            builder.Append(HumanizedOperationToString(operation));
            builder.Append(" ");
            builder.Append(GetHumanizedEntityName(obj));
            var descr = GetHumanizedEntityDescription(obj);
            if (!string.IsNullOrEmpty(descr))
            {
                builder.Append(" \"");
                builder.Append(descr);
                builder.Append("\"");
            }

            var diffStr = new List<string>();
            if (GetHumanizedPropertyDifferencesBase(object1, object2, operation, diffStr))
            {
                var isFirst = true;
                foreach(var str in diffStr)
                {
                    if (!isFirst)
                    {
                        builder.Append(",");
                    }
                    isFirst = false;
                    builder.Append(" ");
                    builder.Append(str);
                }

                return builder.ToString();
            }
            else
            {
                return null;
            }
        }

        private bool GetHumanizedPropertyDifferencesBase(object object1, object object2, DifferenceHumanizerOperation baseOperation, IList<string> diffStr)
        {
            var result = false;

            PropertyInfo[] props = null;
            if (object1 != null)
            {
                props = object1.GetType().GetProperties();
            }
            if (object2 != null)
            {
                props = object2.GetType().GetProperties();
            }

            if (props == null)
            {
                return false;
            }

            foreach (var propInfo in props)
            {
                if (propInfo.GetCustomAttribute<HumanizerIgnoreAttribute>() != null)
                {
                    continue;
                }

                var displayName = "";
                var displayAttr = propInfo.GetCustomAttribute<DisplayAttribute>();
                if (displayAttr != null)
                {
                    displayName = displayAttr.Name;
                }

                if (string.IsNullOrEmpty(displayName) && !processAllFields)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(displayName))
                {
                    displayName = propInfo.Name;
                }

                var strValue = "null";
                object value = null;
                if (object2 != null)
                {
                    value = propInfo.GetValue(object2);
                    strValue = GetValueString(value, propInfo.PropertyType);
                }
                string oldStrValue = "null";
                object oldValue = null;
                if (object1 != null)
                {
                    oldValue = propInfo.GetValue(object1);
                    oldStrValue = GetValueString(oldValue, propInfo.PropertyType);
                }

                if (!string.IsNullOrEmpty(strValue) && ignorePairs != null && ignorePairs.ContainsKey(strValue) && ignorePairs[strValue] == oldStrValue)
                {
                    continue;
                }
                if (!string.IsNullOrEmpty(oldStrValue) && ignorePairs != null && ignorePairs.ContainsKey(oldStrValue) && ignorePairs[oldStrValue] == strValue)
                {
                    continue;
                }

                if (propInfo.PropertyType == typeof(string) && TrimStrings && !string.IsNullOrEmpty(strValue) && !string.IsNullOrEmpty(oldStrValue))
                {
                    if (strValue.Trim() == oldStrValue.Trim())
                    {
                        continue;
                    }
                }

                if (typeof(IEnumerable).IsAssignableFrom(propInfo.PropertyType) && propInfo.PropertyType != typeof(string)
                    && (oldStrValue == null || (oldStrValue.GetType() == typeof(string) && (string)oldStrValue == "null") || !((IEnumerable)oldStrValue).GetEnumerator().MoveNext())
                    && (strValue == null || (strValue.GetType() == typeof(string) && (string)strValue == "null") || !((IEnumerable)strValue).GetEnumerator().MoveNext()))
                {
                    continue;
                }

                if ((value != null || oldValue != null) && propInfo.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(propInfo.PropertyType))
                {
                    if (CompareCollections(propInfo, oldValue, value, displayName, baseOperation, diffStr))
                    {
                        result = true;
                    }
                }
                else
                if (oldStrValue != strValue)
                {
                    result = true;
                    if (object1 != null && object2 != null)
                    {
                        diffStr.Add($"{displayName} : \"{oldStrValue}\" => \"{strValue}\"");
                    }
                    else
                    if (object1 != null)
                    {
                        diffStr.Add($"{displayName} : \"{oldStrValue}\"");
                    }
                    else
                    if (object2 != null)
                    {
                        diffStr.Add($"{displayName} : \"{strValue}\"");
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Compares two collections based on item keys and reports differences.
        /// </summary>
        /// <param name="propInfo">The property information for the collection.</param>
        /// <param name="oldValue">The old collection value (may be null).</param>
        /// <param name="value">The new collection value (may be null).</param>
        /// <param name="displayName">The display name of the collection property.</param>
        /// <param name="baseOperation">The base operation being performed (Create, Update, Delete).</param>
        /// <param name="diffStr">The list to which differences are added.</param>
        /// <returns>True if differences were found, false otherwise.</returns>
        private bool CompareCollections(PropertyInfo propInfo, object oldValue, object value, string displayName, DifferenceHumanizerOperation baseOperation, IList<string> diffStr)
        {
            var result = false;

            if (oldValue != null)
            {
                foreach (var oldItem in (IEnumerable)oldValue)
                {
                    if (IsEntityShouldBeIgnored(oldItem))
                      continue;

                    var key = GetItemKey(oldItem);
                    var isFound = false;
                    object foundNewItem = null;
                    if (value != null)
                    {
                        foreach (var newItem in (IEnumerable)value)
                        {
                            if (IsEntityShouldBeIgnored(newItem))
                                continue;
                            if (GetItemKey(newItem) == key)
                            {
                                foundNewItem = newItem;
                                isFound = true;
                                break;
                            }
                        }
                    }

                    if (!isFound)
                    {
                        result = true;
                        var str = "";
                        if (baseOperation == DifferenceHumanizerOperation.Modify)
                        {
                            str = config.DeletedItemOperation + " ";
                        }
                        var descr = GetHumanizedEntityDescription(oldItem);
                        if (!string.IsNullOrEmpty(descr))
                        {
                            descr = "\"" + descr + "\"";
                        }
                        diffStr.Add((str + displayName + " " + descr).TrimEnd());

                        var list2 = new List<string>();
                        GetHumanizedPropertyDifferencesBase(oldItem, null, baseOperation, list2);
                        if (list2.Count > 0)
                        {
                            diffStr[diffStr.Count - 1] = diffStr[diffStr.Count - 1] + " : " + list2[0];
                            for (var k = 1; k < list2.Count; k++)
                            {
                                diffStr.Add(list2[k]);
                            }
                        }
                    }
                    else
                    {
                        var descr = GetHumanizedEntityDescription(oldItem);
                        if (!string.IsNullOrEmpty(descr))
                        {
                            descr = "\"" + descr + "\"";
                        }
                        var str = (config.ModifiedItemOperation + " " + displayName + " " + descr).TrimEnd();
                        var list = new List<string>();
                        GetHumanizedPropertyDifferencesBase(oldItem, foundNewItem, baseOperation, list);
                        if (list.Count > 0)
                        {
                            result = true;
                            diffStr.Add(str + " : " + list[0]);
                            for (var k = 1; k < list.Count; k++)
                            {
                                diffStr.Add(list[k]);
                            }
                        }
                    }
                }
            }

            if (value != null)
            {
                foreach (var item in (IEnumerable)value)
                {
                    if (IsEntityShouldBeIgnored(item))
                        continue;

                    var itemKey = GetItemKey(item);
                    var isFound = false;
                    if (oldValue != null)
                    {
                        foreach (var oldValueItem in (IEnumerable)oldValue)
                        {
                            if (IsEntityShouldBeIgnored(oldValueItem))
                                continue;
                            if (GetItemKey(oldValueItem) == itemKey)
                            {
                                isFound = true;
                                break;
                            }
                        }
                    }

                    if (!isFound)
                    {
                        result = true;
                        var str = "";
                        if (baseOperation == DifferenceHumanizerOperation.Modify)
                        {
                            str = config.NewItemOperation + " ";
                        }
                        var descr = GetHumanizedEntityDescription(item);
                        if (!string.IsNullOrEmpty(descr))
                        {
                            descr = "\"" + descr + "\"";
                        }
                        diffStr.Add((str + displayName + " " + descr).TrimEnd());
                        var list = new List<string>();
                        GetHumanizedPropertyDifferencesBase(null, item, baseOperation, list);
                        if (list.Count > 0)
                        {
                            diffStr[diffStr.Count - 1] = diffStr[diffStr.Count - 1] + " : " + list[0];
                            for (var k = 1; k < list.Count; k++)
                            {
                                diffStr.Add(list[k]);
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}

