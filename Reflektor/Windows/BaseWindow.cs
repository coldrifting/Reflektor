using SpaceWarp.API.Assets;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reflektor.Windows;

public class BaseWindow
{
    private const string BundlePath = $"{Reflektor.ModGuid}/reflektor_ui/ui";
    public UIDocument Window { get; }

    protected BaseWindow(GameObject parent, string name)
    {
        // Load Unity asset
        WindowOptions winOptions = new WindowOptions
        {
            WindowId = name.Replace("Window", ""),
            Parent = parent.transform,
            IsHidingEnabled = true,
            DisableGameInputForTextFields = true,
            MoveOptions = new MoveOptions()
            {
                CheckScreenBounds = false,
                IsMovingEnabled = true
            }
        };

        VisualTreeAsset? browserWindowUxml = AssetManager.GetAsset<VisualTreeAsset>($"{BundlePath}/{name}.uxml");

        Window = UitkForKsp2.API.Window.Create(winOptions, browserWindowUxml);

        //Window.rootVisualElement.RegisterCallback((MouseDownEvent _) => { Window.rootVisualElement.BringToFront(); });
    }
}