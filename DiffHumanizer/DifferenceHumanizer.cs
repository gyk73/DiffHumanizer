using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using DiffHumanizer.Annotations;

namespace DiffHumanizer
{
    public class DifferenceHumanizer
    {
        private DifferenceHumanizerConfiguration config;
        private Dictionary<string, string> ignorePairs;        
        public bool TrimStrings { get; set; }
        public bool ReturnNullWhenNoChanges { get; set; }

        public DifferenceHumanizer(DifferenceHumanizerConfiguration config, Dictionary<string, string> ignorePairs = null)
        {
            this.config = config;
            this.ignorePairs = ignorePairs;
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

        public static string GetHumanizedEntityName(object obj)
        {
            var typeInfo = obj.GetType().GetTypeInfo();
            var props = typeInfo.GetProperties();
            foreach (var propInfo in props)
            {
                var nameattr = propInfo.GetCustomAttribute<HumanizerEntityNameAttribute>();
                if (nameattr != null)
                {
                    return (string)propInfo.GetValue(obj);
                }
            }

            var attr = typeInfo.GetCustomAttribute<DisplayAttribute>();
            if (attr != null)
            {
                return attr.Name;
            }
            else
            {
                return obj.GetType().Name;
            }
        }

        public static string GetHumanizedEntityDescription(object obj)
        {
            var typeInfo = obj.GetType().GetTypeInfo();
            var props = typeInfo.GetProperties();
            foreach (var propInfo in props)
            {
                var attr = propInfo.GetCustomAttribute<HumanizerEntityDescriptionAttribute>();
                if (attr != null)
                {
                    if (propInfo.PropertyType == typeof(string))
                    {
                        return (string)propInfo.GetValue(obj);
                    }
                    else
                    {
                        return propInfo.GetValue(obj)?.ToString();
                    }
                }
            }

            return null;
        }

        public static bool IsEntityShouldBeIgnored(object obj)
        {
            var typeInfo = obj.GetType().GetTypeInfo();
            var props = typeInfo.GetProperties();
            foreach (var propInfo in props)
            {
                var attr = propInfo.GetCustomAttribute<HumanizerIgnoreEntityAttribute>();
                if (attr != null)
                {
                    if (propInfo.PropertyType == typeof(bool))
                    {
                        return (bool)propInfo.GetValue(obj);
                    }
                    else
                    if (propInfo.PropertyType == typeof(int))
                    {
                        return (int)propInfo.GetValue(obj)>0;
                    }
                    else
                    if (propInfo.PropertyType == typeof(string))
                    {
                        return !string.IsNullOrEmpty((string)propInfo.GetValue(obj));
                    }
                }
            }

            return false;
        }

        private static string GetItemKey(object item)
        {
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
                    return value.ToString();
                }
            }

            return null;
        }

        private string GetValueString(object value, Type type)
        {
            if (!type.IsValueType && value == null)
            {
                return "null";
            }
            if (Nullable.GetUnderlyingType(type) != null && value == null)
            {
                return "null";
            }

            if (Nullable.GetUnderlyingType(type) == typeof(bool))
            {
                if (((bool?)value).Value)
                    return config.TrueValue;
                else
                    return config.FalseValue;
            }

            if (type == typeof(bool))
            {
                if ((bool)value)
                    return config.TrueValue;
                else
                    return config.FalseValue;
            }

            return value.ToString();
        }

        // For objects whose implement interface IHumanizedChanges<T>
        public static string GetCustomHumanizedPropertyDifferences<T>(T object1, T object2) where T : class 
        {
            var obj = object1 ?? object2;
            if ((obj as IHumanizedChanges<T>) == null)
            {
                throw new Exception("Object must implement interface IHumanizedChanges<T>!");
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

            foreach (var propInfo in props)
            {
                if (propInfo.GetCustomAttribute<HumanizerIgnoreAttribute>() != null)
                {
                    continue;
                }

                var displayAttr = propInfo.GetCustomAttribute<DisplayAttribute>();
                if (displayAttr == null || string.IsNullOrEmpty(displayAttr.Name))
                {
                    continue;
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
                                diffStr.Add((str + displayAttr.Name + " " + descr).TrimEnd());

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
                                var str = (config.ModifiedItemOperation + " " + displayAttr.Name + " " + descr).TrimEnd();
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
                                diffStr.Add((str + displayAttr.Name + " " + descr).TrimEnd());
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
                }
                else
                if (oldStrValue != strValue)
                {
                    result = true;
                    if (object1 != null && object2 != null)
                    {
                        diffStr.Add($"{displayAttr.Name} : \"{oldStrValue}\" => \"{strValue}\"");
                    }
                    else
                    if (object1 != null)
                    {
                        diffStr.Add($"{displayAttr.Name} : \"{oldStrValue}\"");
                    }
                    else
                    if (object2 != null)
                    {
                        diffStr.Add($"{displayAttr.Name} : \"{strValue}\"");
                    }
                }
            }

            return result;
        }
    }
}
