using System.Reflection;
using UnityEngine.UIElements;

namespace Reflektor.Elements;

public class ElementBool : BaseElement
{
    private readonly Toggle _toggle = new ();
    
    public ElementBool(object obj, MemberInfo memInfo) : base(obj, memInfo)
    {
        _toggle.AddToClassList("toggle");
        SetStyle();
        Add(_toggle);

        // Initial Value
        SetFieldValue();
        
        // Read only and change callbacks
        bool enabled = MemInfo.HasSetMethod() && !Obj.GetType().IsStruct();
        _toggle.SetEnabled(enabled);

        Reflektor.PropertyChangedEvent += evtObj =>
        {
            if (evtObj == Obj)
            {
                SetFieldValue();
            }
        };
        _toggle.RegisterValueChangedCallback(evt =>
        {
            MemInfo.SetValue(Obj, evt.newValue);
            Reflektor.FirePropertyChangedEvent(Obj);
        });
    }

    private void SetFieldValue()
    {
        if (MemInfo.GetValue(Obj) is bool boolVal)
        {
            _toggle.SetValueWithoutNotify(boolVal);
        }
    }

    private void SetStyle()
    {
        _toggle.style.height = Length.Percent(80);
        _toggle.style.width = Length.Percent(80);
    }
}