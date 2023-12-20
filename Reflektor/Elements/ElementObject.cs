using System.Linq;
using System.Reflection;
using UnityEngine.UIElements;

namespace Reflektor.Elements;

public class ElementObject : BaseElement
{
    private readonly Button _inspectBtn = new();
    private readonly Label _inspectLabel = new();

    private readonly object? _inspectObj;
    
    public ElementObject(object obj, MemberInfo memberInfo) :
        base(obj, memberInfo)
    {
        _inspectObj = MemInfo.GetValue(Obj);

        Label labelVal = new();
        if (_inspectObj == null)
        {
            _inspectBtn.text = "null";
        }
        else if (_inspectObj == Obj)
        {
            _inspectBtn.text = "this";
        }
        else
        {
            _inspectBtn.text = "Inspect";
        }

        SetFieldValue();
        SetStyle();
        Add(_inspectBtn);
        Add(labelVal);
        
        _inspectBtn.clicked += () =>
        {
            Reflektor.Inspect(_inspectObj);
        };        
        
        Reflektor.PropertyChangedEvent += evtObj =>
        {
            if (evtObj == Obj)
            {
                SetFieldValue();
            }
        };
    }

    private void SetFieldValue()
    {
        _inspectBtn.SetEnabled(_inspectObj is not null);
        _inspectLabel.text = _inspectObj?.ToString().Split("\n").First().Trim();
    }

    private void SetStyle()
    {
        _inspectLabel.style.paddingLeft = 20;
        _inspectLabel.style.maxWidth = Length.Percent(40);
        _inspectBtn.style.minWidth = Length.Percent(8);
        _inspectBtn.style.height = 24;
        _inspectBtn.style.paddingTop = 2;
        _inspectBtn.style.paddingBottom = 0;
        _inspectBtn.style.fontSize = 12;   
    }
}