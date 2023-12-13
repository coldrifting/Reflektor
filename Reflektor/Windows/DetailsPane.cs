using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Component = UnityEngine.Component;

namespace Reflektor.Windows;

public class DetailsPane
{
    // GUI elements
    public VisualElement RootElement { get; }
    
    private readonly MainWindow _window;
    private readonly Toggle _curActiveToggle;

    private readonly ListView _componentList = new();
    
    // Data
    private const string LabelName = "Name";
    private const string LabelPos = "Pos";
    private const string LabelScale = "Scale";
    private const string LabelSize = "Size";
    private const string LabelAnchor = "Anchor";

    public DetailsPane(MainWindow window)
    {
        _window = window;
        
        RootElement = new VisualElement();
 
        AddTextField(LabelName);
        AddTextField(LabelPos);
        AddTextField(LabelScale);
        AddTextField(LabelSize);
        AddTextField(LabelAnchor);

        _curActiveToggle = new Toggle("Active");
        _curActiveToggle.SetEnabled(false);
        _curActiveToggle.RegisterValueChangedCallback(evt =>
        {
            if (_window.Cur is not null)
            {
                _window.Cur.gameObject.SetActive(evt.newValue);
            }

            _window.UpdateCurrent();
        });
        _window.SelectedObjectChangedEvent += () =>
        {
            _curActiveToggle.SetEnabled(_window.Cur is not null);
            _curActiveToggle.SetValueWithoutNotify(_window.Cur is not null && _window.Cur.gameObject.activeSelf);
        };
        RootElement.Add(_curActiveToggle);
        
        _componentList.itemsSource = _window.Components;
        _componentList.makeItem = () =>
        {
            Label l = new();
            return l;
        };
        _componentList.bindItem = (element, index) =>
        {
            Component? comp = _window.Components[index];
            if (element is not Label label)
            {
                return;
            }

            label.text = comp.GetType().Name;
            label.style.color = new StyleColor(new Color(0.05f, 0.65f, 0.45f));
        };
        
        _componentList.selectionType = SelectionType.Single;
        
        _componentList.selectedIndicesChanged += _ => { }; // Needs to be set
        _componentList.itemsChosen += enumerable =>
        {
            _window.Component = (Component)enumerable.First();
        };

        _window.SelectedObjectChangedEvent += () =>
        {
            _componentList.Rebuild();
            _componentList.ClearSelection();
            _window.UpdateProperties();
        };

        SetStyle();
        
        RootElement.Add(_componentList);
    }

    private void SetStyle()
    {
        RootElement.style.paddingLeft = 0;
        RootElement.style.paddingRight = 0;

        _curActiveToggle.style.marginLeft = 12;
        _curActiveToggle.style.marginRight = 12;

        _curActiveToggle.labelElement.style.width = 55;
        _curActiveToggle.labelElement.style.minWidth = 55;
        _curActiveToggle.labelElement.style.maxWidth = 55;
    }

    private void AddTextField(string name)
    {
        TextField textField = new TextField();
        textField.name = name;
        textField.label = name;
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

        textField.RegisterValueChangedCallback(_ => SetValues(textField, name));
        
        _window.SelectedObjectChangedEvent += () => GetChanges(textField, name);
        _window.PropertyChangedEvent += () => GetChanges(textField, name);

        RootElement.Add(textField);
    }

    private void SetValues(TextField textField, string name)
    {
        if (_window.Cur is null)
        {
            return;
        }

        bool valid = Extensions.TryParse(textField.value, out Vector3 vec);
        switch (name)
        {
            case LabelName:
                _window.Cur.name = textField.value;
                break;
            case LabelPos:
                if (valid)
                {
                    _window.Cur.transform.localPosition = vec;
                }

                break;
            case LabelScale:
                if (valid)
                {
                    _window.Cur.transform.localScale = vec;
                }

                break;
            case LabelSize:
                if (_window.CanvasScaler is not null && valid)
                {
                    _window.CanvasScaler.referenceResolution = vec;
                }
                else if (_window.RectTransform is not null && valid)
                {
                    _window.RectTransform.sizeDelta = vec;
                }

                break;
            case LabelAnchor:
                if (_window.RectTransform is not null && valid)
                {
                    _window.RectTransform.anchoredPosition = vec;
                }

                break;
        }
        
        _window.UpdateProperties();
    }
    
    private void GetChanges(TextField textField, string name)
    {
        if (_window.Cur is null)
        {
            textField.SetValueWithoutNotify("");
            return;
        }

        switch (name)
        {
            case LabelName:
                textField.SetValueWithoutNotify(_window.Cur.name);
                break;
            case LabelPos:
                textField.SetValueWithoutNotify(_window.Cur.transform.localPosition.ToSimpleString());
                break;
            case LabelScale:
                textField.SetValueWithoutNotify(_window.Cur.transform.localScale.ToSimpleString());
                break;
            case LabelSize:
                textField.SetVisible(false);
                if (_window.RectTransform is not null)
                {
                    textField.SetVisible(true);
                    textField.SetValueWithoutNotify(
                        _window.CanvasScaler is not null
                            ? _window.CanvasScaler.referenceResolution.ToSimpleString()
                            : _window.RectTransform.sizeDelta.ToSimpleString());
                }

                break;
            case LabelAnchor:
                textField.SetVisible(false);
                if (_window.RectTransform is not null)
                {
                    textField.SetVisible(true);
                    textField.SetValueWithoutNotify(_window.RectTransform.anchoredPosition.ToSimpleString());
                }

                break;
        }
    }
}