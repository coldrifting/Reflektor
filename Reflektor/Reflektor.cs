using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using JetBrains.Annotations;
using Reflektor.Windows;
using SpaceWarp;
using SpaceWarp.API.Configuration;
using SpaceWarp.API.Mods;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Reflektor;

[BepInPlugin(ModGuid, ModName, ModVer)]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public class Reflektor : BaseSpaceWarpPlugin
{
    [PublicAPI] public const string ModGuid = "Reflektor";
    [PublicAPI] public const string ModName = "Reflektor";
    [PublicAPI] public const string ModVer = "0.1.0.0";

    private static Reflektor? _instance;
    
    // Windows
    private Inspector? _inspectorWindow;
    private Browser? _browserWindow;

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

        GameObject rootGameObject = new GameObject("_Inspector");
        DontDestroyOnLoad(rootGameObject);
        
        GameObject rootGameObjectBrowser = new GameObject("_Inspector_Browser");
        DontDestroyOnLoad(rootGameObjectBrowser);

        WindowOptions winOptions = new WindowOptions
        {
            WindowId = rootGameObject.name,
            Parent = rootGameObject.transform,
            IsHidingEnabled = true,
            DisableGameInputForTextFields = true,
            MoveOptions = new MoveOptions()
            {
                CheckScreenBounds = false,
                IsMovingEnabled = true
            }
        };
        WindowOptions winOptionsBrowser = winOptions;
        winOptionsBrowser.WindowId += "_Browser";
        winOptionsBrowser.Parent = rootGameObjectBrowser.transform;

        _inspectorWindow = new Inspector();
        _browserWindow = new Browser(_inspectorWindow);
        
        var inspectWindow = Window.Create(winOptions, _inspectorWindow);
        var browseWindow = Window.Create(winOptionsBrowser, _browserWindow);
        
        inspectWindow.rootVisualElement.RegisterCallback((MouseDownEvent _) =>
        {
            inspectWindow.rootVisualElement.BringToFront();
        });
        
        browseWindow.rootVisualElement.RegisterCallback((MouseDownEvent _) =>
        {
            browseWindow.rootVisualElement.BringToFront();
        });
        
        SpaceWarp.API.Game.Messages.StateChanges.GameStateChanged += (_, _, _) => Utils.ResetSorting();
        Log("Initialized");
        
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
    }

    public void Update()
    {
        if (_showHideToggleKey != null && _showHideToggleKey.Value.IsDown() && _browserWindow is not null)
        {
            _browserWindow.ToggleDisplay();
        }

        if (_raycastShortcutKey != null && _raycastShortcutKey.Value.IsDown())
        {
            FireRay();
        }
    }

    public void FireRay()
    {
        if (_browserWindow is null)
        {
            return;
        }

        PointerEventData pointerEventData = new PointerEventData(null);
        pointerEventData.position = Input.mousePosition;
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);

        List<GameObject> objects = raycastResults.Select(result => result.gameObject).ToList();

        Log(objects);

        _browserWindow.Show();
        _browserWindow.ShowRaycastResults(objects);
    }

    public static void Inspect(object? obj)
    {
        if (_instance is not null && obj is not null)
        {
            _instance._inspectorWindow?.SwitchTab(obj);
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