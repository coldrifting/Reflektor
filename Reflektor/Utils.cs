using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Reflektor.Elements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Reflektor;

public static class Utils
{
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
    
    public static void SetListViewEmptyText(ListView? listView, string message, string? hexColor = null)
    {
        VisualElement? emptyText = listView.Query(className: "unity-list-view__empty-label");
        if (emptyText is Label l)
        {
            l.text = hexColor is null 
                ? message 
                : $"<color={hexColor}>{message}</color>";
        }
    }
    
    public static EBase GetElement(object parent, MemberInfo? memberInfo = null, int? indexer = null, object? key = null)
    {
        object? obj;
        if (memberInfo is not null)
        {
            obj = memberInfo.GetValue(parent);
        }
        else if (indexer is not null && parent is IList list)
        {
            obj = list[indexer.Value];
        }
        else if (key is not null && parent is IDictionary dict)
        {
            obj = dict[key];
        }
        else
        {
            obj = null;
        }
        
        EBase x = obj switch
        {
            int => new EInt(parent, memberInfo, indexer, key),
            float => new EFloat(parent, memberInfo, indexer, key),
            double => new EDouble(parent, memberInfo, indexer, key),
            bool => new EBool(parent, memberInfo, indexer, key),
            string => new EString(parent, memberInfo, indexer, key),
            Vector2 => new EVec2(parent, memberInfo, indexer, key),
            Vector3 => new EVec3(parent, memberInfo, indexer, key),
            Vector4 => new EVec4(parent, memberInfo, indexer, key),
            Quaternion => new EQuaternion(parent, memberInfo, indexer, key),
            Color => new EColor(parent, memberInfo, indexer, key),
            Enum => new EEnum(parent, memberInfo, indexer, key),
            IList => new ECollection(parent, memberInfo, indexer, key),       // Array like collections
            IDictionary => new ECollection(parent, memberInfo, indexer, key), // Key/Value collections
            ICollection => new ECollection(parent, memberInfo, indexer, key), // All other collections
            MethodBase methodBase => new EMethod(parent, methodBase, indexer, key),
            _ => new EObject(parent, memberInfo, indexer, key)
        };
        return x;
    }

    public static bool IsList(object obj)
    {
        return obj.GetType().IsGenericType && obj.GetType().GetGenericTypeDefinition() == typeof(IReadOnlyList<>);
    }
    
    // String validation
    public static bool TryParse(string value, out Color output)
    {
        string[] values = value.Split(new[]{',', ' '}, StringSplitOptions.RemoveEmptyEntries);
        switch (values.Length)
        {
            case >= 4:
            {
                if (float.TryParse(values[0], out float r))
                {
                    if (float.TryParse(values[1], out float g))
                    {
                        if (float.TryParse(values[2], out float b))
                        {
                            if (float.TryParse(values[3], out float a))
                            {
                                output = new Color(r, g, b, a);
                                return true;
                            }
                        }
                    }
                }

                break;
            }
            case >= 3:
            {
                if (float.TryParse(values[0], out float r))
                {
                    if (float.TryParse(values[1], out float g))
                    {
                        if (float.TryParse(values[2], out float b))
                        {
                            output = new Color(r, g, b, 1.0f);
                            return true;
                        }
                    }
                }

                break;
            }
            case 1 when float.TryParse(values[0], out float c):
                output = new Color(c, c, c, 1.0f);
                return true;
        }

        output = new Color(0, 0, 0, 0);
        return false;
    }
    
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
    
    public static string ToSimpleString(Color color)
    {
        return $"{color.r} {color.g} {color.b} {color.a}";
    }
    
    public static string ToSimpleString(Vector2 vec)
    {
        return $"{vec.x} {vec.y}";
    }
    
    public static string ToSimpleString(Vector3 vec)
    {
        return $"{vec.x} {vec.y} {vec.z}";
    }
    
    public static string ToSimpleString(Vector4 vec)
    {
        return $"{vec.x} {vec.y} {vec.z} {vec.w}";
    }
    
    public static string ToSimpleString(Quaternion quaternion)
    {
        return $"{quaternion.x} {quaternion.y} {quaternion.z} {quaternion.w}";
    }

    public static GameObject? GetGameObject(object obj)
    {
        return obj switch
        {
            GameObject g => g,
            Component c => c.gameObject,
            _ => null
        };
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