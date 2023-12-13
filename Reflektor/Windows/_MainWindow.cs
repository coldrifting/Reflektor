using System;
using System.Collections;
using System.Collections.Generic;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Reflektor.Windows;

public class MainWindow
{
    public readonly UIDocument Window;
    
    internal readonly GameObject WindowParent = new($"_{Reflektor.ModName}");
    
    internal event Action? SelectedObjectChangedEvent;
    internal event Action? PropertyChangedEvent;
    
    // Internal window data
    private readonly TextField _path = new();
    internal readonly VisualElement Root;
    private readonly VisualElement _objectsPaneRoot;
    private readonly VisualElement _detailsPaneRoot;
    private readonly VisualElement _propertiesPaneRoot;
    
    // Cross window data
    private GameObject? _cur;
    internal GameObject? Cur
    {
        get => _cur;
        set
        {
            Components.Clear();
            RectTransform = null;
            CanvasScaler = null;
            Component = null;
            
            _path.value = value != null ? value.GetPath() : "/";
            if (value is not null)
            {
                var comps = value.GetComponents<Component>();
                foreach (Component c in comps)
                {
                    switch (c)
                    {
                        case RectTransform cRect:
                            RectTransform = cRect;
                            break;
                        case CanvasScaler cs:
                            CanvasScaler = cs;
                            break;
                    }

                    Components.Add(c);
                }
            }
            _cur = value;
            
            UpdateCurrent();
        }
    }

    internal List<Component> Components { get; } = new();
    internal RectTransform? RectTransform { get; private set; }
    internal CanvasScaler? CanvasScaler { get; private set; }

    private Component? _component;
    internal Component? Component
    {
        get => _component;
        set
        {
            _component = value;
            UpdateCurrent();
        }
    }

    internal void UpdateCurrent()
    {
        SelectedObjectChangedEvent?.Invoke();
    }

    internal void UpdateProperties()
    {
        Reflektor.Instance.StartCoroutine(WaitOneFrame());
    }

    private IEnumerator WaitOneFrame()
    {
        yield return 0;
        PropertyChangedEvent?.Invoke();
    }

    public MainWindow()
    {
        Root = Element.Root();

        _path.isDelayed = true;
        _path.RegisterValueChangedCallback(evt =>
        {
            GameObject? candidate = GameObject.Find(evt.newValue);
            if (candidate is not null)
            {
                Cur = candidate;
                _path.SetValueWithoutNotify(Cur.GetPath());
            }
            else
            {
                _path.SetValueWithoutNotify(Cur != null ? Cur.GetPath() : "/");
            }
        });
        
        TwoPaneSplitView splitViewLeft = new(0, 300, TwoPaneSplitViewOrientation.Horizontal);
        TwoPaneSplitView splitViewRight = new(0, 300, TwoPaneSplitViewOrientation.Horizontal);
        
        ObjectsPane objectsPane = new(this);
        _objectsPaneRoot = objectsPane.RootElement;
        splitViewLeft.Add(_objectsPaneRoot);

        DetailsPane detailsPane = new(this);
        _detailsPaneRoot = detailsPane.RootElement;
        splitViewRight.Add(_detailsPaneRoot);

        PropertiesPane propertiesPane = new(this);
        _propertiesPaneRoot = propertiesPane.RootElement;
        splitViewRight.Add(_propertiesPaneRoot);
        
        splitViewLeft.Add(splitViewRight);
        
        SetStyle();
        
        Root.Add(_path);
        Root.Add(splitViewLeft);

        UnityEngine.Object.DontDestroyOnLoad(WindowParent);
        Window = UitkForKsp2.API.Window.CreateFromElement(Root, true, $"{Reflektor.ModName}_MainWindow", WindowParent.transform, true);
    }

    private void SetStyle()
    {
        Root.style.flexGrow = 1;
        Root.style.width = 1200;
        Root.style.height = 600;
        Root.style.minHeight = 600;
        Root.style.maxHeight = 600;

        _path.style.fontSize = 12;
        
        _objectsPaneRoot.style.width = 300;
        _objectsPaneRoot.style.minWidth = 300;
        //_objectsPaneRoot.style.maxWidth = 400;

        _detailsPaneRoot.style.width = 300;
        _detailsPaneRoot.style.minWidth = 300;
        //_detailsPaneRoot.style.maxWidth = 400;

        _propertiesPaneRoot.style.width = 300;
        _propertiesPaneRoot.style.minWidth = 300;
    }
}