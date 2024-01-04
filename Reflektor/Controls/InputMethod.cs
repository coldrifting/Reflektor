using System;
using System.Reflection;
using UnityEngine.UIElements;

namespace Reflektor.Controls;

public class InputMethod : InputBase
{
    private readonly MethodInfo _methodInfo;
    
    private readonly Button _invokeBtn = new();
    private readonly Label _result = new();

    public InputMethod(Info info, MethodInfo m) : base(info)
    {
        _methodInfo = m;
        
        if (m.GetParameters().Length != 0)
        {
            return;
        }
        
        _invokeBtn.text = "Invoke";
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
        
        _result.text = DefaultText();

        _invokeBtn.clickable = null;
        _invokeBtn.clicked += () =>
        {
            try
            {
                object resultObj = m.Invoke(Key.Target, Array.Empty<object>());
                _result.text = $"Result: <color=#FF7700>{resultObj}</color>";
            }
            catch (Exception e)
            {
                _result.text = $"Result: <color=#FF3300>Exception occured: {e.Message}</color>";
            }
        };

        Reflektor.PropertyChangedEvent += (_, b) => _result.text = b ? DefaultText() : _result.text;
    }

    public override void PullChanges()
    {
        // Do nothing
    }

    private string DefaultText()
    {
        return $"Returns -> <color=#FF0077>{_methodInfo.ReturnType.Name}</color>";
    }
}