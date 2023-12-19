using System.Collections.Generic;
using System.Linq;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reflektor.Windows;

public class BrowserRaycastPane : VisualElement
{
    // GUI Elements
    private readonly GroupBox _header = new();
    private readonly Label _label = new("Raycast Results");
    private readonly Button _backBtn = new();
    private readonly ListView _rayHitsList = new();
    
    // Data
    private readonly List<GameObject> _rayHits = new();
    
    public BrowserRaycastPane(Browser window)
    {
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
        
        // First event needs to be set for the second one to work
        _rayHitsList.selectedIndicesChanged += _ => { };
        _rayHitsList.itemsChosen += objects =>
        {
            window.Current = (GameObject)objects.First();
            window.HideRaycastResults();
        };
        
        _backBtn.text = "Back";
        _backBtn.clicked += window.HideRaycastResults;
        
        _header.Add(_label);
        _header.Add(_backBtn);

        Add(_header);
        Add(_rayHitsList);
        SetStyle();
        
        this.Hide();
    }

    public void UpdateRaycastList(IEnumerable<GameObject> gameObjects)
    {
        _rayHits.Clear();
        _rayHits.AddRange(gameObjects);
        
        _rayHitsList.Rebuild();
        Utils.SetListViewEmptyText(_rayHitsList, "<color=#FF7700>(No Raycast Results)</color>");
    }

    private void SetStyle()
    {
        _backBtn.style.marginLeft = 12;
        
        _header.style.flexDirection = FlexDirection.Row;
        _header.style.justifyContent = Justify.SpaceBetween;
        _header.style.marginTop = 12;
        _header.style.marginBottom = 12;

        _label.style.color = Reflektor.ColorFromHex(0x00FF77);
        
        style.paddingTop = 0;
        style.paddingBottom = 0;
        style.paddingLeft = 0;
        style.paddingRight = 0;

        style.marginTop = 0;
        style.marginBottom = 0;
        style.marginLeft = 0;
        style.marginRight = 0;
        
        style.width = 300;
        style.minWidth = 300;
        style.maxWidth = 300;
        
        _rayHitsList.style.flexGrow = 1;
        _rayHitsList.style.backgroundColor = Reflektor.ColorFromHex(0x192128);
        _rayHitsList.style.borderTopLeftRadius = 6;
        _rayHitsList.style.borderTopRightRadius = 6;
        _rayHitsList.style.borderBottomLeftRadius = 6;
        _rayHitsList.style.borderBottomRightRadius = 6;
    }
}