using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UIElements;

namespace Reflektor.Elements;

public sealed class EEnum : EBase
{
    private readonly Type? _enumType;
    private readonly DropdownField _dropdownField = new();
    
    public EEnum(object obj, MemberInfo? memberInfo = null, int? indexer = null, object? key = null) 
        : base(obj, memberInfo, indexer, key)
    {
        if (MemInfo is not null)
        {
            _enumType = MemInfo.GetValue(Parent)?.GetType();
        }
        else if (Indexer is not null && Parent is IList list)
        {
            _enumType = list[Indexer.Value].GetType();
        }
        else if (Key is not null && Parent is IDictionary dict)
        {
            _enumType = dict[Key].GetType();
        }

        if (_enumType is null)
        {
            return;
        }
        
        foreach (object? val in Enum.GetValues(_enumType))
        {
            _dropdownField.choices.Add(val.SetEnabledColor(IsReadOnly));
        }
        GetValue();
        
        _dropdownField.RegisterValueChangedCallback(_ => SetValue());

        Add(_dropdownField);
    }

    public override void GetValue()
    {
        object? value;
        if (MemInfo is not null)
        {
            value = MemInfo.GetValue(Parent);
        }
        else if (Indexer is not null && Parent is IList list)
        {
            value = list[Indexer.Value];
        }
        else if (Key is not null && Parent is IDictionary dict)
        {
            value = dict[Key];
        }
        else
        {
            value = "[ None ]";
        }
        
        _dropdownField.SetValueWithoutNotify(value.SetEnabledColor(IsReadOnly));
    }

    protected override void SetValue()
    {
        string enumVal = _dropdownField.value.StripHtml();
        if (Enum.TryParse(_enumType, enumVal, out object result))
        {
            if (MemInfo is not null)
            {
                MemInfo.SetValue(Parent, result);
            }
            else if (Indexer is not null && Parent is IList list)
            {
                list[Indexer.Value] = (Enum)result;
            }
            else if (Key is not null && Parent is IDictionary dict)
            {
                dict[Key] = (Enum)result;
            }
        }

        Reflektor.FirePropertyChangedEvent(Parent);
    }
}