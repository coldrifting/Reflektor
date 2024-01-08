using RTG;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reflektor.Windows;

public static class Browser
{
    // Events
    public static event Action<GameObject?>? CurrentChangedEvent;
    
    // Data
    public static GameObject? Current { get; private set; }
    private static readonly List<GameObject> RaycastObjects = new();
    private static readonly BrowserValues BrowserValues;
    private static bool _isParentEditMode;
    
    // GUI Elements
    private static readonly UIDocument Window;
    
    private static readonly TextField Path;
    private static readonly Toggle ParentEditToggle;
    private static readonly VisualElement? ObjectPane;
    private static readonly VisualElement? RaycastPane;
    private static readonly Button RaycastBackBtn;
    private static readonly ListView RaycastList;

    static Browser()
    {
        Window = Utils.GetNewWindow("BrowserWindow");
        
        // Find GUI elements
        Path = Window.rootVisualElement.Q<TextField>(name: "PathInput");
        ParentEditToggle = Window.rootVisualElement.Q<Toggle>(name: "EditParentToggle");
        RaycastBackBtn = Window.rootVisualElement.Q<Button>(name: "RaycastBackBtn");
        RaycastList = Window.rootVisualElement.Q<ListView>(name: "RaycastList");
        ObjectPane = Window.rootVisualElement.Q<VisualElement>(name: "ObjectPane");
        RaycastPane = Window.rootVisualElement.Q<VisualElement>(name: "RaycastPane");
        
        // Add callbacks
        Path.RegisterValueChangedCallback(evt =>
        {
            FindGameObject(evt.newValue);
        });
        
        ParentEditToggle.SetEnabled(false);
        ParentEditToggle.RegisterValueChangedCallback(evt =>
        {
            if (Current is null)
            {
                _isParentEditMode = false;
                ParentEditToggle.SetValueWithoutNotify(false);
                return;
            }
            
            _isParentEditMode = evt.newValue;
            if (_isParentEditMode)
            {
                Path.SetValueWithoutNotify(Current.transform.parent is null
                    ? "... No Parent ..."
                    : Current.transform.parent.gameObject.GetPath());
            }
            else
            {
                Refresh();
            }
        });
        
        var browserObjects = new BrowserObjects(Window.rootVisualElement);
        browserObjects.Setup();
        
        BrowserValues = new BrowserValues(Window.rootVisualElement);
        BrowserValues.Setup();
        
        // Setup raycast
        RaycastBackBtn.clicked += Refresh;
        RaycastList.itemsSource = RaycastObjects;
        RaycastList.makeItem = () => new Label();
        RaycastList.bindItem = (element, i) =>
        {
            if (element is not Label label)
            {
                return;
            }

            label.text = RaycastObjects[i].name.ToString();
            label.RegisterCallback<MouseDownEvent>(_ =>
            {
                Refresh(RaycastObjects[i]);
            });
        };

        // Testing
        Window.rootVisualElement.Hide();
    }

    private static void FindGameObject(string newPath)
    {
        if (_isParentEditMode && Current is not null && newPath == "/")
        {
            Current.transform.parent = null;
            _isParentEditMode = false;
            ParentEditToggle.SetValueWithoutNotify(false);
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
                    ParentEditToggle.SetValueWithoutNotify(false);
                }
                Refresh();
            }
            else
            {
                if (_isParentEditMode && Current is not null)
                {
                    Current.transform.parent = candidate.transform;
                    _isParentEditMode = false;
                    ParentEditToggle.SetValueWithoutNotify(false);

                    Refresh();
                }
                else
                {
                    Refresh(candidate);
                }
            }
        }
    }
    
    public static void Refresh(GameObject? newGameObject)
    {
        Current = newGameObject;
        Refresh();
    }
    
    public static void Refresh()
    {
        HideRaycastResults();
        
        Path.SetValueWithoutNotify(Current != null ? Current.GetPath() : "/");

        _isParentEditMode = false;
        ParentEditToggle.SetValueWithoutNotify(false);
        ParentEditToggle.SetEnabled(Current is not null);
            
        CurrentChangedEvent?.Invoke(Current);
    }

    public static void Up()
    {
        Refresh(Current != null && Current.transform.parent != null
                ? Current.transform.parent.gameObject
                : null);
    }

    public static void ToggleDisplay()
    {
        Window.rootVisualElement.ToggleDisplay();
    }

    public static void ShowRaycastResults(IEnumerable<GameObject> objects)
    {
        Window.rootVisualElement.Show();
        
        Path.SetEnabled(false);
        RaycastPane.Show();
        ObjectPane.Hide();
        BrowserValues.Disable();
        ParentEditToggle.SetEnabled(false);
        
        RaycastObjects.Clear();
        RaycastObjects.AddRange(objects);

        RaycastList.Rebuild();
        RaycastList.SetEmptyText("(No Objects Found)", "#FF7700");
    }

    private static void HideRaycastResults()
    {
        if (RaycastPane is null || ObjectPane is null)
        {
            return;
        }

        BrowserValues.Enable();
        ParentEditToggle.SetEnabled(true);
        Path.SetEnabled(true);
        RaycastPane.Hide();
        ObjectPane.Show();
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