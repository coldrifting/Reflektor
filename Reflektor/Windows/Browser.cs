using System;
using System.Collections.Generic;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reflektor.Windows;

public class Browser : VisualElement
{
    // Events
    public event Action<GameObject?>? CurrentChangedEvent;

    // GUI Components
    private readonly TextField _path = new();
    private readonly VisualElement _splitter = new();
    
    private readonly BrowserRaycastPane _raycastPane;
    private readonly BrowserObjectPane _objectPane;
    
    // Data
    public readonly List<string> DisallowDisableList = new()
    {
        "GameManager",
        "KerbalPanelSettings",
        "_Inspector",
        "_Inspector_Browser"
    };
    
    private GameObject? _current;
    public GameObject? Current
    {
        get => _current;
        set
        {
            _path.SetValueWithoutNotify(value != null ? value.GetPath() : "/");
            _current = value;
            
            CurrentChangedEvent?.Invoke(value);
        }
    }

    public Browser(Inspector objectInspector)
    {
        _path.isDelayed = true;
        _path.RegisterValueChangedCallback(evt =>
        {
            GameObject? candidate = GameObject.Find(evt.newValue);
            if (candidate is null)
            {
                _path.SetValueWithoutNotify(evt.previousValue);
            }
            else
            {
                Current = candidate;
                HideRaycastResults();
            }
        });

        _raycastPane = new BrowserRaycastPane(this);
        _raycastPane.Hide();
        _splitter.Add(_raycastPane);

        _objectPane = new BrowserObjectPane(this);
        _splitter.Add(_objectPane);
        
        _splitter.Add(new BrowserValuePane(this, objectInspector));
        
        Add(_path);
        Add(_splitter);
        
        SetStyle();
    }
    
    private void SetStyle()
    {
        AddToClassList("root");

        style.position = Position.Absolute;
        style.width = 650;
        style.paddingTop = 12;
        style.paddingBottom = 12;
        style.paddingLeft = 12;
        style.paddingRight = 12;

        _path.style.fontSize = 12;
        _path.ElementAt(0).style.borderTopLeftRadius = 3;
        _path.ElementAt(0).style.borderTopRightRadius = 3;
        _path.ElementAt(0).style.borderBottomLeftRadius = 3;
        _path.ElementAt(0).style.borderBottomRightRadius = 3;
        
        _splitter.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
        _splitter.style.minHeight = 600;
        _splitter.style.maxHeight = 600;
    }

    public void ShowRaycastResults(IEnumerable<GameObject> gameObjects)
    {
        _raycastPane.UpdateRaycastList(gameObjects);
        _objectPane.Hide();
        _raycastPane.Show();
    }

    public void HideRaycastResults()
    {
        _raycastPane.Hide();
        _objectPane.Show();
    }
}