using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using JetBrains.Annotations;
using Reflektor.Extensions;
using SpaceWarp;
using SpaceWarp.API.Mods;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reflektor;

[BepInPlugin(ModGuid, ModName, ModVer)]
[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
public class Reflektor : BaseSpaceWarpPlugin
{
    [PublicAPI] public const string ModGuid = "Reflektor";
    [PublicAPI] public const string ModName = "Reflektor";
    [PublicAPI] public const string ModVer = "0.1.0.0";

    public static Reflektor Instance = null!;

    private UIDocument? _mainWindow;

    private static ConfigEntry<KeyboardShortcut> _showHideToggleShortcut = null!;
    //private static ConfigEntry<KeyboardShortcut> _raycastShortcut = null!;

    public override void OnInitialized()
    {
        base.OnInitialized();

        Instance = this;
        Log("Initialized");

        GameObject p = new GameObject("_Inspector");
        DontDestroyOnLoad(p);
        
        _mainWindow = Window.CreateFromElement(new WindowTitle(), true, "Reflektor_MainWindow", p.transform);

        _showHideToggleShortcut = Config.Bind(
            "Settings",
            "ShowHideToggleKey",
            new KeyboardShortcut(KeyCode.U, KeyCode.LeftShift),
            "The key to toggle the inspector UI");

        /*
        _raycastShortcut = Config.Bind(
            "Settings",
            "RaycastKey",
            new KeyboardShortcut(KeyCode.R, KeyCode.LeftShift),
            "The key to raycast for an element");
            */
        
        SpaceWarp.API.Game.Messages.StateChanges.GameStateChanged += (_, _, _) => SortOrderHelper.ResetSorting();
        Log("Done Initializing");
    }

    public void Update()
    {
        if (_mainWindow is not null && _showHideToggleShortcut.Value.IsDown())
        {
            _mainWindow.ToggleVisible();
        }

        /*
        if (_raycastWindow is not null && _raycastShortcut.Value.IsDown())
        {
            _raycastWindow.FireRay();
        }

        if (new KeyboardShortcut(KeyCode.A, KeyCode.LeftShift).IsDown())
        {
            
        }
        */
    }

    public static void Log(object? msg)
    {
        if (msg is null)
        {
            return;
        }

        Debug.Log($"<color=#00FF77>{Reflektor.ModName}: {msg}</color>");
    }
}