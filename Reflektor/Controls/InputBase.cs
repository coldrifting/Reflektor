using System;
using System.Reflection;
using UitkForKsp2.API;
using UnityEngine.UIElements;

namespace Reflektor.Controls;

public abstract class InputBase : VisualElement, IComparable
{
    public delegate object? GetSource(); // source -> value -> control
    public delegate void SetSource(object value); // control -> source

    protected readonly GetSource _getSource;
    protected readonly SetSource? _setSource;
    
    public string Name { get; }
    public string Prefix { get; }
    
    public object SourceObj { get; }
    public MemberInfo? Info { get; }

    private readonly Label _labelElement = new();
    public int LabelLength => _labelElement.text.StripColor().Length;

    protected InputBase(string label, MemberInfo? info, object sourceObj, GetSource getSource, SetSource? setSource)
    {
        Info = info;
        SourceObj = sourceObj;
        _getSource = getSource;
        _setSource = setSource;
        
        AddToClassList("input-control");
        Name = label.StripColor();
        string prefix = info is not null ? $"<color=#3AE2A9>{info.DeclaringType?.Name}</color>." : "";
        Prefix = prefix.StripColor();
        _labelElement.text = $"{prefix}{label}";
        Add(_labelElement);
        
        style.flexDirection = FlexDirection.Row;
        
        Reflektor.PropertyChangedEvent += (_, _) => SetField(_getSource.Invoke());
    }

    protected void Init()
    {
        SetField(_getSource.Invoke());
    }

    protected void Refresh()
    {
        Reflektor.FirePropertyChangedEvent(SourceObj);
    }

    public void SetLabelWidth(int charWidth)
    {
        _labelElement.style.width = 7 * charWidth;
    }
    
    public void Filter(object current, DisplayFlags flags, string filterString)
    {
        if (current != SourceObj)
        {
            this.Hide();
        }
        else if (!Name.Contains(filterString, StringComparison.InvariantCultureIgnoreCase))
        {
            this.Hide();
        }
        else
        {
            bool shouldShow = Info switch
            {
                FieldInfo => (flags & DisplayFlags.Fields) != 0,
                PropertyInfo => (flags & DisplayFlags.Properties) != 0,
                MethodInfo => (flags & DisplayFlags.Methods) != 0,
                _ => true
            };
            
            if (shouldShow)
            {
                this.Show();
            }
            else
            {
                this.Hide();
            }
        }
    }

    protected abstract void SetField(object? value);
    
    public int CompareTo(object obj)
    {
        if (obj is InputBase other)
        {
            if (Info is null || other.Info is null)
            {
                return -1;
            }
            
            int m1 = GetMemberSortOrder(Info);
            int val = m1.CompareTo(GetMemberSortOrder(other.Info));
            if (val == 0)
            {
                int val2 = GetDeclaredTypeDepth(Info).CompareTo(GetDeclaredTypeDepth(other.Info));
                return val2 == 0 
                    ? string.Compare(Name, other.Name, StringComparison.Ordinal) 
                    : val2;
            }

            return val;
        }

        return -1;
    }

    private static int GetMemberSortOrder(MemberInfo info) => info switch
    {
        PropertyInfo => 0,
        FieldInfo => 1,
        MethodInfo => 2,
        _ => -1
    };

    private static int GetDeclaredTypeDepth(MemberInfo info)
    {
        int i = 0;
        Type? t = info.DeclaringType;
        while (t is not null)
        {
            i--;
            t = t.BaseType;
        }

        return i;
    }
}