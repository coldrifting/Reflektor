using System;
using System.Globalization;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reflektor;

public class LineItemBaseText<T> : LineItemBase
{
    protected readonly TextField textField = new();

    protected delegate bool Validator<TX>(string value, out TX output);
    protected LineItemBaseText(object obj, PropertyInfo propertyInfo, VisualElement parent, Validator<T> validator, Func<T, string> normalizer) : base(obj, propertyInfo)
    {
        textField.label = propertyInfo.Name;
        textField.isDelayed = true;
        parent.Add(textField);

        if (_propertyInfo.SetMethod is not null)
        {
            textField.RegisterValueChangedCallback(evt =>
            {
                SetValue<T>(evt.newValue, validator);
                textField.value = GetValue<T>(normalizer);
            });
            return;
        }

        textField.isReadOnly = true;
        textField.style.color = Color.gray;
    }

    protected void SetValue<TE>(string newValue, Validator<TE> validator)
    {
        if (validator.Invoke(newValue, out TE newVal))
        {
            _propertyInfo.SetValue(_object, newVal);
        }
    }
}