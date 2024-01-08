using Reflektor.Controls;
using UnityEngine.UIElements;

namespace Reflektor.Windows;

public class Tab
{
    private readonly VisualElement _tabHandle;
    private readonly List<InputBase> _elements;

    public Tab(SelectKey key)
    {
        _tabHandle = new VisualElement();
        Label tabName = new(GetTabName(key.Target));
        Button tabCloseBtn = new(() => Inspector.CloseTab(key));
        tabCloseBtn.text = "\u2717"; // Close "X" Icon

        _tabHandle.Add(tabName);
        _tabHandle.Add(tabCloseBtn);
        _tabHandle.RegisterCallback((MouseDownEvent _) => Inspector.SwitchTab(key));
            
        _elements = InputAccess.GetInputs(key);
    }

    public void AddTab(VisualElement tabBar, VisualElement contentArea)
    {
        tabBar.Add(_tabHandle);
        foreach (InputBase input in _elements)
        {
            contentArea.Add(input);
        }
    }

    public void RemoveTab(VisualElement tabBar, VisualElement contentArea)
    {
        tabBar.Remove(_tabHandle);
        foreach (InputBase input in _elements)
        {
            contentArea.Remove(input);
        }
    }

    public void Refresh(SelectKey current, DisplayFlags flags = DisplayFlags.All, string filterText = "")
    {
        foreach (InputBase input in _elements)
        {
            input.Filter(current, flags, filterText);
        }
    }

    public void FocusTab()
    {
        _tabHandle.AddToClassList("focused");
    }

    public void UnfocusTab()
    {
        _tabHandle.RemoveFromClassList("focused");
    }

    private static string GetTabName(object obj)
    {
        string objType = obj.GetType().Name;
        return obj switch
        {
            UnityEngine.Object o => @$"{objType}\r\n<color=#FFBB00>{o.name}</color>",
            _ => $"{objType}\r\n"
        };
    }
}