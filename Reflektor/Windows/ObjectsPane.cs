using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Reflektor.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

namespace Reflektor.Windows;

public class ObjectsPane
{
    // GUI elements
    public VisualElement RootElement { get; }
    
    private readonly MainWindow _window;
    
    private readonly GroupBox _objectListControls = new();
    private readonly DropdownField _sceneChangeDropdown = new();
    private readonly Button _refreshBtn = new();
    private readonly Button _upBtn = new();
    private readonly Label _currentObjLabel = new();
    
    private readonly ListView _objectsList = new();
    
    // Data
    private readonly Dictionary<string, Scene> _scenes = new();
    private readonly List<GameObject> _objects = new();
    
    public ObjectsPane(MainWindow window)
    {
        _window = window;
        RootElement = new VisualElement();
        
        // Objects list
        _objectsList.itemsSource = _objects;
        _objectsList.makeItem = () => new Label();
        _objectsList.bindItem = (element, index) =>
        {
            var gObj = _objects[index];
            if (element is not Label label)
            {
                return;
            }

            int numChildren = GetChildren(gObj).Count();
            string prefix = numChildren > 0 ? $"<color=#777777>[</color><color=#00AA77>{numChildren.ToString()}</color><color=#777777>]</color> " : "";
            label.text = prefix + gObj.name;
            label.style.color = gObj.activeSelf ? Color.white : Color.grey;
            //label.SetHoverColor();
            label.RegisterCallback((MouseDownEvent evt) =>
            {
                if (evt.button != 1 || gObj.name == $"_{Reflektor.ModName}")
                {
                    return;
                }

                gObj.SetActive(!gObj.activeSelf);
                _objectsList.Rebuild();
            });
        };
        _objectsList.selectionType = SelectionType.Single;
        
        _objectsList.selectedIndicesChanged += _ => { }; // Needs to be set
        _objectsList.itemsChosen += enumerable =>
        {
            _window.Cur = (GameObject)enumerable.First();
        };

        _window.SelectedObjectChangedEvent += () =>
        {
            GameObject? cur = _window.Cur;
            if (cur is not null)
            {
                _sceneChangeDropdown.SetValueWithoutNotify($"<color=#FFFFFF>{cur.scene.name}</color>");
            }
            UpdateObjects();
            string text = cur != null ? cur.name : "";
            _currentObjLabel.text = text.Truncate(15);
        };
        
        // Scene change stuff
        GetScenes();
        _sceneChangeDropdown.choices = new List<string>();
        foreach (var v in _scenes.Keys.ToList())
        {
            _sceneChangeDropdown.choices.Add($"<color=#FFFFFF>{v}</color>");
        }
        _sceneChangeDropdown.RegisterValueChangedCallback(_ =>
        {
            _window.Cur = null;
        });
        
        _sceneChangeDropdown.value = _sceneChangeDropdown.choices.Last();
        
        _refreshBtn.clicked += UpdateObjects;
        _upBtn.clicked += () =>
        {
            if (window.Cur is not null && window.Cur.transform.parent is not null)
            {
                window.Cur = window.Cur.transform.parent.gameObject;
            }
            else
            {
                window.Cur = null;
            }
        };
        
        SetStyle();
        
        _objectListControls.Add(_upBtn);
        _objectListControls.Add(_refreshBtn);
        _objectListControls.Add(_currentObjLabel);

        RootElement.Add(_sceneChangeDropdown);
        RootElement.Add(_objectListControls);
        RootElement.Add(_objectsList);
        
        UpdateObjects();
    }

    private void UpdateObjects()
    {
        _objects.Clear();
        if (_window.Cur is not null)
        {
            foreach (Transform childTransform in _window.Cur.transform)
            {
                _objects.Add(childTransform.gameObject);
            }
        }
        else
        {
            GameObject[] objs = _scenes[_sceneChangeDropdown.value.StripHtml()].GetRootGameObjects();
            foreach (var obj in objs)
            {
                _objects.Add(obj);
            }
        }

        _objectsList.Rebuild();
    }

    private void SetStyle()
    {
        _sceneChangeDropdown.style.color = Color.white;
        
        _upBtn.text = "Up";
        _upBtn.style.marginRight = 8;
        _refreshBtn.text = "Refresh";
        _refreshBtn.style.marginRight = 8;
        
        _sceneChangeDropdown.style.minWidth = 220;
        _objectListControls.style.flexDirection = FlexDirection.Row;
        _objectsList.style.minHeight = 500;
    }

    private void GetScenes()
    {
        _scenes.Clear();
        
        int sceneCount = SceneManager.sceneCount;
        for (int i = 0; i < sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            Add(_scenes, scene);
        }

        Add(_scenes, GetDontDestroyScene());
    }

    private static Scene? GetDontDestroyScene()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        
        object scene = new Scene();
        Type type = typeof(Scene);
        FieldInfo? fieldInfo = type.GetField("m_Handle", flags);
        fieldInfo?.SetValue(scene, -12);

        // Make sure we got the dont destroy scene
        Scene output = (Scene)scene;
        return output.name == "DontDestroyOnLoad" 
            ? output 
            : null;
    }
    
    private static void Add(IDictionary<string, Scene> scenes, Scene? s)
    {
        if (s is null)
        {
            return;
        }
        
        scenes.Add(s.Value.name, s.Value);
    }

    private static IEnumerable<GameObject> GetChildren(GameObject obj)
    {
        List<GameObject> objects = new();
        foreach (Transform t in obj.transform)
        {
            objects.Add(t.gameObject);
        }

        return objects;
    }
}