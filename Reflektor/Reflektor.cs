using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using JetBrains.Annotations;
using Reflektor.Windows;
using SpaceWarp;
using SpaceWarp.API.Mods;
using UnityEngine;
using UnityEngine.UIElements;

using static Reflektor.Extensions;

namespace Reflektor;

[BepInPlugin(ModGuid, ModName, ModVer)]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public class Reflektor : BaseSpaceWarpPlugin
{
    [PublicAPI] public const string ModGuid = "Reflektor";
    [PublicAPI] public const string ModName = "Reflektor";
    [PublicAPI] public const string ModVer = "0.1.0.0";

    public static Reflektor Instance = null!;
    
    private UIDocument _window = null!;

    private RaycastWindow _raycastWindow = null!;

    private static ConfigEntry<KeyboardShortcut> _showHideToggleShortcut = null!;
    private static ConfigEntry<KeyboardShortcut> _raycastShortcut = null!;

    private readonly Dictionary<string, int> _canvasStorage = new();

    public override void OnInitialized()
    {
        base.OnInitialized();

        Instance = this;
        Log("Initialized");

        WindowTitle t = new();
        
        MainWindow mainWindow = new();
        _raycastWindow = new RaycastWindow(mainWindow);

        _window = mainWindow.Window;

        _window.rootVisualElement.SetVisible(false);

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
        
        SpaceWarp.API.Game.Messages.StateChanges.GameStateChanged += (_, _, _) => ResetSorting();
        Log("Done Initializing");
    }

    // Make sure the inspector always shows up on top of pause menus, etc
    private void ResetSorting()
    {
        var canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var canvas in canvases)
        {
            if (_canvasStorage.TryGetValue(canvas.gameObject.GetPath(), out int output))
            {
                canvas.sortingOrder = output - 1000;
            }
            else
            {
                _canvasStorage[canvas.gameObject.GetPath()] = canvas.sortingOrder;
                canvas.sortingOrder -= 1000;
            }
        }
    }

    public void Update()
    {
        if (_showHideToggleShortcut.Value.IsDown())
        {
            _window.rootVisualElement.ToggleVisible();
        }

        if (_raycastShortcut.Value.IsDown())
        {
            _raycastWindow.FireRay();
        }
    }
}