using System;
using System.Reflection;
using Reflektor.Extensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reflektor.Elements;

public class BaseElementText<T> : BaseElement
{
    private readonly TextField _textField = new();

    protected delegate bool Validator<TX>(string value, out TX output);

    protected BaseElementText(object obj, MemberInfo memInfo, Validator<T> validator,
        Func<T, string> normalizer) : base(obj, memInfo)
    {
        _textField.isDelayed = true;
        _textField.value = GetValue(normalizer);
        //_textField.style.fontSize = 12;
        _textField.style.minWidth = Length.Percent(10);
        _textField.style.maxWidth = Length.Percent(55);
        //_textField.style.height = Length.Percent(90);
        //_textField.style.minHeight = 24;
        //_textField.style.maxHeight = 24;
        _textField.style.paddingTop = 0;
        _textField.style.paddingBottom = 2;
        _textField.style.fontSize = 14;
        _textField.style.marginTop = 0;
        _textField.style.marginBottom = 0;
        Add(_textField);

        if (MemInfo is FieldInfo)
        {
            _textField.labelElement.style.color = Color.magenta;
            _textField.style.color = Color.magenta;
        }

        if (MemInfo.HasSetMethod() && !obj.IsStruct())
        {
            _textField.RegisterValueChangedCallback(evt =>
            {
                SetValue(evt.newValue, validator);
                _textField.value = GetValue(normalizer);
            });
        }
        else
        {
            _textField.isReadOnly = true;
            _textField.style.color = Color.gray;
        }
    }

    protected void SetValue<TE>(string newValue, Validator<TE> validator)
    {
        if (!validator.Invoke(newValue, out TE newVal))
        {
            return;
        }

        if (newVal is not null)
        {
            MemInfo.SetValue(Obj, newVal);
        }
    }
}