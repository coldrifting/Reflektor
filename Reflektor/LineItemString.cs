using System.Globalization;
using System.Reflection;
using UnityEngine.UIElements;

namespace Reflektor;

public class LineItemString : LineItemBaseText<string>
{
    public LineItemString(object obj, PropertyInfo prop, VisualElement parent) :
        base(obj, prop, parent, TryValidate, s => s)
    {
    }

    private static bool TryValidate(string input, out string output)
    {
        output = input;
        return true;
    }
}