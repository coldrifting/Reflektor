using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UIElements;

namespace Reflektor;

public class WindowTab
{
    private readonly VisualElement canvas = new();
    private readonly List<LineItemBase> lineItems = new();
    
    public WindowTab(object obj, VisualElement parent)
    {
        foreach (PropertyInfo propertyInfo in obj.GetType().GetProperties())
        {
            switch (propertyInfo.GetValue(obj))
            {
                case int iVal:
                    lineItems.Add(new LineItemInt(obj, propertyInfo, canvas));
                    break;
            }
        }

        parent.Add(canvas);
    }

    public void SetVisible(bool shouldShow)
    {
        canvas.style.display = new StyleEnum<DisplayStyle>(shouldShow ? DisplayStyle.Flex : DisplayStyle.None);
    }
}