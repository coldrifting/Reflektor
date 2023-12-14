using System;
using System.Linq;
using System.Reflection;
using Reflektor.Extensions;
using UnityEngine.UIElements;

namespace Reflektor.Elements;

public class ElementEnum : BaseElement
{
    public ElementEnum(object obj, MemberInfo memInfo) : base(obj, memInfo)
    {
        // Make sure it's an Enum
        Type type = MemInfo.GetType();
        if (!type.IsEnum)
        {
            return;
        }
        
        // Setup GUI
        DropdownField dropdown = new();
        dropdown.label = MemInfo.Name;
        Add(dropdown);

        // Set values
        foreach (object? val in Enum.GetValues(type))
        {
            dropdown.choices.Add(val.ToString());
        }
        GetDropdownValue(dropdown);

        // Setup callbacks
        dropdown.SetEnabled(MemInfo.HasSetMethod() && !obj.IsStruct());
        if (dropdown.enabledSelf)
        {
            dropdown.RegisterValueChangedCallback(evt =>
            {
                if (Enum.TryParse(type, evt.newValue, out object result))
                {
                    MemInfo.SetValue(Obj, result);
                }

                GetDropdownValue(dropdown);
            });
        }
    }

    private void GetDropdownValue(DropdownField dropdown)
    {
        dropdown.SetValueWithoutNotify(MemInfo.GetValue(Obj)?.ToString() ?? dropdown.choices.First());
    }
}