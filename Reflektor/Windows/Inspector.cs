using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Reflektor.Elements;
using UitkForKsp2.API;
using UniLinq;
using UnityEngine;
using UnityEngine.UIElements;
using System.Runtime.CompilerServices;

namespace Reflektor.Windows;

public class Inspector : BaseWindow
{
    // GUI Elements
    private readonly VisualElement _tabBar;
    private readonly TextField _pathInput;
    
    private readonly TextField _searchInput;
    private readonly Toggle _propertyToggle;
    private readonly Toggle _fieldToggle;
    private readonly Toggle _methodToggle;
    private readonly Toggle _readOnlyToggle;
    
    private readonly Toggle _autoRefreshToggle;
    private readonly Button _refreshButton;
    
    private readonly VisualElement _inspectorContent;

    // Data
    private readonly Dictionary<object, Vector2> _tabOffset = new();
    private readonly Dictionary<object, (VisualElement, ScrollView)> _tabs = new();
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
        
        _searchInput = Window.rootVisualElement.Q<TextField>(name: "SearchInput");
        
        _propertyToggle = Window.rootVisualElement.Q<Toggle>(name: "PropertyToggle");
        _fieldToggle = Window.rootVisualElement.Q<Toggle>(name: "FieldToggle");
        _methodToggle = Window.rootVisualElement.Q<Toggle>(name: "MethodToggle");
        _readOnlyToggle = Window.rootVisualElement.Q<Toggle>(name: "ReadOnlyToggle");
        
        _autoRefreshToggle = Window.rootVisualElement.Q<Toggle>(name: "AutoRefreshToggle");
        _refreshButton = Window.rootVisualElement.Q<Button>(name: "RefreshButton");

        _inspectorContent = Window.rootVisualElement.Q<VisualElement>(name: "InspectorContent");

        // Add callbacks
        _pathInput.RegisterValueChangedCallback(evt =>
        {
            string[] fullPath = evt.newValue.Split("|", 2);
            GameObject? candidate = GameObject.Find(fullPath.First());
            Reflektor.Log("Find");
            if (candidate is not null)
            {
                Reflektor.Log("Find Not Null");
                string compTargetType = fullPath.Last().Trim();
                Component? comp = candidate.GetComponentByType(compTargetType);
                SwitchTab(comp != null ? comp : candidate);
            }

            GetPathString();
        });

        _searchInput.RegisterValueChangedCallback(evt =>
        {
            UpdateDisplay();
        });

        _propertyToggle.RegisterValueChangedCallback(evt => ToggleFlag(DisplayFlags.Properties, evt.newValue));
        _fieldToggle.RegisterValueChangedCallback(evt => ToggleFlag(DisplayFlags.Fields, evt.newValue));
        _methodToggle.RegisterValueChangedCallback(evt => ToggleFlag(DisplayFlags.Methods, evt.newValue));
        _readOnlyToggle.RegisterValueChangedCallback(evt => ToggleFlag(DisplayFlags.ReadOnly, evt.newValue));

        _refreshButton.clicked += UpdateDisplay;
        
        // Add Default Tabs
        _tabBar.Clear();
        Window.Hide();
        //SwitchTab(GameObject.Find("/GameManager"));
        SwitchTab(new TestClass());
    }

    private void ToggleFlag(DisplayFlags flag, bool shouldSet)
    {
        Reflektor.Log($"{flag}, {shouldSet}");
        
        _flags = shouldSet ? _flags | flag : _flags & ~flag;
        
        Reflektor.Log(_flags);
        
        UpdateDisplay();
    }

    private void AddTab(object obj)
    {
        if (_tabs.ContainsKey(obj))
        {
            return;
        }
        
        Label b = new(obj.GetTabName());
        Button close = new(() => CloseTab(obj));
        close.text = "\u2717"; // Close Icon

        VisualElement tab = new();
        tab.RegisterCallback((MouseDownEvent _) => SwitchTab(obj));
        tab.Add(b);
        tab.Add(close);

        List<EBase> elements = GetElements(obj);

        ScrollView s = new();
        _inspectorContent.Add(s);
        
        _tabs[obj] = (tab, s);
        _tabBar.Add(tab);
        foreach (EBase? e in elements)
        {
            s.Add(e);
        }
    }

    public void SwitchTab(object obj)
    {
        if (Current is not null)
        {
            _tabs[Current].Item1.RemoveFromClassList("focused");

            _tabOffset[Current] = _tabs[Current].Item2.scrollOffset;
            Reflektor.Log(_tabOffset[Current]);
        }
        
        Current = obj;
        
        Window.Show();
        AddTab(obj);
        UpdateDisplay();

        foreach (VisualElement v in _inspectorContent.Children())
        {
            v.Hide();
        }

        _tabs[obj].Item2.Show();
        if (_tabOffset.TryGetValue(obj, out Vector2 scrollPos))
        {
            if (Reflektor.Instance != null)
            {
                Reflektor.Instance.StartCoroutine(UpdateScrollView(obj, scrollPos));
            }
        }

        _tabs[obj].Item1.AddToClassList("focused");
    }

    private IEnumerator UpdateScrollView(object obj, Vector2 scrollPos)
    {
        yield return new WaitForEndOfFrame();
        
        _tabs[obj].Item2.scrollOffset = scrollPos;
    }

    private void CloseTab(object obj)
    {
        if (obj == Current)
        {
            Current = null;
        }
        
        _tabBar.Remove(_tabs[obj].Item1);
        _inspectorContent.Remove(_tabs[obj].Item2);
        _tabOffset.Remove(obj);

        _tabs.Remove(obj);
        
        if (_tabs.Count > 0)
        {
            SwitchTab(_tabs.Keys.First());
        }
        else
        {
            Window.Hide();
        }
    }

    private void UpdateDisplay()
    {
        foreach ((object obj, (VisualElement _, ScrollView s)) in _tabs)
        {
            foreach (VisualElement? e in s.Children())
            {
                if (e is EBase b)
                {
                    b.Refresh(obj == Current && b.GetName().Contains(_searchInput.value, StringComparison.InvariantCultureIgnoreCase)? _flags : DisplayFlags.None);
                }
            }
        }
    }

    private static List<EBase> GetElements(object obj)
    {
        List<EBase> output = new();
        
        int counter = 0;

        List<MemberInfo> members = new();
        members.AddRange(obj.GetType().GetAllProperties().Where(IgnoreCompilerAttributes));
        members.AddRange(obj.GetType().GetAllFields().Where(IgnoreCompilerAttributes));
        members.AddRange(obj.GetType().GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(IgnoreCompilerAttributes).Where(m => !m.IsSpecialName));

        int maxsize = 0;
        foreach (MemberInfo memberInfo in members)
        {
            try
            {
                EBase element = Utils.GetElement(obj, memberInfo);

                element.style.backgroundColor = counter++ % 2 == 0
                    ? new Color(0, 0, 0, 0.1f)
                    : new Color(0, 0, 0, 0);
                
                output.Add(element);

                if (!element.ConsiderForLengthCalc)
                {
                    continue;
                }
                
                int size = element.GetName().StripHtml().Length;
                if (size > maxsize)
                {
                    maxsize = size;
                }
            }
            catch (Exception)
            {
                Reflektor.Log($"Could not add the element {memberInfo.Name}");
            }
        }

        foreach (EBase b in output)
        {
            b.SetLabelLength(maxsize * 8);
        }

        return output;
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
}