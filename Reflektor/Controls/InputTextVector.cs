using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reflektor.Controls;

public class InputTextColor : InputText<Color>
{
    private readonly VisualElement _preview = new();

    public InputTextColor(string label, MemberInfo? info, object sourceObj, GetSource getSource, SetSource? setSource = null) 
        : base(label, info, sourceObj, getSource, setSource)
    {
        _preview.pickingMode = PickingMode.Ignore;
        _preview.AddToClassList("color-preview");
        
        Add(_preview);
    }

    protected override void SetField(object? newValue)
    {
        base.SetField(newValue);

        if (newValue is Color color)
        {
            _preview.style.backgroundColor = new Color(color.r, color.g, color.b, 1.0f);
        }
    }

    protected override string ToFormattedString(Color inputVal)
    {
        return $"{inputVal.r} {inputVal.g} {inputVal.b} {inputVal.a}";
    }

    protected override bool TryParse(string inputStr, out Color outputVal)
    {
        if (ColorUtility.TryParseHtmlString(inputStr, out Color color))
        {
            outputVal = color;
            return true;
        }

        if (ColorUtility.TryParseHtmlString($"#{inputStr}", out Color color2))
        {
            outputVal = color2;
            return true;
        }
        
        if (TryParseVecGeneric(inputStr, 4, 1.0f, out Vector4 c))
        {
            outputVal = Clamp(c, 0.0f, 1.0f);
            return true;
        }
        
        outputVal = new Color();
        return false;
    }
}

public class InputTextQuaternion : InputText<Quaternion>
{
    public InputTextQuaternion(string label, MemberInfo? info, object sourceObj, GetSource getSource, SetSource? setSource = null) 
        : base(label, info, sourceObj, getSource, setSource)
    {
    }

    protected override string ToFormattedString(Quaternion inputVal)
    {
        return $"{inputVal.x} {inputVal.y} {inputVal.z} {inputVal.w}";
    }

    protected override bool TryParse(string inputStr, out Quaternion outputVal)
    {
        if (TryParseVecGeneric(inputStr, 4, 0.0f, out Vector4 q))
        {
            outputVal = new Quaternion(q.x, q.y, q.z, q.w);
            return true;
        }

        outputVal = Quaternion.identity;
        return false;
    }
}

public class InputTextVec4 : InputText<Vector4>
{
    public InputTextVec4(string label, MemberInfo? info, object sourceObj, GetSource getSource, SetSource? setSource = null) 
        : base(label, info, sourceObj, getSource, setSource)
    {
    }

    protected override string ToFormattedString(Vector4 inputVal)
    {
        return $"{inputVal.x} {inputVal.y} {inputVal.z} {inputVal.w}";
    }

    protected override bool TryParse(string inputStr, out Vector4 outputVal)
    {
        if (TryParseVecGeneric(inputStr, 4, 0.0f, out Vector4 v))
        {
            outputVal = v;
            return true;
        }

        outputVal = Vector4.zero;
        return false;
    }
}

public class InputTextVec3 : InputText<Vector3>
{
    public InputTextVec3(string label, MemberInfo? info, object sourceObj, GetSource getSource, SetSource? setSource = null) 
        : base(label, info, sourceObj, getSource, setSource)
    {
    }

    protected override string ToFormattedString(Vector3 inputVal)
    {
        return $"{inputVal.x} {inputVal.y} {inputVal.z}";
    }

    protected override bool TryParse(string inputStr, out Vector3 outputVal)
    {
        if (TryParseVecGeneric(inputStr, 4, 0.0f, out Vector4 v))
        {
            outputVal = new Vector3(v.x, v.y, v.z);
            return true;
        }

        outputVal = Vector3.zero;
        return false;
    }
}

public class InputTextVec2 : InputText<Vector2>
{
    public InputTextVec2(string label, MemberInfo? info, object sourceObj, GetSource getSource, SetSource? setSource = null) 
        : base(label, info, sourceObj, getSource, setSource)
    {
    }

    protected override string ToFormattedString(Vector2 inputVal)
    {
        return $"{inputVal.x} {inputVal.y}";
    }

    protected override bool TryParse(string inputStr, out Vector2 outputVal)
    {
        if (TryParseVecGeneric(inputStr, 4, 0.0f, out Vector4 v))
        {
            outputVal = new Vector2(v.x, v.y);
            return true;
        }

        outputVal = Vector2.zero;
        return false;
    }
}