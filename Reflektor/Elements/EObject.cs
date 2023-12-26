using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UIElements;

namespace Reflektor.Elements;

public class EObject : EBase
{
    private readonly Button _inspectBtn = new();
    private readonly Label _inspectLabel = new();

    private readonly object? _inspectObj;
    
    public EObject(object obj, MemberInfo? memberInfo = null, int? indexer = null, object? key = null) 
        : base(obj, memberInfo, indexer, key)
    {
        if (MemInfo is not null)
        {
            _inspectObj = MemInfo.GetValue(Parent);
        }
        else if (Indexer is not null && Parent is IList list)
        {
            _inspectObj = list[Indexer.Value];
        }
        else if (Key is not null && Parent is IDictionary dict)
        {
            _inspectObj = dict[Key];
        }
        
        if (_inspectObj == null)
        {
            _inspectBtn.text = "null";
        }
        else if (_inspectObj == Parent)
        {
            _inspectBtn.text = "this";
        }
        else
        {
            _inspectBtn.text = "Inspect";
        }

        _inspectBtn.clicked += () => Reflektor.Inspect(_inspectObj);

        Add(_inspectBtn);
        Add(_inspectLabel);
    }

    public override void GetValue()
    {
        if (_inspectObj is UnityEngine.Object unityObject)
        {
            _inspectLabel.text = unityObject.name;
        }
        else
        {
            _inspectLabel.text = _inspectObj?.GetType().ToString() ?? "";
        }
    }

    protected override void SetValue()
    {
    }
}