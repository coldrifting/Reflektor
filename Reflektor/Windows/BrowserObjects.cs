using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Reflektor.Windows;

public class BrowserObjects
{
    // GUI
    private readonly DropdownField _sceneChangeDropdown;
    private readonly Button _upBtn;
    private readonly Button _refreshBtn;
    private readonly Label _curObjLabel;
    
    private readonly ListView _objectList;
    
    // Data
    private readonly Browser _browser;
    private readonly Dictionary<string, Scene> _scenes = new();
    private readonly List<GameObject> _objects = new();

    public BrowserObjects(Browser browser, VisualElement root)
    {
        _browser = browser;
        
        _sceneChangeDropdown = root.Q<DropdownField>(name: "SceneChangeDropdown");
        _upBtn = root.Q<Button>(name: "UpBtn");
        _refreshBtn = root.Q<Button>(name: "RefreshBtn");
        _curObjLabel = root.Q<Label>(name: "CurObjLabel");
        _objectList = root.Q<ListView>(name: "ObjectList");
    }
    
    public void Setup() 
    {
        GetScenes();
        _sceneChangeDropdown.choices = new List<string>();
        foreach (var scene in _scenes.Values)
        {
            _sceneChangeDropdown.choices.Add(scene.name);
        }
        _sceneChangeDropdown.RegisterValueChangedCallback(_ => _browser.Refresh(null));
        _sceneChangeDropdown.SetValueWithoutNotify(_sceneChangeDropdown.choices.Last());
        
        _upBtn.clicked += _browser.Up;
        _refreshBtn.clicked += UpdateObjects;
        
        _objectList.itemsSource = _objects;
        _objectList.makeItem = () => new Label();
        _objectList.bindItem = (element, index) =>
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
            label.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == 1 && Browser.CanDisable(gObj.name))
                {
                    gObj.SetActive(!gObj.activeSelf);
                    _objectList.Rebuild();
                }
            });
        };
        
        // This event must be set for the items chosen one to work
        _objectList.selectedIndicesChanged += _ => { };
        _objectList.itemsChosen += enumerable => _browser.Refresh(enumerable.First() as GameObject);
        
        _browser.CurrentChangedEvent += UpdateObjects;
    }

    private void UpdateObjects()
    {
        UpdateObjects(_browser.Current);
    }

    private void UpdateObjects(GameObject? obj)
    {
        _curObjLabel.text = obj != null ? obj.name : "[Scene Root]";

        _objects.Clear();
        _objects.AddRange(obj is not null
            ? obj.transform.Cast<Transform>().Select(t => t.gameObject)
            : _scenes[_sceneChangeDropdown.value].GetRootGameObjects());

        _objectList.ClearSelection();
        _objectList.Rebuild();
        _objectList.RefreshItems();
        _objectList.SetEmptyText("(No Child Transforms)", "#777777");
    }

    private void GetScenes()
    {
        _scenes.Clear();
        
        int sceneCount = SceneManager.sceneCount;
        for (int i = 0; i < sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            _scenes.Add(scene.name, scene);
        }
        
        // Get the hidden DontDestroyOnLoad scene
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        
        object sceneObj = new Scene();
        Type type = typeof(Scene);
        FieldInfo? fieldInfo = type.GetField("m_Handle", flags);
        fieldInfo?.SetValue(sceneObj, -12);

        Scene dontDestroyScene = (Scene)sceneObj;
        _scenes.Add(dontDestroyScene.name, dontDestroyScene);
    }
}