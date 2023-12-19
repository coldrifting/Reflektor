using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using JetBrains.Annotations;
using Reflektor.Windows;
using SpaceWarp;
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
    private static ConfigEntry<KeyboardShortcut>? _showHideToggleShortcut;
    private static ConfigEntry<KeyboardShortcut>? _raycastShortcut;

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
        
        var v1 = Window.Create(winOptions, _inspectorWindow);
        var v2 = Window.Create(winOptionsBrowser, _browserWindow);

        v1.rootVisualElement.style.position = Position.Absolute;
        v2.rootVisualElement.style.position = Position.Absolute;
        
        v1.rootVisualElement.RegisterCallback((MouseDownEvent evt) =>
        {
            v1.rootVisualElement.BringToFront();
        });
        
        v2.rootVisualElement.RegisterCallback((MouseDownEvent evt) =>
        {
            v2.rootVisualElement.BringToFront();
        });
        
        _showHideToggleShortcut = Config.Bind(
            "Settings",
            "ShowHideToggleKey",
            new KeyboardShortcut(KeyCode.U, KeyCode.LeftShift),
            "The key to toggle the inspector UI");

        
        _raycastShortcut = Config.Bind(
            "Settings",
            "RaycastKey",
            new KeyboardShortcut(KeyCode.R, KeyCode.LeftShift),
            "The key to raycast for an element");
        
        SpaceWarp.API.Game.Messages.StateChanges.GameStateChanged += (_, _, _) => Utils.ResetSorting();
        Log("Initialized");
    }

    public void Update()
    {
        if (_showHideToggleShortcut != null && _browserWindow is not null && _showHideToggleShortcut.Value.IsDown())
        {
            _browserWindow.ToggleDisplay();
        }

        if (_raycastShortcut != null && _raycastShortcut.Value.IsDown())
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