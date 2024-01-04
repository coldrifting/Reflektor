using System.Reflection;

namespace Reflektor.Controls;

public class Info
{
    public readonly SelectKey Key;
    public readonly string Name;
    public readonly string FormatName;
    public readonly bool IsInCollection;
    
    public delegate object? GetMethod();
    public delegate void SetMethod(object? newValue);
    
    public readonly GetMethod Getter;
    public readonly SetMethod? Setter;
    
    public readonly MemberInfo? MemInfo;
    public readonly int SortOrder;

    public Info(SelectKey key, string formatName, MemberInfo? memberInfo = null)
    {
        Key = key;
        Name = formatName.StripColor().Split(".").Last();
        FormatName = formatName;
        IsInCollection = memberInfo is null;

        string name = IsInCollection ? "" : Name;
        Getter = () => Key.GetValue(name);
        if (Key.CanSetValue(name))
        {
            Setter = srcObject => Key.SetValue(name, srcObject);
        }

        MemInfo = memberInfo;
        SortOrder = memberInfo.GetSortIndex();
    }
}