using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reflektor.Controls;

public abstract class InputText<T> : InputBase
{
    private readonly TextField _textField = new();

    protected InputText(string label, MemberInfo? info, object sourceObj, GetSource getSource, SetSource? setSource = null) 
        : base(label, info, sourceObj, getSource, setSource)
    {
        _textField.isDelayed = true;
        Add(_textField);
        
        if (setSource is null)
        {
            _textField.isReadOnly = true;
            _textField.style.color = Color.gray;
        }
        else
        {
            _textField.RegisterValueChangedCallback(evt =>
            {
                if (TryParse(evt.newValue, out T newVal))
                {
                    _setSource?.Invoke(newVal);
                }
                Refresh();
            });
        }

        Init();
    }

    protected override void SetField(object? newValue)
    {
        if (newValue is T newT)
        {
            _textField.SetValueWithoutNotify(ToFormattedString(newT));
        }
    }

    protected abstract string ToFormattedString(T inputVal);
    protected abstract bool TryParse(string inputStr, [NotNullWhen(true)] out T outputVal);
    
    protected bool TryParseVecGeneric(string input, int length, float defaultVal, out Vector4 output)
    {
        string[] inputs = input.Split(new[]{',', ' '}, StringSplitOptions.RemoveEmptyEntries);
        List<float> comps = new();
        for (int i = 0; i < Math.Min(length, inputs.Length); i++)
        {
            if (float.TryParse(inputs[i], out float f))
            {
                comps.Add(f);
            }
            else
            {
                output = Vector4.zero;
                return false;
            }
        }

        output = comps.Count switch
        {
            >= 4 => new Vector4(comps[0], comps[1], comps[2], comps[3]),
            >= 3 => new Vector4(comps[0], comps[1], comps[2], defaultVal),
            >= 2 => new Vector4(comps[0], comps[1], defaultVal, defaultVal),
            >= 1 => new Vector4(comps[0], comps[0], comps[0], comps[0]),
            _ => Vector4.zero
        };
        return true;
    }
    
    protected static Vector4 Clamp(Vector4 vec, float min, float max)
    {
        return new Vector4(
            Math.Clamp(vec.x, min, max),
            Math.Clamp(vec.y, min, max),
            Math.Clamp(vec.z, min, max),
            Math.Clamp(vec.w, min, max));
    }
}