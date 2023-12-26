using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UIElements;

namespace Reflektor.Elements;

public abstract class EBase : VisualElement
{
    protected readonly Label _label = new();
    
    protected object Parent { get; }
    protected MemberInfo? MemInfo { get; }
    protected int? Indexer { get; }
    protected object? Key { get; }

    public bool IsReadOnly { get; }
    public bool ConsiderForLengthCalc { get; protected set; } = true;

    protected EBase(object parent, MemberInfo? memberInfo = null, int? indexer = null, object? key = null)
    {
        Parent = parent;
        MemInfo = memberInfo;
        Indexer = indexer;
        Key = key;

        if (memberInfo is null && indexer is null && key is null)
        {
            throw new Exception();
        }

        IsReadOnly = MemInfo is not null && MemInfo.IsReadOnly();

        style.flexDirection = FlexDirection.Row;
        
        _label.text = GetName();
        Add(_label);
        AddToClassList("BaseElement");
        
        Reflektor.PropertyChangedEvent += evtObj =>
        {
            if (evtObj == Parent)
            {
                GetValue();
            }
        };
    }

    public string GetName()
    {
        return Parent switch
        {
            IList => Indexer.ToString(),
            IDictionary => Key?.ToString(),
            _ => GetMemInfoName()
        } ?? "";
    }

    public void SetLabelLength(int length)
    {
        _label.style.minWidth = length;
        _label.style.maxWidth = length;
        _label.style.width = length;
    }

    private string GetMemInfoName()
    {
        string colorPrefix = MemInfo switch
        {
            PropertyInfo => "55A38E",
            FieldInfo => "B05DE7",
            MethodInfo => "FF8000",
            _ => "FFFFFF"
        };
        string typePrefix = $"<color=#2CE7A7>{MemInfo?.DeclaringType?.Name}</color>.";
        return $"{typePrefix}<color=#{colorPrefix}>{MemInfo?.GetName()}</color>";
    }

    public abstract void GetValue();
    protected abstract void SetValue();

    public void Refresh(DisplayFlags flags)
    {
        if (flags == DisplayFlags.None)
        {
            style.display = DisplayStyle.None;
            return;
        }
        
        if (IsReadOnly && (flags & DisplayFlags.ReadOnly) == 0)
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

        GetValue();
    }
}