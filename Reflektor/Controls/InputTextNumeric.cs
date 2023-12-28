using System.Globalization;
using System.Reflection;

namespace Reflektor.Controls;

public class NumShort : InputText<short>
{
    public NumShort(string label, MemberInfo? info, object sourceObj, GetSource getSource, SetSource? setSource = null) 
        : base(label, info, sourceObj, getSource, setSource)
    {
    }

    protected override string ToFormattedString(short inputVal)
    {
        return inputVal.ToString();
    }

    protected override bool TryParse(string inputStr, out short outputVal)
    {
        if (short.TryParse(inputStr, out short output))
        {
            outputVal = output;
            return true;
        }

        outputVal = default;
        return false;
    }
}

public class NumUShort : InputText<ushort>
{
    public NumUShort(string label, MemberInfo? info, object sourceObj, GetSource getSource, SetSource? setSource = null) 
        : base(label, info, sourceObj, getSource, setSource)
    {
    }

    protected override string ToFormattedString(ushort inputVal)
    {
        return inputVal.ToString();
    }

    protected override bool TryParse(string inputStr, out ushort outputVal)
    {
        if (ushort.TryParse(inputStr, out ushort output))
        {
            outputVal = output;
            return true;
        }

        outputVal = default;
        return false;
    }
}

public class NumInt : InputText<int>
{
    public NumInt(string label, MemberInfo? info, object sourceObj, GetSource getSource, SetSource? setSource = null) 
        : base(label, info, sourceObj, getSource, setSource)
    {
    }

    protected override string ToFormattedString(int inputVal)
    {
        return inputVal.ToString();
    }

    protected override bool TryParse(string inputStr, out int outputVal)
    {
        if (int.TryParse(inputStr, out int output))
        {
            outputVal = output;
            return true;
        }

        outputVal = default;
        return false;
    }
}

public class NumUInt : InputText<uint>
{
    public NumUInt(string label, MemberInfo? info, object sourceObj, GetSource getSource, SetSource? setSource = null) 
        : base(label, info, sourceObj, getSource, setSource)
    {
    }

    protected override string ToFormattedString(uint inputVal)
    {
        return inputVal.ToString();
    }

    protected override bool TryParse(string inputStr, out uint outputVal)
    {
        if (uint.TryParse(inputStr, out uint output))
        {
            outputVal = output;
            return true;
        }

        outputVal = default;
        return false;
    }
}

public class NumLong : InputText<long>
{
    public NumLong(string label, MemberInfo? info, object sourceObj, GetSource getSource, SetSource? setSource = null) 
        : base(label, info, sourceObj, getSource, setSource)
    {
    }

    protected override string ToFormattedString(long inputVal)
    {
        return inputVal.ToString();
    }

    protected override bool TryParse(string inputStr, out long outputVal)
    {
        if (long.TryParse(inputStr, out long output))
        {
            outputVal = output;
            return true;
        }

        outputVal = default;
        return false;
    }
}

public class NumULong : InputText<ulong>
{
    public NumULong(string label, MemberInfo? info, object sourceObj, GetSource getSource, SetSource? setSource = null) 
        : base(label, info, sourceObj, getSource, setSource)
    {
    }

    protected override string ToFormattedString(ulong inputVal)
    {
        return inputVal.ToString();
    }

    protected override bool TryParse(string inputStr, out ulong outputVal)
    {
        if (ulong.TryParse(inputStr, out ulong output))
        {
            outputVal = output;
            return true;
        }

        outputVal = default;
        return false;
    }
}

public class NumFloat : InputText<float>
{
    public NumFloat(string label, MemberInfo? info, object sourceObj, GetSource getSource, SetSource? setSource = null) 
        : base(label, info, sourceObj, getSource, setSource)
    {
    }

    protected override string ToFormattedString(float inputVal)
    {
        return inputVal.ToString(CultureInfo.InvariantCulture);
    }

    protected override bool TryParse(string inputStr, out float outputVal)
    {
        if (float.TryParse(inputStr, out float output))
        {
            outputVal = output;
            return true;
        }

        outputVal = default;
        return false;
    }
}

public class NumDouble : InputText<double>
{
    public NumDouble(string label, MemberInfo? info, object sourceObj, GetSource getSource, SetSource? setSource = null) 
        : base(label, info, sourceObj, getSource, setSource)
    {
    }

    protected override string ToFormattedString(double inputVal)
    {
        return inputVal.ToString(CultureInfo.InvariantCulture);
    }

    protected override bool TryParse(string inputStr, out double outputVal)
    {
        if (double.TryParse(inputStr, out double output))
        {
            outputVal = output;
            return true;
        }

        outputVal = default;
        return false;
    }
}