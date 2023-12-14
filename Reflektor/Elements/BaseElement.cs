using System;
using System.Reflection;
using Reflektor.Extensions;
using UnityEngine;
using UnityEngine.UIElements;
using Random = System.Random;

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

        Random r = new();
        style.fontSize = 14;
        //style.backgroundColor = new Color(r.Next(100) / 100.0f, r.Next(100) / 100.0f, r.Next(100) / 100.0f);
        _label.text = MemInfo.Name;
        _label.style.width = Length.Percent(40);
        style.flexDirection = FlexDirection.Row;
        //style.paddingTop = 4;
        //style.paddingBottom = 4;
        //style.marginTop = 0;
        //style.marginBottom = 0;
        style.height = 28;
        style.minHeight = 28;
        style.maxHeight = 28;
        style.width = 900; //Length.Percent(70);
        style.minWidth = Length.Percent(100);
        style.maxWidth = Length.Percent(100);
        style.marginLeft = 0;
        style.marginRight = 0;
        Add(_label);
        //style.width = Length.Percent(100);
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