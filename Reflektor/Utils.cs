using System;
using System.Collections.Generic;
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
    
    // Temporary fix for dropdowns
    public static void DropdownFix(DropdownField dropdownField)
    {
        VisualElement? dropdownInput = dropdownField.Query(className: "unity-base-popup-field__input").First();
        if (dropdownInput is not null)
        {
            dropdownInput.style.backgroundColor = Reflektor.ColorFromHex(0x192128);
            dropdownInput.style.borderBottomColor = Reflektor.ColorFromHex(0x333333);
            dropdownInput.style.borderTopColor = Reflektor.ColorFromHex(0x333333);
            dropdownInput.style.borderLeftColor = Reflektor.ColorFromHex(0x333333);
            dropdownInput.style.borderRightColor = Reflektor.ColorFromHex(0x333333);
            dropdownInput.style.height = 30;
            dropdownInput.style.paddingBottom = 0;
            dropdownInput.style.paddingTop = 0;
            dropdownInput.style.paddingLeft = 0;
            dropdownInput.style.paddingRight = 12;
            dropdownInput.style.marginLeft = 0;
            dropdownInput.style.marginRight = 0;
            dropdownInput.style.borderTopLeftRadius = 6;
            dropdownInput.style.borderTopRightRadius = 6;
            dropdownInput.style.borderBottomLeftRadius = 6;
            dropdownInput.style.borderBottomRightRadius = 6;
            
            dropdownField.RegisterCallback((MouseEnterEvent evt) =>
            {
                dropdownInput.style.backgroundColor = Reflektor.ColorFromHex(0x293138);
            });
            
            dropdownField.RegisterCallback((MouseLeaveEvent evt) =>
            {
                dropdownInput.style.backgroundColor = Reflektor.ColorFromHex(0x192128);
            });
        }
        
        VisualElement? dropdownInputText = dropdownField.Query(className: "unity-base-popup-field__text").First();
        if (dropdownInputText is not null)
        {
            dropdownInputText.style.fontSize = 14;
            dropdownInputText.style.marginTop = 0;
            dropdownInputText.style.marginBottom = 0;
            dropdownInputText.style.paddingTop = 3;
            dropdownInputText.style.paddingBottom = 3;
            dropdownInputText.style.paddingLeft = 10;
            dropdownInputText.style.paddingRight = 10;
            dropdownInputText.style.color = Color.white;
            dropdownInputText.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleLeft);
        }
        
        VisualElement? dropdownInputArrow = dropdownField.Query(className: "unity-base-popup-field__arrow").First();
        if (dropdownInputArrow is not null)
        {
            dropdownInputArrow.style.unityBackgroundImageTintColor = Reflektor.ColorFromHex(0xC0C7D5);
        }
    }

    public static void SetListViewEmptyText(ListView listView, string message)
    {
        VisualElement? emptyText = listView.Query(className: "unity-list-view__empty-label");
        if (emptyText is Label l)
        {
            l.text = message;
        }
    }
    
    // String validation
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
}