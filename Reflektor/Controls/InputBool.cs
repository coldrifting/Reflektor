using System.Reflection;
using UnityEngine.UIElements;

namespace Reflektor.Controls;

public class InputBool : InputBase
{
    private readonly Toggle _toggle = new();
    
    public InputBool(string label, MemberInfo? info, object sourceObj, GetSource getSource, SetSource? setSource) 
        : base(label, info, sourceObj, getSource, setSource)
    {
        _toggle.AddToClassList("toggle");
        Add(_toggle);

        if (setSource is null)
        {
            _toggle.SetEnabled(false);
        }
        
        _toggle.RegisterValueChangedCallback(evt =>
        {
            _setSource?.Invoke(evt.newValue);
            Refresh();
        });

        Init();
    }

    protected override void SetField(object? value)
    {
        if (value is bool newBool)
        {
            _toggle.SetValueWithoutNotify(newBool);
        }
    }
}