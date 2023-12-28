using System;
using System.Reflection;
using UnityEngine.UIElements;

namespace Reflektor.Controls;

public class InputMethod : InputBase
{
    public bool HasParameters => _methodInfo.GetParameters().Length != 0;

    private readonly MethodInfo _methodInfo;
    private readonly Button _invokeBtn = new();
    private readonly Label _result = new();

    public InputMethod(MethodInfo methodInfo, object sourceObj, string label, GetSource getSource) 
        : base(label, methodInfo, sourceObj, getSource, null)
    {
        _methodInfo = methodInfo;
        
        if (HasParameters)
        {
            return;
        }

        _invokeBtn.text = "Invoke";
        _invokeBtn.clicked += () =>
        {
            try
            {
                object resultObj = methodInfo.Invoke(sourceObj, Array.Empty<object>());
                _result.text = $"Result: <color=#FF7700>{resultObj}</color>";
            }
            catch (Exception e)
            {
                _result.text = $"Result: <color=#FF3300>Exception occured: {e.Message}</color>";
            }
        };
        _invokeBtn.RegisterCallback((MouseDownEvent evt) =>
        {
            if (evt.button == 1)
            {
                _result.text = DefaultText();
            }
        });

        _result.AddToClassList("method-output");
        Add(_invokeBtn);
        Add(_result);

        Reflektor.PropertyChangedEvent += (_, b) => _result.text = b ? DefaultText() : _result.text;
        
        _result.text = DefaultText();
    }

    protected override void SetField(object? value)
    {
    }

    private string DefaultText()
    {
        return $"Returns -> <color=#FF0077>{_methodInfo.ReturnType.Name}</color>";
    }
}