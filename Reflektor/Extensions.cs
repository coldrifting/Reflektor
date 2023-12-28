using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Reflektor;

public static class Extensions
{
    private const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | 
                                       BindingFlags.Static | BindingFlags.Instance | 
                                       BindingFlags.DeclaredOnly;
    
    // Types and Reflection
    public static bool IsReadOnly(this MemberInfo memberInfo)
    {
        switch (memberInfo)
        {
            case PropertyInfo propertyInfo:
                if (typeof(IList).IsAssignableFrom(propertyInfo.PropertyType) ||
                    typeof(IDictionary).IsAssignableFrom(propertyInfo.PropertyType))
                {
                    return false;
                }

                if (typeof(ICollection).IsAssignableFrom(propertyInfo.PropertyType))
                {
                    return true;
                }
                
                return propertyInfo.GetSetMethod(true) is null;
            case FieldInfo fieldInfo:
                return fieldInfo is { IsLiteral: true, IsInitOnly: false };
            default:
                return true;
        }
    }

    public static object? GetValue(this MemberInfo memberInfo, object obj) => memberInfo switch
    {
        PropertyInfo propertyInfo => propertyInfo.GetValue(obj),
        FieldInfo fieldInfo => fieldInfo.GetValue(obj),
        MethodInfo methodInfo => methodInfo,
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
        return t == null 
            ? Enumerable.Empty<FieldInfo>() 
            : t.GetFields(flags).Concat(GetAllFields(t.BaseType));
    }
    
    public static IEnumerable<PropertyInfo> GetAllProperties(this Type? t)
    {
        return t == null 
            ? Enumerable.Empty<PropertyInfo>() 
            : t.GetProperties(flags).Concat(GetAllProperties(t.BaseType));
    }
    
    public static string GetNameHighlighted(this MemberInfo memInfo)
    {
        string color = memInfo switch
        {
            PropertyInfo => "55A38E",
            FieldInfo => "B05DE7",
            MethodInfo => "FF8000",
            _ => "FFFFFF"
        };
        string name = memInfo switch
        {
            PropertyInfo propertyInfo => propertyInfo.Name,
            FieldInfo fieldInfo => fieldInfo.Name,
            MethodInfo methodInfo => GetMethodNameHighlighted(methodInfo),
            _ => "[n]"
        };
        
        return $"<color=#{color}>{name}</color>";
    }

    private static string GetMethodNameHighlighted(MethodBase method)
    {
        if (method.ReflectedType == null)
        {
            return "";
        }
        
        string name = method.Name;

        List<string> parameters = new();
        foreach (ParameterInfo parameterInfo in method.GetParameters())
        {
            string parameter = parameterInfo.ParameterType.Name;
            if (parameter.EndsWith("&"))
            {
                parameter = "<color=#158B2E>ref</color> " + parameter[..^1];
            }

            parameters.Add($"<color=#4389BB>{parameter}</color>");
        }

        return name + "(" + string.Join(",", parameters) + ")";
    }

    public static string GetTabName(this object obj)
    {
        string objType = obj.GetType().Name;
        return obj switch
        {
            Object o => @$"{objType}\r\n<color=#FFBB00>{o.name}</color>",
            _ => $"{objType}\r\n"
        };
    }
    
    // GUI
    public static bool IsVisible(this UIDocument w)
    {
        return w.rootVisualElement.IsVisible();
    }
    
    public static bool IsVisible(this VisualElement v)
    {
        return v.style.display == DisplayStyle.Flex;
    }
    
    public static void AddRange(this VisualElement v, IEnumerable<VisualElement> elements)
    {
        foreach (VisualElement? e in elements)
        {
            v.Add(e);
        }
    }

    public static void RemoveRange(this VisualElement v, IEnumerable<VisualElement> elements)
    {
        foreach (VisualElement e in elements)
        {
            v.Remove(e);
        }
    }
    
    public static void SetEmptyText(this ListView? listView, string message, string? hexColor = null)
    {
        VisualElement? emptyText = listView.Query(className: "unity-list-view__empty-label");
        if (emptyText is Label l)
        {
            l.text = hexColor is null 
                ? message 
                : $"<color={hexColor}>{message}</color>";
        }
    }
    
    // Strings
    public static string? Truncate(this string? value, int maxLength, string truncationSuffix = "…")
    {
        return value?.Trim().Length > maxLength
            ? value.Trim()[..maxLength] + truncationSuffix
            : value;
    }
    
    public static string StripColor(this string input)
    {   
        return Regex.Replace(input, @"(<color=#([0-9A-Fa-f]{3}){1,2}>|<\/color>)", string.Empty);
    }

    // Game Objects and Components
    public static GameObject? GetGameObject(this object obj)
    {
        return obj switch
        {
            GameObject g => g,
            Component c => c.gameObject,
            _ => null
        };
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
    
    public static Component? GetComponentByType(this GameObject candidate, string compTargetType)
    {
        foreach (Component c in candidate.GetComponents<Component>())
        {
            string curType = c.GetType().ToString().Split(".").Last().Trim();
            if (!string.Equals(compTargetType, curType, StringComparison.CurrentCultureIgnoreCase))
            {
                continue;
            }

            return c;
        }

        return null;
    }
}