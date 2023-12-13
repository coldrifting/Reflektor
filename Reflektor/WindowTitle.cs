using System.Collections.Generic;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reflektor;

public class WindowTitle
{
    private readonly VisualElement root;
    private readonly GroupBox tabBar = new();

    private readonly Dictionary<object, Button> tabButtons = new();
    private readonly Dictionary<object, WindowTab> tabs = new();
    
    public WindowTitle()
    {
        root = Element.Root();
        root.style.width = 400;
        root.style.minWidth = 400;
        root.style.height = 400;
        root.style.minHeight = 400;

        tabBar.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
        root.Add(tabBar);

        GameObject g = GameObject.Find("/GameManager");
        RectTransform r = g.GetComponent<RectTransform>();

        SwitchTab(g);
        SwitchTab(r);

        Window.CreateFromElement(root, true, "TESTING", null, true);
    }

    public void SwitchTab(object obj)
    {
        if (!tabs.ContainsKey(obj))
        {
            tabs.Add(obj, new WindowTab(obj, root));
            Button b = new();
            b.text = obj.ToString();
            b.clicked += () =>
            {
                SwitchTab(obj);
            };
            tabBar.Add(b);
            tabButtons.Add(obj, b);
        }

        foreach (WindowTab t in tabs.Values)
        {
            t.SetVisible(false);
        }

        tabs[obj].SetVisible(true);
    }

    public void CloseTab(object obj)
    {
        tabs.Remove(obj);
        tabBar.Remove(tabButtons[obj]);
        tabButtons.Remove(obj);
    }
}