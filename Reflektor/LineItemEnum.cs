using System;
using System.Reflection;
using UnityEngine.UIElements;

namespace Reflektor;

public class LineItemEnum : LineItemBase
{
    public LineItemEnum(object obj, PropertyInfo prop, VisualElement parent) : base(obj, prop)
    {
        DropdownField dropdown = new(_propertyInfo.Name);
        dropdown.label = _propertyInfo.Name;
        parent.Add(dropdown);

        Type enumType = _propertyInfo.PropertyType;
        
        if (enumType.IsEnum)
        {
            foreach (object? val in Enum.GetValues(enumType))
            {
                dropdown.choices.Add(val.ToString());
            }

            dropdown.value = _propertyInfo.GetValue(_object).ToString();
        }

        if (_propertyInfo.SetMethod is null)
        {
            dropdown.SetEnabled(false);
            return;
        }

        dropdown.RegisterValueChangedCallback(evt =>
        {
            if (Enum.TryParse(enumType, evt.newValue, out object result))
            {
                _propertyInfo.SetValue(_object, result);
            }
            dropdown.value = _propertyInfo.GetValue(_object).ToString();
        });
    }
}