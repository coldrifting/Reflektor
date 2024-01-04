using System;
using UnityEngine.UIElements;

namespace Reflektor.Controls;

public class InputObject : InputBase
{
    private readonly Button _inspectBtn = new();
    private readonly Label _inspectLabel = new();
    
    public InputObject(Info info) : base (info)
    {
        Add(_inspectBtn);
        Add(_inspectLabel);
    }

    public override void PullChanges()
    {
        object? inspect = Getter.Invoke();

        if (inspect == null)
        {
            _inspectBtn.text = "null";
            return;
        }

        _inspectBtn.text = !IsInsideCollection && inspect == Key.Target ? "this" : GetName(inspect);

        _inspectBtn.clickable = null;
        _inspectBtn.clicked += () =>
        {
            SelectKey v = Key.GetSubKey(Name);
            Reflektor.Inspect(v);
        };
    }

    private static string GetName(object obj)
    {
        if (obj is UnityEngine.Object o)
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