using System;
using System.Collections.Generic;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reflektor.Windows;

public class Browser
{
    // Events
    public event Action<GameObject?>? CurrentChangedEvent;
    
    // GUI Elements
    private readonly VisualElement _root;
    private readonly TextField _path;
    
    // Data
    private GameObject? _current;
    public GameObject? Current
    {
        get => _current;
        set
        {
            _current = value;

            _oldPath = value != null ? value.GetPath() : "/";
            _path.SetValueWithoutNotify(_oldPath);
            SetPathFontSize();
            
            CurrentChangedEvent?.Invoke(value);
        }
    }
    private readonly BrowserRaycast _browserRaycast;
    private string _oldPath = "/";

    public Browser(VisualElement root, Inspector inspector)
    {
        _root = root;
        _path = root.Q<TextField>(name: "PathInput");
        _path.isDelayed = false;
        _path.RegisterValueChangedCallback(evt =>
        {
            SetPathFontSize();
        });
        
        _path.RegisterCallback((FocusOutEvent evt) =>
        {
            if (evt.target is TextField text)
            {
                string newPath = text.text;
                FindGameObject(newPath);
            }
        });

        _browserRaycast = new BrowserRaycast(this, root);
        _browserRaycast.Setup();
        
        var browserObjects = new BrowserObjects(this, root);
        browserObjects.Setup();
        
        var browserValues = new BrowserValues(this, root, inspector);
        browserValues.Setup();
    }

    private void FindGameObject(string newPath)
    {
        GameObject? candidate = GameObject.Find(newPath);
        if (candidate is null)
        {
            _path.SetValueWithoutNotify(_oldPath);
            SetPathFontSize();
        }
        else
        {
            _browserRaycast.HideRaycastResults();
            Current = candidate;
            _oldPath = newPath;
        }
    }

    private void SetPathFontSize()
    {
        float divisor = Math.Max(1.0f, _path.value.Length / 75.0f);
        _path.style.fontSize = Math.Max(14 / divisor, 9);
    }

    public void ToggleDisplay()
    {
        _root.ToggleDisplay();
    }

    public void Refresh()
    {
        Current = Current;
    }

    public void Up()
    {
        Current = Current != null && Current.transform.parent != null 
            ? Current.transform.parent.gameObject 
            : null;
    }

    public void ShowRaycastResults(IEnumerable<GameObject> objects)
    {
        _root.Show();
        _browserRaycast.ShowRaycastResults(objects);
    }
    
    public static bool CanDisable(string name)
    {
        List<string> disallowDisableList = new()
        {
            "GameManager",
            "KerbalPanelSettings",
            "_Inspector",
            "_Inspector_Browser"
        };
        
        return !disallowDisableList.Contains(name);
    }
}