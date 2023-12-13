using System.Globalization;
using System.Reflection;
using UnityEngine.UIElements;

namespace Reflektor;

public class LineItemFloat : LineItemBaseText<float>
{
    public LineItemFloat(object obj, PropertyInfo prop, VisualElement parent) : 
        base(obj, prop, parent, TryValidate, f => f.ToString(CultureInfo.InvariantCulture))
    {
    }

    private static bool TryValidate(string input, out float output)
    {
        if (float.TryParse(input, out float outputValue))
        {
            output = outputValue;
            return true;
        }

        output = default;
        return false;
    }
}