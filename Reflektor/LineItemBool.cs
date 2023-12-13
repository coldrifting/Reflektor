using System.Reflection;
using UnityEngine.UIElements;

namespace Reflektor;

public class LineItemBool : LineItemBase
{
    public LineItemBool(object obj, PropertyInfo prop, VisualElement parent) : base(obj, prop)
    {
        Toggle toggle = new(_propertyInfo.Name);
        parent.Add(toggle);
        
        if (_propertyInfo.GetValue(_object) is bool newVal)
        {
            toggle.value = newVal;
        }

        if (_propertyInfo.SetMethod is null)
        {
            toggle.SetEnabled(false);
            return;
        }

        toggle.RegisterValueChangedCallback(evt =>
        {
            _propertyInfo.SetValue(_object, evt.newValue);
        });
    }
}