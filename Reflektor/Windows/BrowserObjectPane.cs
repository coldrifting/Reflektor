using System;
using System.Collections.Generic;
using System.Reflection;
using UniLinq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Reflektor.Windows;

public class BrowserObjectPane : VisualElement
{
    // GUI Elements
    private readonly GroupBox _browserPaneControls = new();
    private readonly DropdownField _sceneChangeDropdown = new();
    private readonly Button _refreshBtn = new();
    private readonly Button _upBtn = new();
    private readonly Label _currentObjLabel = new();
    private readonly ListView _objectsList = new();
    
    // Data
    private readonly Dictionary<string, Scene> _scenes = new();
    private readonly List<GameObject> _objects = new();

    public BrowserObjectPane(Browser browser)
    {
        _objectsList.itemsSource = _objects;
        _objectsList.makeItem = () => new Label();
        _objectsList.bindItem = (element, index) =>
        {
            var gObj = _objects[index];
            if (element is not Label label)
            {
                return;
            }

            int numChildren = gObj.transform.childCount;
            const string colGreen = "<color=#00AA77>";
            const string colGray = "<color=#777777>";
            const string colEnd = "</color>";
            string prefix = numChildren > 0 
                ? $"{colGray}[{colEnd}{colGreen}{numChildren}{colEnd}{colGray}]{colEnd} " 
                : "";
            label.text = prefix + gObj.name;
            label.style.color = gObj.activeSelf ? Color.white : Color.grey;
            
            // Right click to disable game objects
            label.RegisterCallback((MouseDownEvent evt) =>
            {
                if (evt.button != 1 || browser.DisallowDisableList.Contains(gObj.name))
                {
                    return;
                }

                gObj.SetActive(!gObj.activeSelf);
                _objectsList.Rebuild();
            });
        };
        
        // First event needs to be set for the second one to work
        _objectsList.selectedIndicesChanged += _ => { };
        _objectsList.itemsChosen += enumerable =>
        {
            if (enumerable.First() is GameObject gObj)
            {
                browser.Current = gObj;
            }
        };
        
        // Scene change stuff
        GetScenes();
        _sceneChangeDropdown.choices = new List<string>();
        foreach (var scene in _scenes.Keys.ToList())
        {
            _sceneChangeDropdown.choices.Add(scene);
        }
        _sceneChangeDropdown.RegisterValueChangedCallback(_ => browser.Current = null);
        
        _sceneChangeDropdown.value = _sceneChangeDropdown.choices.Last();

        _refreshBtn.clicked += () => UpdateObjects(browser.Current);
        _upBtn.clicked += () =>
        {
            if (browser.Current is not null && browser.Current.transform.parent is not null)
            {
                browser.Current = browser.Current.transform.parent.gameObject;
            }
            else
            {
                browser.Current = null;
            }
        };
        
        _browserPaneControls.Add(_upBtn);
        _browserPaneControls.Add(_refreshBtn);
        _browserPaneControls.Add(_currentObjLabel);

        Add(_sceneChangeDropdown);
        Add(_browserPaneControls);
        Add(_objectsList);
        
        browser.CurrentChangedEvent += obj =>
        {
            if (obj is not null)
            {
                _sceneChangeDropdown.SetValueWithoutNotify(obj.scene.name); //$"<color=#FFFFFF>{obj.scene.name}</color>");
                _currentObjLabel.text = obj.name.Truncate(15);
            }
            else
            {
                _currentObjLabel.text = "";
            }

            UpdateObjects(obj);
        };

        // Trigger refresh
        browser.Current = null;
        
        SetStyle();
    }

    private void UpdateObjects(GameObject? gObj)
    {
        _objects.Clear();
        if (gObj is not null)
        {
            foreach (Transform childTransform in gObj.transform)
            {
                _objects.Add(childTransform.gameObject);
            }
        }
        else
        {
            GameObject[] objs = _scenes[_sceneChangeDropdown.value].GetRootGameObjects();
            foreach (var obj in objs)
            {
                _objects.Add(obj);
            }
        }

        _objectsList.Rebuild();
        Utils.SetListViewEmptyText(_objectsList, "<color=#777777>(No Child Transforms)</color>");
    }

    private void SetStyle()
    {
        style.paddingTop = 0;
        style.paddingBottom = 0;
        style.paddingLeft = 0;
        style.paddingRight = 0;

        style.marginTop = 0;
        style.marginBottom = 0;
        style.marginLeft = 0;
        style.marginRight = 0;
        
        style.width = 300;
        style.minWidth = 300;
        style.maxWidth = 300;

        _sceneChangeDropdown.style.width = Length.Percent(100);
        _sceneChangeDropdown.style.marginRight = 0;
        _sceneChangeDropdown.style.marginLeft = 0;
        Utils.DropdownFix(_sceneChangeDropdown);
        
        _browserPaneControls.style.flexDirection = FlexDirection.Row;
        _browserPaneControls.style.marginLeft = 0;
        _browserPaneControls.style.paddingLeft = 0;
        _browserPaneControls.style.width = Length.Percent(100);
        
        _upBtn.text = "Up";
        _upBtn.style.marginLeft = 0;
        _upBtn.style.marginRight = 8;
        _refreshBtn.text = "Refresh";
        _refreshBtn.style.marginRight = 8;
        
        _objectsList.style.flexGrow = 1;
        _objectsList.style.backgroundColor = Reflektor.ColorFromHex(0x192128);
        _objectsList.style.borderTopLeftRadius = 6;
        _objectsList.style.borderTopRightRadius = 6;
        _objectsList.style.borderBottomLeftRadius = 6;
        _objectsList.style.borderBottomRightRadius = 6;
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
}