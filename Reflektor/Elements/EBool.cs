using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UIElements;

namespace Reflektor.Elements;

public class EBool : EBase
{
    private readonly Toggle _toggle;

    public EBool(object obj, MemberInfo? memberInfo = null, int? indexer = null, object? key = null) 
        : base(obj, memberInfo, indexer, key)
    {
        _toggle = new Toggle();
        _toggle.AddToClassList("toggle");
        _toggle.SetEnabled(IsReadOnly);
        _toggle.RegisterValueChangedCallback(_ => SetValue());

        Add(_toggle);
    }

    public override void GetValue()
    {
        bool value;
        if (MemInfo is not null)
        {
            value = (bool)(MemInfo.GetValue(Parent) ?? false);
        }
        else if (Indexer is not null && Parent is IList<bool> list)
        {
            value = list[Indexer.Value];
        }
        else if (Key is not null && Parent is IDictionary<object, bool> dict)
        {
            value = dict[Key];
        }
        else
        {
            value = false;
        }

        _toggle.SetValueWithoutNotify(value);
    }

    protected override void SetValue()
    {
        if (IsReadOnly)
        {
            return;
        }
        
        if (MemInfo is not null)
        {
            MemInfo.SetValue(Parent, _toggle.value);
        }
        else if (Indexer is not null && Parent is IList list)
        {
            list[Indexer.Value] = _toggle.value;
        }
        else if (Key is not null && Parent is IDictionary dict)
        {
            dict[Key] = _toggle.value;
        }

        Reflektor.FirePropertyChangedEvent(Parent);
    }
}