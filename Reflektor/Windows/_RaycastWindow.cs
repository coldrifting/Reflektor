using System.Collections.Generic;
using System.Linq;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Reflektor.Windows;

public class RaycastWindow
{
    public readonly UIDocument Window;

    private readonly MainWindow _mainWindow;

    private readonly VisualElement _rootElement;

    private readonly List<GameObject> _rayHits = new();
    private readonly ListView _rayHitsList = new();
    
    public RaycastWindow(MainWindow window)
    {
        _mainWindow = window;
        
        _rootElement = Element.Root();

        _rayHitsList.itemsSource = _rayHits;
        _rayHitsList.makeItem = () => new Label();
        _rayHitsList.bindItem = (element, i) =>
        {
            if (element is not Label label)
            {
                return;
            }

            label.text = _rayHits[i].name;
        };
        _rayHitsList.selectionType = SelectionType.Single;
        _rayHitsList.selectedIndicesChanged += _ => { }; // Needs to be set
        _rayHitsList.itemsChosen += objects =>
        {
            _mainWindow.Cur = (GameObject)objects.First();
            _rootElement.SetVisible(false);
        };
        
        SetStyle();
        
        _rootElement.Add(_rayHitsList);
        _rootElement.SetVisible(false);

        Window = UitkForKsp2.API.Window.CreateFromElement(_rootElement, true, $"{Reflektor.ModName}_RayCastResults", _mainWindow.WindowParent.transform, true);
    }

    public void FireRay(bool scanUI = true)
    {
        if (scanUI)
        {
            PointerEventData pointerEventData = new PointerEventData(null);
            pointerEventData.position = Input.mousePosition;
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);

            _rayHits.Clear();
            foreach (var result in raycastResults)
            {
                _rayHits.Add(result.gameObject);
            }

            _rayHitsList.Rebuild();

            bool showWindow = _rayHits.Count != 0;
            _rootElement.SetVisible(showWindow);

            if (!_mainWindow.Root.IsVisible())
            {
                _mainWindow.Root.SetVisible(showWindow);
            }
        }
    }

    private void SetStyle()
    {
        _rootElement.style.flexGrow = 1;
        _rootElement.style.width = 300;
        _rootElement.style.height = 600;
        _rootElement.style.minHeight = 600;
        _rootElement.style.maxHeight = 600;
    }
}