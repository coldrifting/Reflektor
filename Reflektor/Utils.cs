using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using SpaceWarp.API.Assets;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Reflektor;

[Flags]
public enum DisplayFlags
{
    None = 0,
    Properties = 1,
    Fields = 2,
    Methods = 4,
    All = None | Properties | Fields | Methods
}

public static class Utils
{
    private const BindingFlags ReflectionFlags =
        BindingFlags.Instance |
        BindingFlags.Static |
        BindingFlags.Public |
        BindingFlags.NonPublic;

    private static readonly Dictionary<string, int> CanvasStorage = new();
    
    // Make sure the inspector always shows up on top of pause menus, etc
    public static void ResetSorting()
    {
        var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var canvas in canvases)
        {
            if (CanvasStorage.TryGetValue(canvas.gameObject.GetPath(), out int output))
            {
                canvas.sortingOrder = output - 1000;
            }
            else
            {
                CanvasStorage[canvas.gameObject.GetPath()] = canvas.sortingOrder;
                canvas.sortingOrder -= 1000;
            }
        }
    }
    
    public static UIDocument GetNewWindow(string name)
    {
        // Load Unity asset
        WindowOptions winOptions = new WindowOptions
        {
            WindowId = name.Replace("Window", ""),
            Parent = Reflektor.Root.transform,
            IsHidingEnabled = true,
            DisableGameInputForTextFields = true,
            MoveOptions = new MoveOptions()
            {
                CheckScreenBounds = false,
                IsMovingEnabled = true
            }
        };
        VisualTreeAsset? browserWindowUxml = AssetManager.GetAsset<VisualTreeAsset>($"{Reflektor.ModGuid}/reflektor_ui/ui/{name}.uxml");

        UIDocument window = Window.Create(winOptions, browserWindowUxml);

        window.rootVisualElement.RegisterCallback((MouseDownEvent _) => window.rootVisualElement.BringToFront());

        return window;
    }
    
    ///////////////////////////
    //// Extension Methods ////
    ///////////////////////////
    
    // Arrays
    public static T? First<T>(this T[] arr)
    {
        return arr.Length > 0 
            ? arr[0] 
            : default;
    }

    public static T? Last<T>(this T[] arr)
    {
        return arr.Length > 0
            ? arr[^1]
            : default;
    }
    
    public static string First(this string[] arr)
    {
        return arr.Length > 0 
            ? arr[0] 
            : "";
    }

    public static string Last(this string[] arr)
    {
        return arr.Length > 0
            ? arr[^1]
            : "";
    }

    public static object Get(this IEnumerable enumerable, int index)
    {
        int count = 0;
        foreach (var v in enumerable)
        {
            if (count == index)
            {
                return v;
            }

            count++;
        }

        throw new Exception("Could not find element in enumeration");
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
    
    // UI Toolkit
    public static void Display(this VisualElement v, bool shouldDisplay)
    {
        if (shouldDisplay)
        {
            v.Show();
        }
        else
        {
            v.Hide();
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
    
    // Reflection
    public static object? GetValue(this object obj, string name)
    {
        if (obj.GetType().GetField(name, ReflectionFlags) is { } f)
        {
            return f.GetValue(obj);
        }

        if (obj.GetType().GetProperty(name, ReflectionFlags) is { } p)
        {
            return p.GetValue(obj);
        }

        return null;
    }

    public static void SetValue(this object obj, string name, object? value)
    {
        switch (obj.GetType().GetFieldOrProperty(name))
        {
            case PropertyInfo p:
                p.SetValue(obj, value);
                break;
            case FieldInfo f:
                f.SetValue(obj, value);
                break;
        }
    }

    private static MemberInfo? GetFieldOrProperty(this IReflect t, string name)
    {
        if (t.GetProperty(name, ReflectionFlags) is { } p)
        {
            return p;
        }

        if (t.GetField(name, ReflectionFlags) is { } f)
        {
            return f;
        }
        
        Reflektor.Log($"Can't find Field or Property {name} for type {t}");
        return null;
    }
    
    public static List<MemberInfo> GetMembersFiltered(this Type type)
    {
        List<MemberInfo> members = new();

        foreach (PropertyInfo p in type.GetProperties(ReflectionFlags))
        {
            if (p.GetIndexParameters().Length > 0)
            {
                continue;
            }
            
            members.Add(p);
        }
        
        foreach (FieldInfo f in type.GetFields(ReflectionFlags))
        {
            if (f.IsDefined(typeof(CompilerGeneratedAttribute), false))
            {
                continue;
            }
            
            members.Add(f);
        }
        
        foreach (MethodInfo m in type.GetMethods(ReflectionFlags))
        {
            if (m.IsSpecialName)
            {
                continue;
            }

            members.Add(m);
        }

        return members;
    }
    
    public static int GetSortIndex(this MemberInfo? m)
    {
        if (m is null)
        {
            return -1;
        }
        
        int baseVal = m switch
        {
            PropertyInfo => 500,
            FieldInfo => 1500,
            MethodInfo => 2500,
            _ => -1
        };

        int i = 0;
        Type? t = m.DeclaringType;
        while (t is not null)
        {
            i--;
            t = t.BaseType;
        }

        return baseVal + i;
    }

}