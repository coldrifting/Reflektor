using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using JetBrains.Annotations;
using Reflektor.Windows;
using SpaceWarp;
using SpaceWarp.API.Mods;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Reflektor;

[BepInPlugin(ModGuid, ModName, ModVer)]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public class Reflektor : BaseSpaceWarpPlugin
{
    [PublicAPI] public const string ModGuid = "Reflektor";
    [PublicAPI] public const string ModName = "Reflektor";
    [PublicAPI] public const string ModVer = "0.2.1.0";
    
    // Instance stuff
    public static Reflektor? Instance;

    private const string RootName = "_InspectorRoot";
    public static GameObject Root => GameObject.Find(RootName) ?? new GameObject(RootName);

    // Events
    public static event Action<SelectKey, bool>? PropertyChangedEvent;

    // Config
    private ConfigEntry<KeyCode>? _showHideToggleShortcutBrowser;
    private ConfigEntry<KeyCode>? _showHideToggleShortcutInspector;
    private ConfigEntry<KeyCode>? _raycastShortcut;

    private KeyboardShortcut? _raycastShortcutKey;
    private KeyboardShortcut? _showHideToggleKeyBrowser;
    private KeyboardShortcut? _showHideToggleKeyInspector;

    public override void OnInitialized()
    {
        base.OnInitialized();
        Log("Initializing");
        Instance = this;

        GameObject rootGameObject = new GameObject("_InspectorRoot");
        DontDestroyOnLoad(rootGameObject);
        
        SpaceWarp.API.Game.Messages.StateChanges.GameStateChanged += (_, _, _) => Utils.ResetSorting();
        SpaceWarp.API.Game.Messages.StateChanges.GameStateChanged += (_, _, _) => RefreshBrowser();
        
        _showHideToggleShortcutBrowser = Config.Bind(
            new ConfigDefinition("Settings", "Toggle Browser UI Key"),
            KeyCode.Q,
            new ConfigDescription("Toggle the browser UI with: this key + L SHIFT + L ALT"));
        
        _showHideToggleShortcutInspector = Config.Bind(
            new ConfigDefinition("Settings", "Toggle Inspector UI Key"),
            KeyCode.E,
            new ConfigDescription("Toggle the inspector UI with: this key + L SHIFT + L ALT"));

        _raycastShortcut = Config.Bind(
            "Settings",
            "Fire Raycast Key",
            KeyCode.R,
            "Fire a raycast with: this key + L SHIFT + L ALT");

        Config.SettingChanged += (_, _) => SetKeyboardShortcuts();
        Config.ConfigReloaded += (_, _) => SetKeyboardShortcuts();
        SetKeyboardShortcuts();
        
        Log("Initialized");
        
        // Add Default Tabs
        #if DEBUG
        Inspector.SwitchTab(new SelectKey(GameObject.Find("/GameManager")));

        GameObject g = new GameObject("[Testing]");
        DontDestroyOnLoad(g);
        ZZZ_TestClass t = g.AddComponent<ZZZ_TestClass>();
        if (t is not null)
        {
            Inspector.SwitchTab(new SelectKey(t));
        }
        #endif
    }

    private static void RefreshBrowser()
    {
        if (Instance == null)
        {
            return;
        }

        IEnumerator coroutine = RefreshBrowserAfterWait(3);
        Instance.StartCoroutine(coroutine);
    }

    private static IEnumerator RefreshBrowserAfterWait(int numFrames)
    {
        for (int i = 0; i < numFrames; i++)
        {
            yield return 0;
        }
        Browser.Refresh();
    }

    public void Update()
    {
        if (_showHideToggleKeyBrowser != null && _showHideToggleKeyBrowser.Value.IsDown())
        {
            Browser.ToggleDisplay();
        }
        
        if (_showHideToggleKeyInspector != null && _showHideToggleKeyInspector.Value.IsDown())
        {
            Inspector.ToggleDisplay();
        }

        if (_raycastShortcutKey != null && _raycastShortcutKey.Value.IsDown())
        {
            FireRay();
        }
    }

    public void FireRay()
    {
        PointerEventData pointerEventData = new(null);
        pointerEventData.position = Input.mousePosition;
        var raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);

        var objects = raycastResults.Select(result => result.gameObject).ToList();

        Browser.ShowRaycastResults(objects);
    }

    public static void FirePropertyChangedEvent(SelectKey key, bool triggeredManually = false)
    {
        if (Instance is null)
        {
            return;
        }

        Instance.StartCoroutine(FireEventAfterWait((selectKey, b) =>
        {
            GameObject? g = selectKey.Target.GetGameObject();
            if (g is not null)
            {
                PropertyChangedEvent?.Invoke(new SelectKey(g), b);
                foreach (Component c in g.GetComponents<Component>())
                {
                    PropertyChangedEvent?.Invoke(new SelectKey(c), b);
                }
            }
            else
            {
                PropertyChangedEvent?.Invoke(selectKey, b);
            }
        }, key, triggeredManually, 3));
    }

    private static IEnumerator FireEventAfterWait(Action<SelectKey, bool> eventAction, SelectKey obj, bool b, int numFrames)
    {
        for (int i = 0; i < numFrames; i++)
        {
            yield return 0;
        }
        
        eventAction.Invoke(obj, b);
    }

    public static void Inspect(SelectKey key)
    {
        if (Instance is not null)
        {
            Inspector.SwitchTab(key);
        }
    }

    private void SetKeyboardShortcuts()
    {
        if (_raycastShortcut != null)
        {
            _raycastShortcutKey = 
                new KeyboardShortcut(_raycastShortcut.Value, KeyCode.LeftShift, KeyCode.LeftAlt);
        }

        if (_showHideToggleShortcutBrowser != null)
        {
            _showHideToggleKeyBrowser =
                new KeyboardShortcut(_showHideToggleShortcutBrowser.Value, KeyCode.LeftShift, KeyCode.LeftAlt);
        }

        if (_showHideToggleShortcutInspector != null)
        {
            _showHideToggleKeyInspector =
                new KeyboardShortcut(_showHideToggleShortcutInspector.Value, KeyCode.LeftShift, KeyCode.LeftAlt);
        }
    }

    public static void Log(object? msg)
    {
        if (msg is IEnumerable<object> enumerable)
        {
            Log(enumerable);
        }
        StringBuilder sb = new();
        string list = sb.AppendJoin(", ", msg).ToString();
        Debug.Log($"<color=#00FF77>{ModName}: {list}</color>");
    }

    private static void Log(IEnumerable<object>? msg)
    {
        StringBuilder sb = new();
        string list = sb.AppendJoin(", ", msg).ToString();
        Debug.Log($"<color=#00FF77>{ModName}: {list}</color>");
    }
}