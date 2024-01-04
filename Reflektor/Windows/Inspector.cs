using System.Collections;
using System.Collections.Generic;
using UitkForKsp2.API;
using UniLinq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reflektor.Windows;

public static class Inspector
{
    // Data
    private static readonly Dictionary<SelectKey, Tab> Tabs = new ();
    private static DisplayFlags _flags = DisplayFlags.All;
    private static SelectKey? _current;
    private static SelectKey? Current
    {
        get => _current;
        set
        {
            _current = value;
            GetPathString();
        }
    }
    
    // GUI Elements
    private static readonly UIDocument Window;
    
    private static readonly VisualElement TabBar;
    private static readonly TextField PathInput;
    private static readonly TextField FilterInput;
    private static readonly Toggle AutoRefreshToggle;
    private static readonly Button RefreshButton;
    private static readonly ScrollView InspectorContent;
    
    static Inspector()
    {
        Window = Utils.GetNewWindow("InspectorWindow");
        
        // Find GUI elements
        TabBar = Window.rootVisualElement.Q<VisualElement>(name: "TabBar");
        PathInput = Window.rootVisualElement.Q<TextField>(name: "PathInput");
        FilterInput = Window.rootVisualElement.Q<TextField>(name: "FilterInput");
        
        Toggle propertyToggle = Window.rootVisualElement.Q<Toggle>(name: "PropertyToggle");
        Toggle fieldToggle = Window.rootVisualElement.Q<Toggle>(name: "FieldToggle");
        Toggle methodToggle = Window.rootVisualElement.Q<Toggle>(name: "MethodToggle");
        
        AutoRefreshToggle = Window.rootVisualElement.Q<Toggle>(name: "AutoRefreshToggle");
        RefreshButton = Window.rootVisualElement.Q<Button>(name: "RefreshButton");
        InspectorContent = Window.rootVisualElement.Q<ScrollView>(name: "InspectorContent");

        // Add callbacks
        PathInput.RegisterValueChangedCallback(evt =>
        {
            string[] fullPath = evt.newValue.Split("|", 2);
            GameObject? candidate = GameObject.Find(fullPath.First());
            if (candidate is not null)
            {
                string compTargetType = fullPath.Last().Trim();
                Component? comp = candidate.GetComponentByType(compTargetType);
                SwitchTab(comp != null ? new SelectKey(comp) : new SelectKey(candidate));
            }

            GetPathString();
        });

        FilterInput.RegisterValueChangedCallback(_ => Refresh());

        propertyToggle.RegisterValueChangedCallback(evt => ToggleFlag(DisplayFlags.Properties, evt.newValue));
        fieldToggle.RegisterValueChangedCallback(evt => ToggleFlag(DisplayFlags.Fields, evt.newValue));
        methodToggle.RegisterValueChangedCallback(evt => ToggleFlag(DisplayFlags.Methods, evt.newValue));

        AutoRefreshToggle.RegisterValueChangedCallback(evt =>
        {
            if (evt.newValue)
            {
                if (Reflektor.Instance != null)
                {
                    Reflektor.Instance.StartCoroutine(RefreshAutoCycle());
                }
            }
        });
            
        RefreshButton.clicked += () =>
        {
            if (Current != null)
            {
                Reflektor.FirePropertyChangedEvent(Current, true);
            }
        };
        
        // Remove placeholder elements
        TabBar.Clear();
        InspectorContent.Clear();
        
        Window.Hide();
    }

    private static void ToggleFlag(DisplayFlags flag, bool shouldSet)
    {
        _flags = shouldSet ? _flags | flag : _flags & ~flag;
        Refresh();
    }

    private static void AddTab(SelectKey key)
    {
        if (Tabs.ContainsKey(key))
        {
            return;
        }

        Tab t = new(key);
        t.AddTab(TabBar, InspectorContent);
        Tabs.Add(key, t);
    }

    public static void SwitchTab(SelectKey key)
    {
        if (Current is not null && Tabs.TryGetValue(Current, out Tab prevTab))
        {
            prevTab.UnfocusTab();
        }
        
        Current = key;
        
        Window.Show();
        
        AddTab(key);
        Refresh();

        if (Tabs.TryGetValue(Current, out Tab currentTab))
        {
            currentTab.FocusTab();
        }
        
        InspectorContent.scrollOffset = Vector2.zero;
    }

    public static void CloseTab(SelectKey key)
    {
        if (!Tabs.TryGetValue(key, out Tab closeTab))
        {
            return;
        }
        
        if (Equals(key, Current))
        {
            Current = null;
        }
        
        closeTab.RemoveTab(TabBar, InspectorContent);
        Tabs.Remove(key);
        
        // Switch tab
        if (Tabs.Count > 0)
        {
            if (Current == null)
            {
                SwitchTab(Tabs.Keys.First());
            }
        }
        else
        {
            Window.Hide();
        }
    }

    private static void Refresh()
    {
        if (Current is null)
        {
            return;
        }

        foreach (Tab t in Tabs.Values)
        {
            t.Refresh(Current, _flags, FilterInput.text);
        }
    }

    private static void GetPathString()
    {
        if (Current != null)
        {
            string path = Current.Target switch
            {
                null => "null",
                Component c => $"{c.gameObject.GetPath()}|{c.GetType().Name}",
                GameObject g => g.GetPath(),
                _ => Current.Target.ToString(),
            };
            PathInput.SetValueWithoutNotify(path);
        }
    }

    public static void ToggleDisplay()
    {
        if (Window.rootVisualElement.style.display == DisplayStyle.Flex)
        {
            Window.Hide();
        }
        else
        {
            if (Tabs.Count > 0)
            {
                Window.Show();
            }
        }
    }

    private static IEnumerator RefreshAutoCycle() {
        while(AutoRefreshToggle.value) {
            yield return new WaitForSeconds(1f);
            
            if (Current != null)
            {
                Reflektor.FirePropertyChangedEvent(Current);
            }
        }
    }
}