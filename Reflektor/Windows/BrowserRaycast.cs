using System.Collections.Generic;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reflektor.Windows;

public class BrowserRaycast
{
    // GUI
    private readonly VisualElement _objectPane;
    private readonly VisualElement _raycastPane;
    private readonly Button _raycastBackBtn;
    private readonly ListView _raycastList;
    
    // Data
    private readonly Browser _browser;
    private readonly List<GameObject> _raycastObjects = new();

    public BrowserRaycast(Browser browser, VisualElement root)
    {
        _browser = browser;
        
        _objectPane = root.Q<VisualElement>(name: "ObjectPane");
        _raycastPane = root.Q<VisualElement>(name: "RaycastPane");
        _raycastBackBtn = root.Q<Button>(name: "RaycastBackBtn");
        _raycastList = root.Q<ListView>(name: "RaycastList");
    }

    public void Setup()
    {
        _raycastBackBtn.clicked += HideRaycastResults;
        _raycastList.itemsSource = _raycastObjects;
        _raycastList.makeItem = () => new Label();
        _raycastList.bindItem = (element, i) =>
        {
            if (element is not Label label)
            {
                return;
            }

            label.text = _raycastObjects[i].name.ToString();
            label.RegisterCallback((MouseDownEvent _) =>
            {
                HideRaycastResults();
                _browser.Current = _raycastObjects[i];
            });
        };
    }

    public void ShowRaycastResults(IEnumerable<GameObject> objects)
    {
        //_path.SetEnabled(false);
        _raycastPane.Show();
        _objectPane.Hide();
        
        _raycastObjects.Clear();
        _raycastObjects.AddRange(objects);

        _raycastList.Rebuild();
        Utils.SetListViewEmptyText(_raycastList, "(No Objects Found)", "#FF7700");
    }

    public void HideRaycastResults()
    {
        //_path.SetEnabled(true);
        _raycastPane.Hide();
        _objectPane.Show();
        
        _browser.Refresh();
    }
}