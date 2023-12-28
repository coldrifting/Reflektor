using System.Collections.Generic;
using System.Linq;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Toggle = UnityEngine.UIElements.Toggle;

namespace Reflektor.Windows;

public class BrowserValues
{
    // GUI
    private readonly List<TextField> _inputs;
    private readonly Toggle _activeToggle;
    private readonly ListView _componentList;
    
    // Data
    private readonly Browser _browser;
    private readonly Inspector _inspector;
    
    private bool _useAbsolutePosition;
    private List<Component> Components { get; } = new();
    private RectTransform? RectTransform { get; set; }
    private CanvasScaler? CanvasScaler { get; set; }
    
    // Constants
    private const string LabelName = "NameInput";
    private const string LabelPos = "PosInput";
    private const string LabelScale = "ScaleInput";
    private const string LabelSize = "SizeInput";
    private const string LabelAnchor = "AnchorInput";

    public BrowserValues(Browser browser, VisualElement root, Inspector inspector)
    {
        _browser = browser;
        _inspector = inspector;

        TextField? nameInput = root.Q<TextField>(name: "NameInput");
        TextField? posInput = root.Q<TextField>(name: "PosInput");
        TextField? scaleInput = root.Q<TextField>(name: "ScaleInput");
        TextField? sizeInput = root.Q<TextField>(name: "SizeInput");
        TextField? anchorInput = root.Q<TextField>(name: "AnchorInput");
        _inputs = new List<TextField> { nameInput, posInput, scaleInput, sizeInput, anchorInput };
        
        _activeToggle = root.Q<Toggle>(name: "ActiveToggle");
        _componentList = root.Q<ListView>(name: "ComponentList");
    }
    
    public void Setup()
    {
        foreach (TextField textField in _inputs)
        {
            textField.RegisterValueChangedCallback(_ => SetValues(textField));

            // Special cases
            switch (textField.name)
            {
                case LabelPos:
                    textField.labelElement.RegisterCallback<MouseDownEvent>(evt =>
                    {
                        if (evt.button != 1)
                        {
                            return;
                        }

                        _useAbsolutePosition = !_useAbsolutePosition;
                        textField.labelElement.text =
                            (_useAbsolutePosition ? LabelPos : LabelPos + " [A]").Replace("Input", "");
                        textField.SetValueWithoutNotify("");
                        SetValues(textField);
                    });
                    break;
                case LabelSize or LabelAnchor:
                    textField.Hide();
                    break;
            }
        }
        
        _activeToggle.SetEnabled(false);
        _activeToggle.AddToClassList("toggle");
        _activeToggle.RegisterValueChangedCallback(evt =>
        {
            if (_browser.Current is null || !Browser.CanDisable(_browser.Current.name))
            {
                return;
            }

            _browser.Current.gameObject.SetActive(evt.newValue);
            Reflektor.FirePropertyChangedEvent(_browser.Current);
        });
        
        _componentList.itemsSource = Components;
        _componentList.makeItem = () => new Label();
        _componentList.bindItem = (element, index) =>
        {
            Component? comp = Components[index];
            if (element is Label label)
            {
                label.text = comp.GetType().Name;
            }
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
            
            _inspector.SwitchTab(obj);
        };
        
        // View game object on right click of component list
        _componentList.RegisterCallback<MouseDownEvent>(evt =>
        {
            if (evt.button != 1)
            {
                return;
            }

            if (_browser.Current is not null)
            {
                _inspector.SwitchTab(_browser.Current.gameObject);
            }
        });
        
        // Setup callbacks
        _browser.CurrentChangedEvent += obj =>
        {
            Components.Clear();
            RectTransform = null;
            CanvasScaler = null;
            
            List<Component> comps = obj != null ? obj.GetComponents<Component>().ToList() : new List<Component>();
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
            _componentList.SetEmptyText("(No Object Selected)", "#777777");

            foreach (TextField? input in _inputs)
            {
                GetChanges(input);
            }
            
            _activeToggle.SetEnabled(obj);
            _activeToggle.SetValueWithoutNotify(obj != null && obj.gameObject.activeSelf);
        };
        
        Reflektor.PropertyChangedEvent += (propObj, _) =>
        {
            if (propObj is not GameObject propObjG || propObjG != _browser.Current)
            {
                return;
            }

            foreach (TextField textField in _inputs)
            {
                GetChanges(textField);
            }
            
            _activeToggle.SetValueWithoutNotify(_browser.Current.activeSelf);
        };
    }

    private void SetValues(TextField textField)
    {
        if (_browser.Current is null)
        {
            return;
        }
        
        bool valid = Parsing.TryParse(textField.value, out Vector3 vec);
        switch (textField.name)
        {
            case LabelName:
                _browser.Current.name = textField.value;
                break;
            case LabelPos:
                if (valid)
                {
                    if (_useAbsolutePosition)
                    {
                        _browser.Current.transform.position = vec;
                    }
                    else
                    {
                        _browser.Current.transform.localPosition = vec;
                    }
                }

                break;
            case LabelScale:
                if (valid)
                {
                    _browser.Current.transform.localScale = vec;
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

        Reflektor.FirePropertyChangedEvent(_browser.Current);
    }
    
    private void GetChanges(TextField textField)
    {
        if (_browser.Current is null)
        {
            textField.SetEnabled(false);
            textField.SetValueWithoutNotify("");
            return;
        }

        textField.SetEnabled(true);
        switch (textField.name)
        {
            case LabelName:
                textField.SetValueWithoutNotify(_browser.Current.name);
                break;
            case LabelPos:
                textField.SetValueWithoutNotify(_useAbsolutePosition
                    ? Parsing.ToSimpleString(_browser.Current.transform.position)
                    : Parsing.ToSimpleString(_browser.Current.transform.localPosition));

                break;
            case LabelScale:
                textField.SetValueWithoutNotify(Parsing.ToSimpleString(_browser.Current.transform.localScale));
                break;
            case LabelSize:
                if (RectTransform is not null)
                {
                    textField.Show();
                    textField.SetValueWithoutNotify(
                        Parsing.ToSimpleString(
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
                    textField.SetValueWithoutNotify(Parsing.ToSimpleString(RectTransform.anchoredPosition));
                }
                else
                {
                    textField.Hide();
                }

                break;
        }
    }

    public void Disable()
    {
        foreach (var t in _inputs)
        {
            t.SetEnabled(false);
        }

        _activeToggle.SetEnabled(false);
        _componentList.SetEnabled(false);
    }

    public void Enable()
    {
        _componentList.SetEnabled(true);
    }
}