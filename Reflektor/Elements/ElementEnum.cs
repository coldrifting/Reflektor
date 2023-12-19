using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reflektor.Elements;

public class ElementEnum : BaseElement
{

    private readonly bool _hasSetMethod;
    
    public ElementEnum(object obj, MemberInfo memInfo) : base(obj, memInfo)
    {
        // Make sure it's an Enum
        Type? type = MemInfo.GetValue(obj)?.GetType();
        if (type is null || !type.IsEnum)
        {
            return;
        }
        
        // Setup GUI
        DropdownField dropdown = new();
        dropdown.style.width = Length.Percent(25);
        dropdown.style.color = Color.white;
        Utils.DropdownFix(dropdown);
        Add(dropdown);
        
        _hasSetMethod = MemInfo.HasSetMethod();

        // Set values
        foreach (object? val in Enum.GetValues(type))
        {
            dropdown.choices.Add(val.SetEnabledColor(_hasSetMethod));
        }
        GetDropdownValue(dropdown);

        // Setup callbacks
        if (!obj.GetType().IsStruct())
        {
            dropdown.RegisterValueChangedCallback(evt =>
            {
                string enumVal = evt.newValue.StripHtml();
                if (Enum.TryParse(type, enumVal, out object result))
                {
                    MemInfo.SetValue(Obj, result);
                }

                GetDropdownValue(dropdown);
            });
        }
    }

    private void GetDropdownValue(DropdownField dropdown)
    {
        string? val = MemInfo.GetValue(Obj)?.ToString();
        dropdown.SetValueWithoutNotify(val.SetEnabledColor(_hasSetMethod));
    }
}