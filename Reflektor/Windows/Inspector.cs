using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reflektor.Windows;

public class Inspector : VisualElement
{
    // GUI Elements
    private readonly TextField _path = new();
    private readonly GroupBox _tabBar = new();
    private readonly ScrollView _tabScrollView = new(ScrollViewMode.Horizontal);

    // Data
    private object? _currentObject;
    private object? _prevObject;

    private readonly Dictionary<object, (int, Button, InspectorTab)> _tabs = new();
    
    public Inspector()
    {
        _path.isDelayed = true;
        _path.RegisterValueChangedCallback(evt =>
        {
            string[] fullPath = evt.newValue.Split("|", 2);
            GameObject? candidate = GameObject.Find(fullPath.First());
            if (candidate is not null)
            {
                string compTargetType = fullPath.Last().Trim();
                if (TryGetCompByName(candidate, compTargetType, out Component? comp))
                {
                    if (comp is not null)
                    {
                        SwitchTab(comp);
                    }
                }
                else
                {
                    SwitchTab(candidate);
                }
            }
            
            string path = _currentObject switch
            {
                GameObject g => g.GetPath(),
                Component c => c.gameObject.GetPath(),
                _ => "/"
            };

            _path.SetValueWithoutNotify(path);
        });
        
        SetStyle();

        Add(_path);
        Add(_tabScrollView);
        
        _tabScrollView.Add(_tabBar);

        try
        {
            AddTab(new TestClass());
            GameObject g = GameObject.Find("/GameManager/Default Game Instance(Clone)/UI Manager(Clone)/Main Canvas");
            if (g is null)
            {
                return;
            }

            AddTab(g);
            RectTransform r = g.GetComponent<RectTransform>();
            if (r is not null)
            {
                AddTab(r);
            }
                
            Canvas c = g.GetComponent<Canvas>();
            if (c is not null)
            {
                AddTab(c);
            }
                
            SwitchTab(g);
        }
        catch (Exception e)
        {
            Reflektor.Log(e.ToString());
            Reflektor.Log(e.Message);
            Reflektor.Log(e.StackTrace);
        }
        
        //this.Hide();
    }

    private static bool TryGetCompByName(GameObject candidate, string compTargetType, out Component? comp)
    {
        foreach (Component c in candidate.GetComponents<Component>())
        {
            string curType = c.GetType().ToString().Split(".").Last().Trim();
            if (!string.Equals(compTargetType, curType, StringComparison.CurrentCultureIgnoreCase))
            {
                continue;
            }

            comp = c;
            return true;
        }

        comp = null;
        return false;
    }

    public void SwitchTab(object obj)
    {
        this.Show();
        AddTab(obj);
        
        _prevObject = _currentObject;

        foreach ((int id, Button b, InspectorTab t) in _tabs.Values)
        {
            b.style.backgroundColor = id % 2 == 0 
                ? new Color(0.3f, 0.3f, 0.3f) 
                : new Color(0.4f, 0.4f, 0.4f);
            
            t.Hide();
        }

        (_, Button curBtn, InspectorTab curTab) = _tabs[obj];
        curTab.Show();
        curBtn.style.backgroundColor = new Color(0.05f, 0.45f, 0.35f);
        curBtn.style.color = Color.white;
        
        _currentObject = obj;
    }

    private void UpdateTabButtonStyles()
    {
        foreach ((int id, Button b, InspectorTab _) in _tabs.Values)
        {
            if (id == 0)
            {
                b.style.borderTopLeftRadius = 6;
                b.style.borderBottomLeftRadius = 6;
            }
            else
            {
                b.style.borderTopLeftRadius = 0;
                b.style.borderBottomLeftRadius = 0;
            }

            if (id == _tabs.Count - 1)
            {
                b.style.borderTopRightRadius = 6;
                b.style.borderBottomRightRadius = 6;
            }
            else
            {
                b.style.borderTopRightRadius = 0;
                b.style.borderBottomRightRadius = 0;
            }
        }
    }

    private void AddTab(object obj)
    {
        if (_tabs.ContainsKey(obj))
        {
            return;
        }

        InspectorTab tabPane = new(obj);
        Button tabButton = new();
        tabButton.text = obj.GetShortName(true);
        tabButton.clicked += () => { SwitchTab(obj); };
        tabButton.RegisterCallback((MouseDownEvent evt) =>
        {
            if (evt.button == 1)
            {
                CloseTab(obj);
            }
        });
        
        _tabs[obj] = (_tabs.Count, tabButton, tabPane);
        _tabBar.Add(tabButton);
        Add(tabPane);
        
        UpdateTabButtonStyles();
    }

    private void CloseTab(object obj)
    {
        (_, Button curBtn, InspectorTab curTab) = _tabs[obj];

        _tabBar.Remove(curBtn);
        _tabs.Remove(obj);
        Remove(curTab);
        
        _tabs.Reorder();
    
        UpdateTabButtonStyles();

        if (obj == _prevObject)
        {
            _prevObject = null;
        }

        if (obj == _currentObject)
        {
            _currentObject = null;
        }

        if (_prevObject is not null)
        {
            SwitchTab(_prevObject);
            return;
        }

        if (_tabs.Count > 0)
        {
            SwitchTab(_tabs.Keys.First());
        }
        else
        {
            // No more tabs left
            this.Hide();
        }
    }

    private void SetStyle()
    {
        AddToClassList("root");
        
        style.width = 1500;
        style.minWidth = 1200;
        style.maxWidth = 1200;
        style.height = 900;
        style.minHeight = 900;
        style.maxHeight = 900;

        _path.style.fontSize = 12;

        _tabScrollView.style.backgroundColor = new StyleColor(StyleKeyword.None);
        _tabScrollView.style.flexDirection = FlexDirection.Row;
        _tabScrollView.style.maxHeight = 44;
        _tabScrollView.style.minHeight = 44;
        _tabScrollView.style.height = 44;
        _tabScrollView.style.paddingLeft = 0;
        _tabScrollView.style.paddingRight = 0;
        _tabScrollView.style.paddingBottom = 0;
        _tabScrollView.style.paddingTop = 0;
        _tabScrollView.style.marginLeft = 0;
        _tabScrollView.style.marginRight = 0;
        _tabScrollView.style.marginBottom = 0;
        _tabScrollView.style.marginTop = 0;
        _tabBar.style.paddingLeft = 0;
        _tabBar.style.paddingRight = 0;
        _tabBar.style.paddingBottom = 0;
        _tabBar.style.paddingTop = 0;
        
        _tabBar.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
    }
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "CollectionNeverQueried.Local")]
public class TestClass
{
    private List<int> intList { get; set; } = new();
    private readonly List<float> _floatList = new();
    private readonly List<string> _stringList = new();
    
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
}