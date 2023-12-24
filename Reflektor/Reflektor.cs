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
    [PublicAPI] public const string ModVer = "0.1.0.0";

    private static Reflektor? _instance;

    // Events
    public static event Action<object>? PropertyChangedEvent;
    
    // Windows
    private Inspector? _inspector;
    private Browser? _browser;

    // Config
    private ConfigEntry<KeyCode>? _showHideToggleShortcut;
    private ConfigEntry<KeyCode>? _raycastShortcut;

    private KeyboardShortcut? _raycastShortcutKey;
    private KeyboardShortcut? _showHideToggleKey;

    public override void OnInitialized()
    {
        base.OnInitialized();
        Log("Initializing");
        _instance = this;

        GameObject rootGameObject = new GameObject("_InspectorRoot");
        DontDestroyOnLoad(rootGameObject);

        _inspector = new Inspector(rootGameObject);
        _browser = new Browser(rootGameObject, _inspector);
        
        SpaceWarp.API.Game.Messages.StateChanges.GameStateChanged += (_, _, _) => Utils.ResetSorting();
        SpaceWarp.API.Game.Messages.StateChanges.GameStateChanged += (_, _, _) => RefreshBrowser();
        
        _showHideToggleShortcut = Config.Bind(
            new ConfigDefinition("Settings", "Toggle Inspector UI Key"),
            KeyCode.U,
            new ConfigDescription("Toggle the inspector UI with this key + L SHIFT + L ALT"));

        _raycastShortcut = Config.Bind(
            "Settings",
            "Fire Raycast Key",
            KeyCode.R,
            "Fire a raycast with this key + L SHIFT + L ALT");

        Config.SettingChanged += (_, _) => SetKeyboardShortcuts();
        Config.ConfigReloaded += (_, _) => SetKeyboardShortcuts();
        SetKeyboardShortcuts();
        
        Log("Initialized");
    }

    private void RefreshBrowser()
    {
        if (_instance == null)
        {
            return;
        }

        var coroutine = RefreshBrowserAfterWait(3);
        _instance.StartCoroutine(coroutine);
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
        if (_showHideToggleKey != null && _showHideToggleKey.Value.IsDown())
        {
            _browser?.ToggleDisplay();
        }

        if (_raycastShortcutKey != null && _raycastShortcutKey.Value.IsDown())
        {
            FireRay();
        }
    }

    public void FireRay()
    {
        PointerEventData pointerEventData = new PointerEventData(null);
        pointerEventData.position = Input.mousePosition;
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);

        List<GameObject> objects = raycastResults.Select(result => result.gameObject).ToList();

        Log(objects);

        _browser?.ShowRaycastResults(objects);
    }

    public static void FirePropertyChangedEvent(object obj)
    {
        if (_instance is null)
        {
            return;
        }

        _instance.StartCoroutine(FirePropertyChangeEventAfterWait(obj, 3));
    }

    private static IEnumerator FirePropertyChangeEventAfterWait(object obj, int numFrames)
    {
        for (int i = 0; i < numFrames; i++)
        {
            yield return 0;
        }
        
        GameObject? g = Utils.GetGameObject(obj);
        if (g is not null)
        {
            PropertyChangedEvent?.Invoke(g);
            foreach (Component v in g.GetComponents<Component>())
            {
                PropertyChangedEvent?.Invoke(v);
            }
        }
        else
        {
            PropertyChangedEvent?.Invoke(obj);
        }
    }

    public static void Inspect(object? obj)
    {
        if (_instance is not null && obj is not null)
        {
            _instance._inspector?.SwitchTab(obj);
        }
    }

    private void SetKeyboardShortcuts()
    {
        if (_raycastShortcut != null)
        {
            _raycastShortcutKey = 
                new KeyboardShortcut(_raycastShortcut.Value, KeyCode.LeftShift, KeyCode.LeftAlt);
        }

        if (_showHideToggleShortcut != null)
        {
            _showHideToggleKey =
                new KeyboardShortcut(_showHideToggleShortcut.Value, KeyCode.LeftShift, KeyCode.LeftAlt);
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