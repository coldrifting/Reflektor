using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

using Reflektor.Extensions;
using UitkForKsp2.API;

namespace Reflektor;

public class WindowTitle : VisualElement
{
    private readonly VisualElement _root;
    
    private readonly TextField _path = new();
    
    private readonly GroupBox _tabBar = new();
    private readonly ScrollView _tabScrollView = new(ScrollViewMode.Horizontal);

    private readonly Dictionary<object, (Button, WindowTab)> _tabs = new();

    public static WindowTitle? Instance;

    private object? _currentObject;
    private object? _lastObject;
    
    public WindowTitle()
    {
        Instance = this;

        _root = Element.Root();

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

        _root.Add(_path);
        _root.Add(_tabScrollView);

        try
        {
            GameObject g = GameObject.Find("/GameManager/Default Game Instance(Clone)/UI Manager(Clone)/Main Canvas");
            if (g is not null)
            {
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
        }
        catch (Exception e)
        {
            Reflektor.Log(e.ToString());
            Reflektor.Log(e.Message);
            Reflektor.Log(e.StackTrace);
        }

        Add(_root);
        _root.AddManipulator(new DragManipulator(true));
    }

    private static bool TryGetCompByName(GameObject candidate, string compTargetType, out Component? comp)
    {
        foreach (Component c in candidate.GetComponents<Component>())
        {
            string curType = c.GetType().ToString().Split(".").Last().Trim();
            if (compTargetType != curType)
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
        AddTab(obj);
        
        _lastObject = _currentObject;

        int counter = 0;
        foreach ((Button b, WindowTab t) in _tabs.Values.ToArray())
        {
            b.style.backgroundColor = counter++ % 2 == 0 
                ? new Color(0.5f, 0.5f, 0.5f) 
                : new Color(0.4f, 0.4f, 0.4f);
            
            t.SetVisible(false);
        }
        
        (Button curBtn, WindowTab curTab) = _tabs[obj];
        curTab.SetVisible(true);
        curBtn.style.backgroundColor = new Color(0.05f, 0.45f, 0.35f);
        curBtn.style.color = Color.white;

        _currentObject = obj;
    }

    private void SetTabButtonStyles()
    {
        int counter = 0;
        var bt = _tabs.Values.ToArray();
        foreach ((Button b, WindowTab _) in bt)
        {
            b.style.borderTopLeftRadius = 0;
            b.style.borderBottomLeftRadius = 0;
            b.style.borderTopRightRadius = 0;
            b.style.borderBottomRightRadius = 0;
            
            if (counter == 0)
            {
                b.style.borderTopLeftRadius = 4;
                b.style.borderBottomLeftRadius = 4;
            }

            if (counter == bt.Length - 1)
            {
                b.style.borderTopRightRadius = 4;
                b.style.borderBottomRightRadius = 4;
            }
            
            counter++;
        }
    }

    private void AddTab(object obj)
    {
        if (_tabs.ContainsKey(obj))
        {
            return;
        }

        WindowTab tabPane = new(obj);
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

        _tabBar.Add(tabButton);
        _root.Add(tabPane);

        _tabs.Add(obj, (tabButton, tabPane));
        SetTabButtonStyles();
    }

    private void CloseTab(object obj)
    {
        (Button button, WindowTab windowTab) = _tabs[obj];
        _tabBar.Remove(button);
        _tabs.Remove(obj);
        _root.Remove(windowTab);

        SetTabButtonStyles();

        if (obj == _lastObject)
        {
            _lastObject = null;
        }

        if (obj == _currentObject)
        {
            _currentObject = null;
        }

        if (_lastObject is not null)
        {
            SwitchTab(_lastObject);
            return;
        }
        
        object? objToSwitch = _tabs.Keys.First();
        if (objToSwitch is not null)
        {
            SwitchTab(objToSwitch);
        }
        else
        {
            // No more tabs left
            this.SetVisible(false);
        }
    }

    private void SetStyle()
    {
        _root.style.flexGrow = 1;
        _root.style.width = 1500;
        _root.style.minWidth = 1200;
        _root.style.maxWidth = 1200;
        _root.style.height = 900;
        _root.style.minHeight = 900;
        _root.style.maxHeight = 900;

        _path.style.fontSize = 12;

        _tabScrollView.Add(_tabBar);
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