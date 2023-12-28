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
}