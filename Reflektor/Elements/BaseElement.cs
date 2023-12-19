using System;
using System.Reflection;
using UnityEngine.UIElements;

namespace Reflektor.Elements;

public abstract class BaseElement : VisualElement
{
    // Data
    protected readonly object Obj;
    protected readonly MemberInfo MemInfo;

    private readonly Label _label = new();

    protected BaseElement(object obj, MemberInfo memInfo)
    {
        Obj = obj;
        MemInfo = memInfo;

        style.fontSize = 14;
        _label.text = MemInfo.Name;
        _label.style.width = Length.Percent(40);
        _label.style.height = 28;
        _label.style.paddingTop = 2;
        _label.style.paddingBottom = 2;
        style.flexDirection = FlexDirection.Row;
        style.width = 900;
        style.minWidth = Length.Percent(100);
        style.maxWidth = Length.Percent(100);
        style.marginLeft = 0;
        style.marginRight = 0;
        Add(_label);
    }

    protected string GetValue<T>(Func<T, string> normalizer)
    {
        if (MemInfo.GetValue(Obj) is T newVal)
        {
            return normalizer.Invoke(newVal) ?? "";
        }

        return "";
    }
}