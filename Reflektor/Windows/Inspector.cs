using System;
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
    
    private readonly ScrollView _inspectorContent;

    // Data
    private readonly Dictionary<object, (VisualElement, List<BaseElement>)> _tabs = new();
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

        _inspectorContent = Window.rootVisualElement.Q<ScrollView>(name: "InspectorContent");

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
        //SwitchTab(new TestClass());
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

        List<BaseElement> elements = GetElements(obj);
        _tabs[obj] = (tab, elements);
        _tabBar.Add(tab);
        foreach (BaseElement? e in elements)
        {
            _inspectorContent.Add(e);
        }
    }

    public void SwitchTab(object obj)
    {
        if (Current is not null)
        {
            _tabs[Current].Item1.RemoveFromClassList("focused");
        }
        
        Current = obj;
        
        Window.Show();
        AddTab(obj);
        UpdateDisplay();

        _tabs[obj].Item1.AddToClassList("focused");
    }

    private void CloseTab(object obj)
    {
        if (obj == Current)
        {
            Current = null;
        }
        
        _tabBar.Remove(_tabs[obj].Item1);
        foreach (BaseElement v in _tabs[obj].Item2)
        {
            _inspectorContent.Remove(v);
        }

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
        foreach ((object obj, (VisualElement _, List<BaseElement> el)) in _tabs)
        {
            foreach (var b in el)
            {
                b.RefreshDisplay(obj == Current && b.GetName().Contains(_searchInput.value, StringComparison.InvariantCultureIgnoreCase)? _flags : DisplayFlags.None);
            }
        }
    }

    private static List<BaseElement> GetElements(object obj)
    {
        List<BaseElement> output = new();
        
        int counter = 0;

        List<MemberInfo> members = new();
        members.AddRange(obj.GetType().GetAllProperties().Where(IgnoreCompilerAttributes));
        members.AddRange(obj.GetType().GetAllFields().Where(IgnoreCompilerAttributes));
        members.AddRange(obj.GetType().GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(IgnoreCompilerAttributes).Where(m => !m.IsSpecialName));

        foreach (MemberInfo memberInfo in members)
        {
            try
            {
                BaseElement element;

                if (memberInfo is MethodInfo methodInfo)
                {
                    element = new ElementMethod(obj, methodInfo);
                }
                else
                {
                    object? memberObj = memberInfo.GetValue(obj);
                    if (memberObj is null)
                    {
                        continue;
                    }
                    Type memberType = memberObj.GetType();
                    if (memberType.IsEnum)
                    {
                        element = new ElementEnum(obj, memberInfo);
                    }
                    else if (memberType.IsArray)
                    {
                        element = new ElementCollection(obj, memberInfo, true);
                    }
                    else if (memberType.IsGenericList())
                    {
                        element = new ElementCollection(obj, memberInfo);
                    }
                    else
                    {
                        element = GetElement(obj, memberInfo);
                    }
                }

                element.style.backgroundColor = counter++ % 2 == 0
                    ? new Color(0, 0, 0, 0.1f)
                    : new Color(0, 0, 0, 0);
                
                output.Add(element);
            }
            catch (Exception)
            {
                Reflektor.Log($"Could not add the element {memberInfo.Name}");
            }
        }

        return output;
    }

    private static bool IgnoreCompilerAttributes(MemberInfo m)
    {
        return m.GetCustomAttribute<CompilerGeneratedAttribute>() == null;
    }

    private static BaseElement GetElement(object obj, MemberInfo memberInfo)
    {
        BaseElement x = memberInfo.GetValue(obj) switch
        {
            int => new ElementInt(obj, memberInfo),
            float => new ElementFloat(obj, memberInfo),
            double => new ElementDouble(obj, memberInfo),
            bool => new ElementBool(obj, memberInfo),
            string => new ElementString(obj, memberInfo),
            Vector2 => new ElementVector2(obj, memberInfo),
            Vector3 => new ElementVector3(obj, memberInfo),
            Vector4 => new ElementVector4(obj, memberInfo),
            Quaternion => new ElementQuaternion(obj, memberInfo),
            Color => new ElementColor(obj, memberInfo),
            _ => new ElementObject(obj, memberInfo)
        };
        return x;
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

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "CollectionNeverQueried.Local")]
public class TestClass
{
    private List<int> intList { get; set; } = new();
    private readonly List<float> _floatList = new();
    private readonly List<string> _stringList = new();

    private string[] values = new string[] { "A", "KSP 2", "test" };

    private bool val = true;
    
    public TestClass()
    {
        intList.Add(1);
        intList.Add(0);
        intList.Add(36);

        _floatList.Add(3.0f);
        _floatList.Add(-1.5f);

        _stringList.Add("A");
        _stringList.Add("B");
        _stringList.Add("C");
        _stringList.Add("D");
    }

    private static bool StaticBoolFalseMethod()
    {
        return false;
    }
    
    private bool InstanceBoolTrueMethod()
    {
        return val;
    }
    
    public void InstanceVoidMethod()
    {
        Debug.Log("Nothing to see here...");
    }
}