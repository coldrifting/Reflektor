using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Reflektor.Extensions;

public static class TypeExtensions
{
    public static bool IsStruct(this object obj)
    {
        Type type = obj.GetType();
        return type is { IsValueType: true, IsPrimitive: false };
    }
    
    public static bool IsGenericList(this object o)
    {
        Type oType = o.GetType();
        return (oType.IsGenericType && (oType.GetGenericTypeDefinition() == typeof(List<>)));
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
}