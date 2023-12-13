using System.Globalization;
using System.Reflection;
using UnityEngine.UIElements;

namespace Reflektor;

public class LineItemInt : LineItemBaseText<int>
{
    public LineItemInt(object obj, PropertyInfo prop, VisualElement parent) : 
        base(obj, prop, parent, TryValidate, i => i.ToString(CultureInfo.InvariantCulture))
    {
    }

    private static bool TryValidate(string input, out int output)
    {
        if (int.TryParse(input, out int outputValue))
        {
            output = outputValue;
            return true;
        }

        output = default;
        return false;
    }
}