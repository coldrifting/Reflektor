using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reflektor.Elements;

public class ElementEnum : BaseElement
{
    private readonly bool _hasSetMethod;
    private readonly DropdownField _dropdownField = new();
    
    public ElementEnum(object obj, MemberInfo memInfo) : base(obj, memInfo)
    {
        // Make sure it's an Enum
        Type? type = MemInfo.GetValue(obj)?.GetType();
        if (type is null || !type.IsEnum)
        {
            return;
        }
        
        // Setup GUI
        SetStyle();
        Add(_dropdownField);
        
        _hasSetMethod = MemInfo.HasSetMethod();

        // Set values
        foreach (object? val in Enum.GetValues(type))
        {
            _dropdownField.choices.Add(val.SetEnabledColor(_hasSetMethod));
        }
        SetFieldValue();

        // Setup callbacks
        if (obj.GetType().IsStruct())
        {
            return;
        }

        Reflektor.PropertyChangedEvent += evtObj =>
        {
            if (evtObj == Obj)
            {
                SetFieldValue();
            }
        };
        _dropdownField.RegisterValueChangedCallback(evt =>
        {
            string enumVal = evt.newValue.StripHtml();
            if (Enum.TryParse(type, enumVal, out object result))
            {
                MemInfo.SetValue(Obj, result);
            }

            Reflektor.FirePropertyChangedEvent(Obj);
        });
    }

    private void SetFieldValue()
    {
        string? val = MemInfo.GetValue(Obj)?.ToString();
        _dropdownField.SetValueWithoutNotify(val.SetEnabledColor(_hasSetMethod));
    }

    private void SetStyle()
    {
        Utils.DropdownFix(_dropdownField);
        _dropdownField.style.width = Length.Percent(25);
        _dropdownField.style.color = Color.white;
    }
}