using System;
using System.Reflection;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Reflektor.Controls;

public class InputObject : InputBase
{
    private readonly Button _inspectBtn = new();
    private readonly Label _inspectLabel = new();

    private object? _inspectObj;
    private readonly object? _sourceObj;
    
    public InputObject(string label, MemberInfo? info, object sourceObj, object? inspectObj, GetSource getSource) 
        : base(label, info, sourceObj, getSource, null)
    {
        _inspectObj = inspectObj;
        _sourceObj = sourceObj;

        _inspectBtn.clicked += () => Reflektor.Inspect(_inspectObj);

        Add(_inspectBtn);
        Add(_inspectLabel);
        
        Init();
    }

    protected override void SetField(object? value)
    {
        _inspectObj = value;
        _inspectBtn.text = _inspectObj == _sourceObj ?  "this" : _inspectObj == null ? "null" : GetName(_inspectObj);
    }

    private static string GetName(object obj)
    {
        if (obj is Object o)
        {
            return o.name;
        }

        string objectString = obj.ToString();
        Type objectType = obj.GetType();
        
        return objectString.Equals(objectType.FullName) || objectString.Contains("\n")
            ? objectType.Name 
            : objectString;
    }
}