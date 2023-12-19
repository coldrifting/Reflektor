using System.Reflection;
using UnityEngine.UIElements;

namespace Reflektor.Elements;

public class ElementBool : BaseElement
{
    public ElementBool(object obj, MemberInfo memInfo) : base(obj, memInfo)
    {
        Toggle toggle = new();
        toggle.AddToClassList("toggle");
        toggle.style.height = Length.Percent(80);
        toggle.style.width = Length.Percent(80);
        Add(toggle);

        // Initial Value
        if (MemInfo.GetValue(Obj) is bool boolVal)
        {
            toggle.SetValueWithoutNotify(boolVal);
        }
        
        // Read only and change callbacks
        bool enabled = MemInfo.HasSetMethod() && !obj.GetType().IsStruct();
        toggle.SetEnabled(enabled);
        if (enabled)
        {
            toggle.RegisterValueChangedCallback(evt =>
            {
                MemInfo.SetValue(Obj, evt.newValue);
            });
        }
    }
}