using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reflektor.Elements;

public class BaseElementText<T> : BaseElement
{
    private readonly TextField _textField = new();
    protected readonly Func<T, string> Normalizer;

    protected delegate bool Validator<TX>(string value, out TX output);

    protected BaseElementText(object obj, MemberInfo memInfo, Validator<T> validator,
        Func<T, string> normalizer) : base(obj, memInfo)
    {
        Normalizer = normalizer;
        _textField.isDelayed = true;
        SetValue();
        SetStyle();
        Add(_textField);

        if (MemInfo is FieldInfo)
        {
            _textField.labelElement.style.color = Color.magenta;
            _textField.style.color = Color.magenta;
        }

        if (MemInfo.IsReadOnly() || Obj.GetType().IsStruct())
        {
            _textField.isReadOnly = true;
            _textField.style.color = Color.gray;
        }
        else
        {
            Reflektor.PropertyChangedEvent += evtObj =>
            {
                if (evtObj == Obj)
                {
                    SetFieldValue();
                }
            };
            _textField.RegisterValueChangedCallback(evt =>
            {
                SetValue(evt.newValue, validator);
                Reflektor.FirePropertyChangedEvent(obj);
            });
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

    private void SetValue()
    {
        SetFieldValue();
    }

    protected override void SetFieldValue()
    {
        _textField.value = GetValue(Normalizer);
    }

    private void SetStyle()
    {
        _textField.style.minWidth = Length.Percent(10);
        _textField.style.maxWidth = Length.Percent(55);
        _textField.style.height = 30;
        _textField.style.paddingTop = 2;
        _textField.style.paddingBottom = 2;
        _textField.style.fontSize = 14;
        _textField.style.marginTop = 0;
        _textField.style.marginBottom = 0;
        
        foreach (VisualElement v in _textField.Children())
        {
            v.style.borderTopLeftRadius = 6;
            v.style.borderTopRightRadius = 6;
            v.style.borderBottomLeftRadius = 6;
            v.style.borderBottomRightRadius = 6;
        }
    }
}