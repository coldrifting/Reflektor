using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Reflektor;

public static class Extensions
{
    // Types and Reflection
    public static bool IsStruct(this Type type)
    {
        return type is { IsValueType: true, IsPrimitive: false };
    }
    
    public static bool IsGenericList(this Type type)
    {
        return (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(List<>)));
    }
    
    public static bool HasSetMethod(this MemberInfo memberInfo) => memberInfo switch
    {
        PropertyInfo propertyInfo => propertyInfo.GetSetMethod() is not null,
        _ => true
    };

    public static string GetName(this MemberInfo memberInfo) => memberInfo switch
    {
        PropertyInfo propertyInfo => propertyInfo.Name,
        FieldInfo fieldInfo => fieldInfo.Name,
        _ => "[n]"
    };

    public static object? GetValue(this MemberInfo memberInfo, object obj) => memberInfo switch
    {
        PropertyInfo propertyInfo => propertyInfo.GetValue(obj),
        FieldInfo fieldInfo => fieldInfo.GetValue(obj),
        _ => null
    };

    public static void SetValue(this MemberInfo memberInfo, object obj, object val)
    {
        switch (memberInfo)
        {
            case PropertyInfo propertyInfo:
                if (propertyInfo.SetMethod != null)
                {
                    propertyInfo.SetValue(obj, val);
                }
                break;
            case FieldInfo fieldInfo:
                fieldInfo.SetValue(obj, val);
                break;
        }
    }
    
    public static IEnumerable<FieldInfo> GetAllFields(this Type? t)
    {
        if (t == null)
        {
            return Enumerable.Empty<FieldInfo>();
        }

        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | 
                                   BindingFlags.Static | BindingFlags.Instance | 
                                   BindingFlags.DeclaredOnly;
        return t.GetFields(flags).Concat(GetAllFields(t.BaseType));
    }
    
    public static IEnumerable<PropertyInfo> GetAllProperties(this Type? t)
    {
        if (t == null)
        {
            return Enumerable.Empty<PropertyInfo>();
        }

        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | 
                                   BindingFlags.Static | BindingFlags.Instance | 
                                   BindingFlags.DeclaredOnly;
        return t.GetProperties(flags).Concat(GetAllProperties(t.BaseType));
    }

    public static string GetShortName(this object obj, bool addPrefix = false)
    {
        var fullName = obj.GetType().FullName;
        if (fullName == null)
        {
            return "";
        }

        string shortName = fullName.Split(".").Last();
        if (!addPrefix)
        {
            return shortName;
        }

        return obj switch
        {
            Component => $"[C] {shortName}",
            GameObject => $"[G] {shortName}",
            _ => shortName
        };
    }
    
    // Misc
    public static void Reorder<T,TE>(this Dictionary<object, (int, T, TE)> dict)
    {
        List<(object, int, T, TE)> l = new(dict.Values.Count);
        l.AddRange(dict.Select(v => (v.Key, v.Value.Item1, v.Value.Item2, v.Value.Item3)));
        l.Sort(((tuple, valueTuple) => tuple.Item2.CompareTo(valueTuple.Item2)));

        dict.Clear();

        int counter = 0;
        foreach (var ax in l)
        {
            dict.Add(ax.Item1, (counter++, ax.Item3, ax.Item4));
        }
    }
    
    public static string? Truncate(this string? value, int maxLength, string truncationSuffix = "…")
    {
        return value?.Trim().Length > maxLength
            ? value.Trim()[..maxLength] + truncationSuffix
            : value;
    }

    public static string SetEnabledColor(this object? input, bool enable)
    {
        return enable
            ? WrapColor(input, "ffffff")
            : WrapColor(input, "777777");
    }

    private static string WrapColor(this object? input, string hexColor)
    {
        return $"<color=#{hexColor}>{input}</color>";
    }
    
    public static string StripHtml(this string input)
    {
        return Regex.Replace(input, "<.*?>", string.Empty);
    }
    
    public static string GetPath(this GameObject obj)
    {
        string path = "/" + obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = "/" + obj.name + path;
        }
        return path;
    }
}