using System;
using System.Collections.Generic;
using RTG;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reflektor.Windows;

public class Browser : BaseWindow
{
    // Events
    public event Action<GameObject?>? CurrentChangedEvent;
    
    // GUI Elements
    private readonly TextField _path;
    private readonly Toggle _parentEditToggle;
    
    private readonly VisualElement? _objectPane;
    private readonly VisualElement? _raycastPane;
    private readonly Button _raycastBackBtn;
    private readonly ListView _raycastList;
    
    // Data
    public GameObject? Current { get; private set; }
    private readonly List<GameObject> _raycastObjects = new();

    private readonly BrowserValues _browserValues;
    
    private bool _isParentEditMode;

    public Browser(GameObject parent) : base(parent, "BrowserWindow")
    {
        // Find GUI elements
        _path = Window.rootVisualElement.Q<TextField>(name: "PathInput");
        _parentEditToggle = Window.rootVisualElement.Q<Toggle>(name: "EditParentToggle");
        _raycastBackBtn = Window.rootVisualElement.Q<Button>(name: "RaycastBackBtn");
        _raycastList = Window.rootVisualElement.Q<ListView>(name: "RaycastList");
        _objectPane = Window.rootVisualElement.Q<VisualElement>(name: "ObjectPane");
        _raycastPane = Window.rootVisualElement.Q<VisualElement>(name: "RaycastPane");
        
        // Add callbacks
        _path.RegisterValueChangedCallback(evt =>
        {
            FindGameObject(evt.newValue);
        });
        
        _parentEditToggle.SetEnabled(false);
        _parentEditToggle.RegisterValueChangedCallback(evt =>
        {
            if (Current is null)
            {
                _isParentEditMode = false;
                _parentEditToggle.SetValueWithoutNotify(false);
                return;
            }
            
            _isParentEditMode = evt.newValue;
            if (_isParentEditMode)
            {
                _path.SetValueWithoutNotify(Current.transform.parent is null
                    ? "... No Parent ..."
                    : Current.transform.parent.gameObject.GetPath());
            }
            else
            {
                Refresh();
            }
        });
        
        var browserObjects = new BrowserObjects(this, Window.rootVisualElement);
        browserObjects.Setup();
        
        _browserValues = new BrowserValues(this, Window.rootVisualElement);
        _browserValues.Setup();
        
        // Setup raycast
        _raycastBackBtn.clicked += Refresh;
        _raycastList.itemsSource = _raycastObjects;
        _raycastList.makeItem = () => new Label();
        _raycastList.bindItem = (element, i) =>
        {
            if (element is not Label label)
            {
                return;
            }

            label.text = _raycastObjects[i].name.ToString();
            label.RegisterCallback<MouseDownEvent>(_ =>
            {
                Refresh(_raycastObjects[i]);
            });
        };

        // Testing
        Window.rootVisualElement.Hide();
    }

    private void FindGameObject(string newPath)
    {
        if (_isParentEditMode && Current is not null && newPath == "/")
        {
            Current.transform.parent = null;
            _isParentEditMode = false;
            _parentEditToggle.SetValueWithoutNotify(false);
            Refresh();
        }
        else
        {
            GameObject? candidate = GameObject.Find(newPath.RemoveTrailingSlashes());
            if (candidate is null)
            {
                if (_isParentEditMode)
                {
                    _isParentEditMode = false;
                    _parentEditToggle.SetValueWithoutNotify(false);
                }
                Refresh();
            }
            else
            {
                if (_isParentEditMode && Current is not null)
                {
                    Current.transform.parent = candidate.transform;
                    _isParentEditMode = false;
                    _parentEditToggle.SetValueWithoutNotify(false);

                    Refresh();
                }
                else
                {
                    Refresh(candidate);
                }
            }
        }
    }
    
    public void Refresh(GameObject? newGameObject)
    {
        Current = newGameObject;
        Refresh();
    }
    
    public void Refresh()
    {
        HideRaycastResults();
        
        _path.SetValueWithoutNotify(Current != null ? Current.GetPath() : "/");

        _isParentEditMode = false;
        _parentEditToggle.SetValueWithoutNotify(false);
        _parentEditToggle.SetEnabled(Current is not null);
            
        CurrentChangedEvent?.Invoke(Current);
    }

    public void Up()
    {
        Refresh(Current != null && Current.transform.parent != null
                ? Current.transform.parent.gameObject
                : null);
    }

    public void ToggleDisplay()
    {
        Window.rootVisualElement.ToggleDisplay();
    }

    public void ShowRaycastResults(IEnumerable<GameObject> objects)
    {
        Window.rootVisualElement.Show();
        
        _path.SetEnabled(false);
        _raycastPane.Show();
        _objectPane.Hide();
        _browserValues.Disable();
        _parentEditToggle.SetEnabled(false);
        
        _raycastObjects.Clear();
        _raycastObjects.AddRange(objects);

        _raycastList.Rebuild();
        _raycastList.SetEmptyText("(No Objects Found)", "#FF7700");
    }

    private void HideRaycastResults()
    {
        if (_raycastPane is null || _objectPane is null)
        {
            return;
        }

        _browserValues.Enable();
        _parentEditToggle.SetEnabled(true);
        _path.SetEnabled(true);
        _raycastPane.Hide();
        _objectPane.Show();
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