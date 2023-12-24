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

        style.fontSize = 12;
        
        string colorPrefix = MemInfo switch
        {
            PropertyInfo => "55A38E",
            FieldInfo => "B05DE7",
            MethodInfo => "FF8000",
            _ => "FFFFFF"
        };
        string typePrefix = $"<color=#2CE7A7>{MemInfo.DeclaringType?.Name}</color>.";
        _label.text = $"{typePrefix}<color=#{colorPrefix}>{MemInfo.GetName()}</color>";
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

    public string GetName()
    {
        return MemInfo.Name;
    }

    protected string GetValue<T>(Func<T, string> normalizer)
    {
        if (MemInfo.GetValue(Obj) is T newVal)
        {
            return normalizer.Invoke(newVal) ?? "";
        }

        return "";
    }

    public void RefreshDisplay(DisplayFlags flags)
    {
        if (MemInfo.IsReadOnly() && (flags & DisplayFlags.ReadOnly) == 0)
        {
            style.display = DisplayStyle.None;
            return;
        }
        
        style.display = MemInfo switch
        {
            PropertyInfo => (flags & DisplayFlags.Properties) != 0 ? DisplayStyle.Flex : DisplayStyle.None,
            FieldInfo => (flags & DisplayFlags.Fields) != 0 ? DisplayStyle.Flex : DisplayStyle.None,
            MethodInfo => (flags & DisplayFlags.Methods) != 0 ? DisplayStyle.Flex : DisplayStyle.None,
            _ => DisplayStyle.Flex
        };
        
        SetFieldValue();
    }

    protected abstract void SetFieldValue();
}