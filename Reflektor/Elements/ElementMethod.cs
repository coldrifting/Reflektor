using System;
using System.Reflection;
using UnityEngine.UIElements;

namespace Reflektor.Elements;

public class ElementMethod : BaseElement
{
    private readonly Label _result = new();
    private readonly Button _invokeBtn = new();
    
    public ElementMethod(object obj, MethodBase methodInfo) : base(obj, methodInfo)
    {
        if (methodInfo.GetParameters().Length == 0)
        {
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
                    SetFieldValue();
                }
            });
            
            SetStyle();
            Add(_invokeBtn);
            Add(_result);
        }
    }

    protected override void SetFieldValue()
    {
        _result.text = $"Returns -> <color=#FF0077>{(MemInfo as MethodInfo)?.ReturnType.Name}</color>";
    }

    private void SetStyle()
    {
        _invokeBtn.style.minWidth = Length.Percent(8);
        _invokeBtn.style.height = 24;
        _invokeBtn.style.paddingTop = 2;
        _invokeBtn.style.paddingBottom = 0;
        _invokeBtn.style.marginRight = 20;
        _invokeBtn.style.fontSize = 12;   
    }
}