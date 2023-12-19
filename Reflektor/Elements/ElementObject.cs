using System.Linq;
using System.Reflection;
using UnityEngine.UIElements;

namespace Reflektor.Elements;

public class ElementObject : BaseElement
{
    public ElementObject(object obj, MemberInfo memberInfo) :
        base(obj, memberInfo)
    {
        object? objVal = MemInfo.GetValue(obj);

        Label labelVal = new(objVal?.ToString().Split("\n").First().Trim());
        labelVal.style.paddingLeft = 20;
        labelVal.style.maxWidth = Length.Percent(40);
        Button button = new();
        button.style.minWidth = Length.Percent(8);
        button.style.height = 24;
        button.style.paddingTop = 2;
        button.style.paddingBottom = 0;
        button.style.fontSize = 12;
        button.text = objVal is not null ? "Inspect" : "null";
        if (objVal == obj)
        {
            button.text = "this";
        }
        
        Add(button);
        Add(labelVal);

        if (objVal is null)
        {
            button.SetEnabled(false);
            return;
        }
        
        button.clicked += () =>
        {
            Reflektor.Inspect(objVal);
        };
    }
}