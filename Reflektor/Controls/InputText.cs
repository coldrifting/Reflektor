using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reflektor.Controls;

public abstract class InputText<T> : InputBase
{
    protected readonly TextField TextField = new();

    protected InputText(Info info) : base (info)
    {
        TextField.isDelayed = true;
        Add(TextField);
        
        if (Setter is null)
        {
            TextField.isReadOnly = true;
            TextField.style.color = Color.gray;
        }

        TextField.RegisterValueChangedCallback(_ => PushChanges());
    }

    public override void PullChanges()
    {
        if (Getter.Invoke() is T newValue)
        {
            TextField.SetValueWithoutNotify(ToFormattedString(newValue));
        }
    }

    private void PushChanges()
    {
        if (TryParse(TextField.value, out T newVal))
        {
            Setter?.Invoke(newVal);
        }
        Refresh();
    }

    protected abstract string ToFormattedString(T inputVal);
    protected abstract bool TryParse(string inputStr, [NotNullWhen(true)] out T outputVal);
    
    public static bool TryParseVecGeneric(string input, int length, float defaultVal, out Vector4 output)
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