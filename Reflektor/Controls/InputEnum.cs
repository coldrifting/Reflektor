using System;
using System.Reflection;
using UnityEngine.UIElements;

namespace Reflektor.Controls;

public class InputEnum : InputBase
{
    private readonly EnumField _dropdownField;

    public InputEnum(string label, Enum enumObj, object sourceObj, MemberInfo? info, GetSource getSource, SetSource? setSource) 
        : base(label, info, sourceObj, getSource, setSource)
    {
        _dropdownField = new EnumField(enumObj);
        _dropdownField.SetEnabled(setSource is not null);
        _dropdownField.RegisterValueChangedCallback(_ => UpdateSource());
    
        Add(_dropdownField);
        Init();
    }

    protected override void SetField(object? value)
    {
        if (value is Enum valEnum)
        {
            _dropdownField.SetValueWithoutNotify(valEnum);
        }
    }
    
    private void UpdateSource()
    {
        _setSource?.Invoke(_dropdownField.value);
        Refresh();
    }
}