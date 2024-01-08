using Reflektor.Controls;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Toggle = UnityEngine.UIElements.Toggle;

namespace Reflektor.Windows;

public class BrowserValues
{
    // Constants
    private const string LabelName = "NameInput";
    private const string LabelPos = "PosInput";
    private const string LabelScale = "ScaleInput";
    private const string LabelSize = "SizeInput";
    private const string LabelAnchor = "AnchorInput";
    
    // Data
    private bool _useAbsolutePosition;
    private List<Component> Components { get; } = new();
    private RectTransform? RectTransform { get; set; }
    private CanvasScaler? CanvasScaler { get; set; }
    
    // GUI
    private readonly List<TextField> _inputs;
    private readonly Toggle _activeToggle;
    private readonly ListView _componentList;

    public BrowserValues(VisualElement root)
    {
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
                            (_useAbsolutePosition ? LabelPos + " [A]" : LabelPos).Replace("Input", "");
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
            if (Browser.Current is not null && Browser.CanDisable(Browser.Current.name))
            {
                Browser.Current.gameObject.SetActive(evt.newValue);
            }

            Refresh();
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
            if (obj is not null)
            {
                Inspector.SwitchTab(new SelectKey(obj));
            }
        };
        
        // View game object on right click of component list
        _componentList.RegisterCallback<MouseDownEvent>(evt =>
        {
            if (evt.button != 1)
            {
                return;
            }

            if (Browser.Current is not null)
            {
                Inspector.SwitchTab(new SelectKey(Browser.Current.gameObject));
            }
        });
        
        // Setup callbacks
        Browser.CurrentChangedEvent += obj =>
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
        
        Reflektor.PropertyChangedEvent += (key, _) =>
        {
            if (key.Target is not GameObject propObjG || propObjG != Browser.Current)
            {
                return;
            }

            foreach (TextField textField in _inputs)
            {
                GetChanges(textField);
            }
            
            _activeToggle.SetValueWithoutNotify(Browser.Current.activeSelf);
        };
    }

    private void SetValues(TextField textField)
    {
        if (Browser.Current is null)
        {
            return;
        }
        
        bool valid = TryParse(textField.value, out Vector3 vec);
        switch (textField.name)
        {
            case LabelName:
                Browser.Current.name = textField.value;
                break;
            case LabelPos:
                if (valid)
                {
                    if (_useAbsolutePosition)
                    {
                        Browser.Current.transform.position = vec;
                    }
                    else
                    {
                        Browser.Current.transform.localPosition = vec;
                    }
                }

                break;
            case LabelScale:
                if (valid)
                {
                    Browser.Current.transform.localScale = vec;
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

        Refresh();
    }
    
    private void GetChanges(TextField textField)
    {
        if (Browser.Current is null)
        {
            textField.SetEnabled(false);
            textField.SetValueWithoutNotify("");
            return;
        }

        textField.SetEnabled(true);
        switch (textField.name)
        {
            case LabelName:
                textField.SetValueWithoutNotify(Browser.Current.name);
                break;
            case LabelPos:
                textField.SetValueWithoutNotify(_useAbsolutePosition
                    ? ToSimpleString(Browser.Current.transform.position)
                    : ToSimpleString(Browser.Current.transform.localPosition));

                break;
            case LabelScale:
                textField.SetValueWithoutNotify(ToSimpleString(Browser.Current.transform.localScale));
                break;
            case LabelSize:
                if (RectTransform is not null)
                {
                    textField.Show();
                    textField.SetValueWithoutNotify(
                        ToSimpleString(
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
                    textField.SetValueWithoutNotify(ToSimpleString(RectTransform.anchoredPosition));
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

    private static bool TryParse(string input, out Vector3 output)
    {
        if (InputText<int>.TryParseVecGeneric(input, 3, 0, out Vector4 vec4))
        {
            output = vec4;
            return true;
        }
        
        output = Vector3.zero;
        return true;
    }

    private static string ToSimpleString(Vector2 vec)
    {
        return $"{vec.x} {vec.y}";
    }

    private static string ToSimpleString(Vector3 vec)
    {
        return $"{vec.x} {vec.y} {vec.z}";
    }

    private void Refresh()
    {
        if (Browser.Current != null)
        {
            Reflektor.FirePropertyChangedEvent(new SelectKey(Browser.Current));
        }
    }
}