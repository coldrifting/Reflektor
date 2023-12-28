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
    [PublicAPI] public const string ModVer = "0.2.0.0";

    public static Reflektor? Instance;

    // Events
    public static event Action<object, bool>? PropertyChangedEvent;
    
    // Windows
    private Inspector? _inspector;
    private Browser? _browser;

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

        GameObject rootGameObject = new("_InspectorRoot");
        DontDestroyOnLoad(rootGameObject);

        _browser = new Browser(rootGameObject);
        _inspector = new Inspector(rootGameObject);
        
        
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
    }

    private void RefreshBrowser()
    {
        if (Instance == null)
        {
            return;
        }

        IEnumerator coroutine = RefreshBrowserAfterWait(3);
        Instance.StartCoroutine(coroutine);
    }

    private IEnumerator RefreshBrowserAfterWait(int numFrames)
    {
        for (int i = 0; i < numFrames; i++)
        {
            yield return 0;
        }
        _browser?.Refresh();
    }

    public void Update()
    {
        if (_showHideToggleKeyBrowser != null && _showHideToggleKeyBrowser.Value.IsDown())
        {
            _browser?.ToggleDisplay();
        }
        
        if (_showHideToggleKeyInspector != null && _showHideToggleKeyInspector.Value.IsDown())
        {
            _inspector?.ToggleDisplay();
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

        _browser?.ShowRaycastResults(objects);
    }

    public static void FirePropertyChangedEvent(object obj, bool triggeredManually = false)
    {
        if (Instance is null)
        {
            return;
        }

        Instance.StartCoroutine(FireEventAfterWait((o, b) =>
        {
            GameObject? g = o.GetGameObject();
            if (g is not null)
            {
                PropertyChangedEvent?.Invoke(g, b);
                foreach (Component c in g.GetComponents<Component>())
                {
                    PropertyChangedEvent?.Invoke(c, b);
                }
            }
            else
            {
                PropertyChangedEvent?.Invoke(o, b);
            }
        }, obj, triggeredManually, 3));
    }

    private static IEnumerator FireEventAfterWait(Action<object, bool> eventAction, object obj, bool b, int numFrames)
    {
        for (int i = 0; i < numFrames; i++)
        {
            yield return 0;
        }
        
        eventAction.Invoke(obj, b);
    }

    public static void Inspect(object? obj)
    {
        if (Instance is not null && obj is not null)
        {
            if (Instance._inspector != null)
            {
                Instance._inspector.SwitchTab(obj);
                Instance._inspector.Window.rootVisualElement.BringToFront();
                Reflektor.Log("TESTING");
            }
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
        Debug.Log($"{ModName}: {list}");
    }

    private static void Log(IEnumerable<object>? msg)
    {
        StringBuilder sb = new();
        string list = sb.AppendJoin(", ", msg).ToString();
        Debug.Log($"{ModName}: {list}");
    }

    public static Color ColorFromHex(uint hexNum)
    {
        if (hexNum > 0xFFFFFF)
        {
            return Color.white;
        }
        
        uint b = hexNum & 0xff;
        uint g = (hexNum>>8) & 0xff;
        uint r = (hexNum>>16) & 0xff;

        return new Color(r / 255.0f, g / 255.0f, b / 255.0f);
    }
}