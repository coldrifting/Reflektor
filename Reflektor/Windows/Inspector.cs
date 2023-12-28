using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UitkForKsp2.API;
using UniLinq;
using UnityEngine;
using UnityEngine.UIElements;
using System.Runtime.CompilerServices;
using Reflektor.Controls;
using Color = UnityEngine.Color;

namespace Reflektor.Windows;

public class Inspector : BaseWindow
{
    private record struct Tab
    (
        List<InputBase> data, 
        VisualElement tab
    );
    
    // GUI Elements
    private readonly VisualElement _tabBar;
    private readonly TextField _pathInput;

    private readonly TextField _filterInput;

    private readonly Toggle _autoRefreshToggle;
    private readonly Button _refreshButton;
    
    private readonly ScrollView _inspectorContent;

    // Data
    private readonly Dictionary<object, Tab> _tabs = new ();
    
    private DisplayFlags _flags = DisplayFlags.All;
    
    private object? _current;
    private object? Current
    {
        get => _current;
        set
        {
            _current = value;
            GetPathString();
        }
    }


    public Inspector(GameObject parent) : base(parent, "InspectorWindow")
    {
        // Find GUI elements
        _tabBar = Window.rootVisualElement.Q<VisualElement>(name: "TabBar");
        _pathInput = Window.rootVisualElement.Q<TextField>(name: "PathInput");
        
        _filterInput = Window.rootVisualElement.Q<TextField>(name: "FilterInput");
        
        Toggle propertyToggle = Window.rootVisualElement.Q<Toggle>(name: "PropertyToggle");
        Toggle fieldToggle = Window.rootVisualElement.Q<Toggle>(name: "FieldToggle");
        Toggle methodToggle = Window.rootVisualElement.Q<Toggle>(name: "MethodToggle");
        
        _autoRefreshToggle = Window.rootVisualElement.Q<Toggle>(name: "AutoRefreshToggle");
        _refreshButton = Window.rootVisualElement.Q<Button>(name: "RefreshButton");

        _inspectorContent = Window.rootVisualElement.Q<ScrollView>(name: "InspectorContent");

        // Add callbacks
        _pathInput.RegisterValueChangedCallback(evt =>
        {
            string[] fullPath = evt.newValue.Split("|", 2);
            GameObject? candidate = GameObject.Find(fullPath.First());
            if (candidate is not null)
            {
                string compTargetType = fullPath.Last().Trim();
                Component? comp = candidate.GetComponentByType(compTargetType);
                SwitchTab(comp != null ? comp : candidate);
            }

            GetPathString();
        });

        _filterInput.RegisterValueChangedCallback(_ => Refresh());

        propertyToggle.RegisterValueChangedCallback(evt => ToggleFlag(DisplayFlags.Properties, evt.newValue));
        fieldToggle.RegisterValueChangedCallback(evt => ToggleFlag(DisplayFlags.Fields, evt.newValue));
        methodToggle.RegisterValueChangedCallback(evt => ToggleFlag(DisplayFlags.Methods, evt.newValue));

        _autoRefreshToggle.RegisterValueChangedCallback(evt =>
        {
            if (evt.newValue)
            {
                if (Reflektor.Instance != null)
                {
                    Reflektor.Instance.StartCoroutine(RefreshAutoCycle());
                }
            }
        });
            
        _refreshButton.clicked += () =>
        {
            if (Current != null)
            {
                Reflektor.FirePropertyChangedEvent(Current, true);
            }
        };
        
        // Remove placeholder elements
        _tabBar.Clear();
        _inspectorContent.Clear();
        
        Window.Hide();
    }

    private IEnumerator RefreshAutoCycle() {
        while(_autoRefreshToggle.value) {
            yield return new WaitForSeconds(1f);
            
            if (Current != null)
            {
                Reflektor.FirePropertyChangedEvent(Current);
            }
        }
    }

    private void ToggleFlag(DisplayFlags flag, bool shouldSet)
    {
        _flags = shouldSet ? _flags | flag : _flags & ~flag;
        Refresh();
    }

    private void AddTab(object obj)
    {
        if (_tabs.ContainsKey(obj))
        {
            return;
        }
        
        // Create data
        var elements = GetElements(obj);
        
        Label b = new(obj.GetTabName());
        Button close = new(() => CloseTab(obj));
        close.text = "\u2717"; // Close Icon

        VisualElement tab = new();
        tab.RegisterCallback((MouseDownEvent _) => SwitchTab(obj));
        tab.Add(b);
        tab.Add(close);
        
        // Add to GUI
        _tabBar.Add(tab);
        _inspectorContent.AddRange(elements);

        // Add to data
        _tabs[obj] = new Tab(elements, tab);
    }

    public void SwitchTab(object obj)
    {
        if (Current is not null)
        {
            _tabs[Current].tab.RemoveFromClassList("focused");
        }
        
        Current = obj;
        
        Window.Show();
        AddTab(obj);
        Refresh();

        _tabs[Current].tab.AddToClassList("focused");
        _inspectorContent.scrollOffset = Vector2.zero;
    }

    private void CloseTab(object obj)
    {
        if (obj == Current)
        {
            Current = null;
        }
        
        // Remove from GUI
        _tabBar.Remove(_tabs[obj].tab);
        _inspectorContent.RemoveRange(_tabs[obj].data);

        // Remove data
        _tabs.Remove(obj);
        
        // Switch tab
        if (_tabs.Count > 0)
        {
            if (Current == null)
            {
                SwitchTab(_tabs.Keys.First());
            }
        }
        else
        {
            Window.Hide();
        }
    }

    private void Refresh()
    {
        if (Current is null)
        {
            return;
        }

        foreach (Tab tab in _tabs.Values)
        {
            foreach (InputBase input in tab.data)
            {
                input.Filter(Current, _flags, _filterInput.value);
            }
        }
    }

    private static List<InputBase> GetElements(object obj)
    {
        List<InputBase> inputs = new();

        const BindingFlags bindingFlags = BindingFlags.Static | 
                                          BindingFlags.Instance | 
                                          BindingFlags.Public | 
                                          BindingFlags.NonPublic;
        
        List<MemberInfo> members = new();
        members.AddRange(obj.GetType().GetAllProperties().Where(IgnoreCompilerAttributes).Where(p => p.GetIndexParameters().Length == 0));
        members.AddRange(obj.GetType().GetAllFields().Where(IgnoreCompilerAttributes));
        members.AddRange(obj.GetType().GetMethods(bindingFlags).Where(IgnoreCompilerAttributes).Where(m => !m.IsSpecialName));
        
        int lineCounter = 0;
        foreach (MemberInfo memberInfo in members)
        {
            try
            {
                InputBase element = InputAccess.GetInput(obj, memberInfo);
                
                element.style.backgroundColor = lineCounter++ % 2 == 0
                    ? new Color(0, 0, 0, 0.1f)
                    : new Color(0, 0, 0, 0);

                inputs.Add(element);
            }
            catch (Exception)
            {
                Reflektor.Log($"Could not add the element {memberInfo.Name}");
            }
        }

        int maxWidth = 0;
        foreach (InputBase input in inputs)
        {
            if (input is InputMethod { HasParameters: true })
            {
                continue;
            }
            if (input.LabelLength > maxWidth)
            {
                maxWidth = input.LabelLength;
            }
        }

        foreach (InputBase input in inputs)
        {
            if (input is not InputMethod { HasParameters: true })
            {
                input.SetLabelWidth(maxWidth);
            }
        }

        inputs.Sort();
        
        return inputs;
    }

    private static bool IgnoreCompilerAttributes(MemberInfo m)
    {
        return m.GetCustomAttribute<CompilerGeneratedAttribute>() == null;
    }

    private void GetPathString()
    {
        string path = Current switch
        {
            null => "null",
            Component c => $"{c.gameObject.GetPath()}|{c.GetType().Name}",
            GameObject g => g.GetPath(),
            _ => Current.ToString(),
        };
        _pathInput.SetValueWithoutNotify(path);
    }

    public void ToggleDisplay()
    {
        if (Window.IsVisible())
        {
            Window.Hide();
        }
        else
        {
            if (_tabs.Count > 0)
            {
                Window.Show();
            }
        }
    }
}