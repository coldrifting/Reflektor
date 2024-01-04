using System;
using System.Reflection;
using UitkForKsp2.API;
using UnityEngine.UIElements;

namespace Reflektor.Controls;

public abstract class InputBase : VisualElement, IComparable
{
    protected SelectKey Key { get; }
    protected string Name { get; }

    protected readonly Info.GetMethod Getter;
    protected readonly Info.SetMethod? Setter;

    protected readonly bool IsInsideCollection;

    private readonly MemberInfo? _info;
    private readonly int _sortOrder;

    private readonly Label _labelElement = new();

    protected InputBase(Info info)
    {
        _labelElement.text = info.FormatName;
        Add(_labelElement);
        AddToClassList("input-control");
        
        Key = info.Key;
        Name = info.Name;
        Getter = info.Getter;
        Setter = info.Setter;
        IsInsideCollection = info.IsInCollection;
        _info = info.MemInfo;
        _sortOrder = info.SortOrder;

        Reflektor.PropertyChangedEvent += (refreshKey, _) =>
        {
            if (refreshKey.Equals(Key))
            {
                PullChanges();
            }
        };
    }
    
    public abstract void PullChanges();
    
    protected void Refresh()
    {
        Reflektor.FirePropertyChangedEvent(Key);
    }
    
    public void Filter(SelectKey current, DisplayFlags flags, string filterString)
    {
        if (!Equals(current, Key))
        {
            this.Hide();
        }
        else if (!Name.Contains(filterString, StringComparison.InvariantCultureIgnoreCase))
        {
            this.Hide();
        }
        else
        {
            this.Display(_info switch
            {
                PropertyInfo => (flags & DisplayFlags.Properties) != 0,
                FieldInfo => (flags & DisplayFlags.Fields) != 0,
                MethodInfo => (flags & DisplayFlags.Methods) != 0,
                _ => true
            });
        }
    }

    public void SetLabelWidth(int charWidth)
    {
        _labelElement.style.width = 7 * charWidth;
    }
    
    public int CompareTo(object obj)
    {
        if (obj is InputBase other)
        {
            int val = _sortOrder.CompareTo(other._sortOrder);
            return val == 0 
                ? string.Compare(Name, other.Name, StringComparison.Ordinal) 
                : val;
        }

        return -1;
    }
}