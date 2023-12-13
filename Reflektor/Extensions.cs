using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Reflektor;

public static class Extensions
{
    public static bool TryParse(string value, out Quaternion output)
    {
        if (TryParse(value, out Vector4 outVec))
        {
            output = new Quaternion(outVec.x, outVec.y, outVec.z, outVec.w);
            return true;
        }

        output = Quaternion.identity;
        return false;
    }
    
    public static bool TryParse(string value, out Vector4 output)
    {
        string[] values = value.Split(new[]{',', ' '}, StringSplitOptions.RemoveEmptyEntries);
        
        if (values.Length >= 4)
        {
            if (float.TryParse(values[0], out float x))
            {
                if (float.TryParse(values[1], out float y))
                {
                    if (float.TryParse(values[2], out float z))
                    {
                        if (float.TryParse(values[3], out float w))
                        {
                            output = new Vector4(x, y, z, w);
                            return true;
                        }
                    }
                }
            }
        }

        if (TryParse(value, out Vector3 vec3))
        {
            output = vec3;
            return true;
        }

        output = Vector4.zero;
        return false;
    }
    
    public static bool TryParse(string value, out Vector3 output)
    {
        string[] values = value.Split(new[]{',', ' '}, StringSplitOptions.RemoveEmptyEntries);
        
        if (values.Length >= 3)
        {
            if (float.TryParse(values[0], out float x))
            {
                if (float.TryParse(values[1], out float y))
                {
                    if (float.TryParse(values[2], out float z))
                    {
                        output = new Vector3(x, y, z);
                        return true;
                    }
                }
            }
        }

        if (TryParse(value, out Vector2 vec2))
        {
            output = vec2;
            return true;
        }

        output = Vector3.zero;
        return false;
    }
    
    public static bool TryParse(string value, out Vector2 output)
    {
        string[] values = value.Split(new[]{',', ' '}, StringSplitOptions.RemoveEmptyEntries);
        if (values.Length >= 2)
        {
            if (float.TryParse(values[0], out float x))
            {
                if (float.TryParse(values[1], out float y))
                {
                    output = new Vector3(x, y, 0);
                    return true;
                }
            }
        }

        if (values.Length >= 1)
        {
            if (float.TryParse(values[0], out float x))
            {
                output = new Vector3(x, x, x);
                return true;
            }
        }

        output = Vector3.zero;
        return false;
    }
    
    public static string ToSimpleString(this Vector2 vec)
    {
        return $"{vec.x} {vec.y}";
    }
    
    public static string ToSimpleString(this Vector3 vec)
    {
        return $"{vec.x} {vec.y} {vec.z}";
    }
    
    public static string ToSimpleString(this Vector4 vec)
    {
        return $"{vec.x} {vec.y} {vec.z} {vec.w}";
    }
    
    public static string ToSimpleString(this Quaternion quaternion)
    {
        return $"{quaternion.x} {quaternion.y} {quaternion.z} {quaternion.w}";
    }
    
    public static string? Truncate(this string? value, int maxLength, string truncationSuffix = "…")
    {
        return value?.Trim().Length > maxLength
            ? value.Trim()[..maxLength] + truncationSuffix
            : value;
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

    public static void Add(this Dictionary<string, Scene> scenes, Scene? s)
    {
        if (s is null)
        {
            return;
        }
        
        scenes.Add(s.Value.name, s.Value);
    }

    public static IEnumerable<GameObject> GetChildren(this GameObject obj)
    {
        List<GameObject> objects = new();
        foreach (Transform t in obj.transform)
        {
            objects.Add(t.gameObject);
        }

        return objects;
    }

    public static bool IsVisible(this VisualElement element)
    {
        return element.style.display == new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
    }

    public static void SetVisible(this VisualElement element, bool isVisible)
    {
        element.style.display = isVisible 
            ? new StyleEnum<DisplayStyle>(DisplayStyle.Flex) 
            : new StyleEnum<DisplayStyle>(DisplayStyle.None);
    }

    public static void ToggleVisible(this VisualElement element)
    {
        element.SetVisible(!element.IsVisible());
    }

    public static void Log(object? msg)
    {
        if (msg is null)
        {
            return;
        }

        Debug.Log($"{Reflektor.ModName}: {msg}");
    }
}