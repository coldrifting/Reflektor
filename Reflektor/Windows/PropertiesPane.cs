using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reflektor.Windows;

public class PropertiesPane
{
    private readonly MainWindow _window;
    
    // GUI elements
    public VisualElement RootElement { get; }
    private readonly ScrollView _scrollView = new();

    // Data
    private readonly List<Action> _delegates = new();
    private readonly List<PropertyInfo> _properties = new();
    
    public PropertiesPane(MainWindow window)
    {
        _window = window;

        RootElement = new VisualElement();
        RootElement.Add(_scrollView);
        
        _window.SelectedObjectChangedEvent += () =>
        {
            _properties.Clear();
            if (_window.Component != null)
            {
                foreach (PropertyInfo p in _window.Component.GetType().GetProperties())
                {
                    _properties.Add(p);
                }
            }
            
            _scrollView.Clear();
            foreach (Action action in _delegates)
            {
                _window.PropertyChangedEvent -= action;
            }
            _delegates.Clear();

            List<VisualElement> elements = new();
            
            foreach (PropertyInfo p in _properties)
            {
                if (_window.Component is null)
                {
                    return;
                }
                
                object value = p.GetValue(_window.Component);
                VisualElement v = value switch
                {
                    bool valueBool => AddButton(p, _window.Component, valueBool),
                    string valueString => AddText(p, _window.Component, valueString, s => s, (string s, out string output) =>
                    {
                        output = s;
                        return true;
                    }),
                    Vector2 valueVec2 => AddText(p, _window.Component, valueVec2, Extensions.ParseExtensions.ToSimpleString, Extensions.ParseExtensions.TryParse),
                    Vector3 valueVec3 => AddText(p, _window.Component, valueVec3, Extensions.ParseExtensions.ToSimpleString, Extensions.ParseExtensions.TryParse),
                    Vector4 valueVec4 => AddText(p, _window.Component, valueVec4, Extensions.ParseExtensions.ToSimpleString, Extensions.ParseExtensions.TryParse),
                    Quaternion valueQuat => AddText(p, _window.Component, valueQuat, Extensions.ParseExtensions.ToSimpleString, Extensions.ParseExtensions.TryParse),
                    int valInt => AddText(p, _window.Component, valInt, s => s.ToString(), int.TryParse),
                    float valFloat => AddText(p, _window.Component, valFloat, s => s.ToString(CultureInfo.InvariantCulture), float.TryParse),
                    _ => AddLabel(p, _window.Component, value)
                };
                elements.Add(v);
            }
                
            foreach (var el in elements.Where(el => el is not Label))
            {
                _scrollView.Add(el);
            }
                
            foreach (var el in elements.OfType<Label>())
            {
                _scrollView.Add(el);
            }
        };
    }

    private VisualElement AddButton(PropertyInfo p, Component c, bool defaultValue)
    {
        Toggle toggle = new Toggle();
        toggle.label = p.Name;
        toggle.value = defaultValue;
        if (p.SetMethod != null)
        {
            toggle.RegisterValueChangedCallback(evt =>
            {
                p.SetValue(c, evt.newValue);
                _window.UpdateProperties();
            });
        }
        else
        {
            toggle.SetEnabled(false);
        }

        Action propertyChangeAction = () => toggle.SetValueWithoutNotify((bool)p.GetValue(c));
        _window.PropertyChangedEvent += propertyChangeAction;
        _delegates.Add(propertyChangeAction);

        return toggle;
    }
    
    private VisualElement AddLabel(PropertyInfo p, Component c, object? defaultValue)
    {
        Label label = new Label($"{p.Name}: {defaultValue}");
        label.style.color = Color.grey;
        _scrollView.Add(label);

        Action propertyChangeAction = () => label.text = $"{p.Name}: {p.GetValue(c)}";
        _window.PropertyChangedEvent += propertyChangeAction;
        _delegates.Add(propertyChangeAction);

        return label;
    }
    
    private delegate bool Validator<T>(string value, out T output);
    
    private VisualElement AddText<T>(PropertyInfo p, Component c, T defaultValue, Func<T, string> normalizer, Validator<T>? validator)
    {
        try
        {
            if (_window.Component is null)
            {
                return new Label();
            }

            TextField textField = new TextField(p.Name);
            textField.value = normalizer.Invoke(defaultValue);
            textField.isDelayed = true;

            textField.isReadOnly = validator is null;
            if (validator is not null && p.GetSetMethod() != null)
            {
                textField.RegisterValueChangedCallback(evt =>
                {
                    if (validator.Invoke(evt.newValue, out T output))
                    {
                        p.SetValue(c, output);
                    }

                    _window.UpdateProperties();
                });
            }
            else
            {
                textField.isReadOnly = true;
                textField.style.color = Color.grey;
            }

            Action propertyChangeAction = () => textField.SetValueWithoutNotify(normalizer.Invoke((T)p.GetValue(c)));
            _window.PropertyChangedEvent += propertyChangeAction;
            _delegates.Add(propertyChangeAction);

            return textField;

        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log(e.StackTrace);
            Debug.Log(e.Source);
        }

        return new Label("NADA");
    }
}