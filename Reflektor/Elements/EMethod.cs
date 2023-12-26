using System;
using System.Reflection;
using UnityEngine.UIElements;

namespace Reflektor.Elements;

public class EMethod : EBase
{
    private readonly Button _invokeBtn = new();
    private readonly Label _result = new();
    
    public EMethod(object obj, MethodBase? methodInfo = null, int? indexer = null, object? key = null) 
        : base(obj, methodInfo, indexer, key)
    {
        if (methodInfo is null || methodInfo.GetParameters().Length != 0)
        {
            ConsiderForLengthCalc = false;
            return;
        }

        _invokeBtn.text = "Invoke";
        _invokeBtn.clicked += () =>
        {
            object resultObj = methodInfo.Invoke(obj, Array.Empty<object>());
            _result.text = $"Result: <color=#FF7700>{resultObj}</color>";
        };
        _invokeBtn.RegisterCallback((MouseDownEvent evt) =>
        {
            if (evt.button == 1)
            {
                GetValue();
            }
        });

        Add(_invokeBtn);
        Add(_result);
    }

    public override void GetValue()
    {
        _result.text = $"Returns -> <color=#FF0077>{(MemInfo as MethodInfo)?.ReturnType.Name}</color>";
    }

    protected override void SetValue()
    {
    }
}