using System.Collections.Generic;
using System.Linq;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Toggle = UnityEngine.UIElements.Toggle;

namespace Reflektor.Windows;

public class BrowserValuePane : VisualElement
{
    // GUI Elements
    private readonly Toggle _curActiveToggle = new("Active");
    private readonly List<TextField> _inputs = new();
    private readonly ListView _componentList = new();
    
    // Data
    private GameObject? Current { get; set; }
    
    private List<Component> Components { get; } = new();
    private RectTransform? RectTransform { get; set; }
    private CanvasScaler? CanvasScaler { get; set; }

    private bool _useAbsolutePosition;
    
    // Constants
    private const string LabelName = "Name";
    private const string LabelPos = "Pos";
    private const string LabelScale = "Scale";
    private const string LabelSize = "Size";
    private const string LabelAnchor = "Anchor";
    
    public BrowserValuePane(Browser browser, Inspector inspector)
    {
        AddTextField(LabelName);
        AddTextField(LabelPos);
        AddTextField(LabelScale);
        AddTextField(LabelSize);
        AddTextField(LabelAnchor);

        _curActiveToggle.SetEnabled(false);
        _curActiveToggle.AddToClassList("toggle");
        _curActiveToggle.RegisterValueChangedCallback(evt =>
        {
            if (Current is not null && !browser.DisallowDisableList.Contains(Current.name))
            {
                Current.gameObject.SetActive(evt.newValue);
            }
        });
        Add(_curActiveToggle);
        
        _componentList.itemsSource = Components;
        _componentList.makeItem = () =>
        {
            Label l = new();
            return l;
        };
        _componentList.bindItem = (element, index) =>
        {
            Component? comp = Components[index];
            if (element is not Label label)
            {
                return;
            }

            label.text = comp.GetType().Name;
            label.style.color = new StyleColor(new Color(0.05f, 0.65f, 0.45f));
        };
        
        // First event needs to be set for the second one to work
        _componentList.selectedIndicesChanged += _ => { };
        _componentList.itemsChosen += enumerable =>
        {
            object? obj = enumerable.First();
            if (obj is null)
            {
                return;
            }
            
            inspector.SwitchTab(obj);
        };
        // View game object on right click of component list
        _componentList.RegisterCallback((MouseDownEvent evt) =>
        {
            if (evt.button != 1)
            {
                return;
            }

            evt.PreventDefault();
            if (Current is not null)
            {
                inspector.SwitchTab(Current.gameObject);
            }
        });
        
        Add(_componentList);
        
        // Setup callbacks
        browser.CurrentChangedEvent += obj =>
        {
            Current = obj;
            
            Components.Clear();
            RectTransform = null;
            CanvasScaler = null;
            
            List<Component> comps = Current != null ? Current.GetComponents<Component>().ToList() : new List<Component>();
            foreach (Component c in comps)
            {
                switch (c)
                {
                    case RectTransform cRect:
                        RectTransform = cRect;
                        break;
                    case CanvasScaler cs:
                        CanvasScaler = cs;
                        break;
                }

                Components.Add(c);
            }
            
            _componentList.ClearSelection();
            
            _componentList.Rebuild();
            Utils.SetListViewEmptyText(_componentList, "<color=#777777>(No Object Selected)</color>");

            foreach (TextField? input in _inputs)
            {
                GetChanges(input);
            }
            
            _curActiveToggle.SetEnabled(Current);
            _curActiveToggle.SetValueWithoutNotify(Current != null && Current.gameObject.activeSelf);
        };
        
        SetStyle();
    }

    private void AddTextField(string textFieldName)
    {
        TextField textField = new TextField();
        textField.name = textFieldName;
        textField.label = textFieldName;
        textField.labelElement.style.width = 55;
        textField.labelElement.style.minWidth = 55;
        textField.labelElement.style.maxWidth = 55;
        textField.style.borderTopLeftRadius = 10;
        textField.style.borderTopRightRadius = 10;
        textField.style.borderBottomLeftRadius = 10;
        textField.style.borderBottomRightRadius = 10;
        textField.style.marginLeft = 12;
        textField.style.marginRight = 12;
        textField.isDelayed = true;

        textField.RegisterValueChangedCallback(_ => SetValues(textField));

        // Special cases
        switch (textFieldName)
        {
            case LabelPos:
                textField.labelElement.RegisterCallback((MouseDownEvent evt) =>
                {
                    if (evt.button != 1)
                    {
                        return;
                    }

                    _useAbsolutePosition = !_useAbsolutePosition;
                    textField.labelElement.text = _useAbsolutePosition ? LabelPos : LabelPos + " [A]";
                    textField.SetValueWithoutNotify("");
                    SetValues(textField);
                });
                break;
            case LabelSize or LabelAnchor:
                // Hide conditional fields on startup
                textField.Hide();
                break;
        }

        _inputs.Add(textField);
        Add(textField);
    }
    
    private void SetValues(TextField textField)
    {
        if (Current is null)
        {
            return;
        }

        bool valid = Utils.TryParse(textField.value, out Vector3 vec);
        switch (textField.name)
        {
            case LabelName:
                Current.name = textField.value;
                break;
            case LabelPos:
                if (valid)
                {
                    if (_useAbsolutePosition)
                    {
                        Current.transform.position = vec;
                    }
                    else
                    {
                        Current.transform.localPosition = vec;
                    }
                }

                break;
            case LabelScale:
                if (valid)
                {
                    Current.transform.localScale = vec;
                }

                break;
            case LabelSize:
                if (CanvasScaler is not null && valid)
                {
                    CanvasScaler.referenceResolution = vec;
                }
                else if (RectTransform is not null && valid)
                {
                    RectTransform.sizeDelta = vec;
                }

                break;
            case LabelAnchor:
                if (RectTransform is not null && valid)
                {
                    RectTransform.anchoredPosition = vec;
                }

                break;
        }

        GetChanges(textField);
    }
    
    private void GetChanges(TextField textField)
    {
        if (Current is null)
        {
            textField.SetEnabled(false);
            textField.SetValueWithoutNotify("");
            return;
        }

        textField.SetEnabled(true);
        switch (textField.name)
        {
            case LabelName:
                textField.SetValueWithoutNotify(Current.name);
                break;
            case LabelPos:
                textField.SetValueWithoutNotify(_useAbsolutePosition
                    ? Utils.ToSimpleString(Current.transform.position)
                    : Utils.ToSimpleString(Current.transform.localPosition));

                break;
            case LabelScale:
                textField.SetValueWithoutNotify(Utils.ToSimpleString(Current.transform.localScale));
                break;
            case LabelSize:
                if (RectTransform is not null)
                {
                    textField.Show();
                    textField.SetValueWithoutNotify(
                        Utils.ToSimpleString(
                        CanvasScaler is not null
                            ? CanvasScaler.referenceResolution
                            : RectTransform.sizeDelta));
                }
                else
                {
                    textField.Hide();
                }

                break;
            case LabelAnchor:
                if (RectTransform is not null)
                {
                    textField.Show();
                    textField.SetValueWithoutNotify(Utils.ToSimpleString(RectTransform.anchoredPosition));
                }
                else
                {
                    textField.Hide();
                }

                break;
        }
    }

    private void SetStyle()
    {
        style.width = Length.Percent(100);
        
        style.paddingTop = 0;
        style.paddingBottom = 0;
        style.paddingLeft = 6;
        style.paddingRight = 0;

        style.marginTop = 0;
        style.marginBottom = 0;
        style.marginLeft = 0;
        style.marginRight = 0;

        _curActiveToggle.style.marginLeft = 6;
        _curActiveToggle.style.marginRight = 0;
        _curActiveToggle.style.marginTop = 6;
        _curActiveToggle.style.marginBottom = 12;

        _curActiveToggle.labelElement.style.width = 55;
        _curActiveToggle.labelElement.style.minWidth = 55;
        _curActiveToggle.labelElement.style.maxWidth = 55;

        _componentList.style.flexGrow = 1;
        _componentList.style.backgroundColor = Reflektor.ColorFromHex(0x192128);
        _componentList.style.borderTopLeftRadius = 6;
        _componentList.style.borderTopRightRadius = 6;
        _componentList.style.borderBottomLeftRadius = 6;
        _componentList.style.borderBottomRightRadius = 6;

        foreach (TextField textField in _inputs)
        {
            textField.labelElement.style.minWidth = 75;
            textField.style.marginLeft = 6;
            textField.style.marginRight = 0;
            textField.ElementAt(1).style.borderTopLeftRadius = 3;
            textField.ElementAt(1).style.borderTopRightRadius = 3;
            textField.ElementAt(1).style.borderBottomLeftRadius = 3;
            textField.ElementAt(1).style.borderBottomRightRadius = 3;
        }
    }
}