using System.Reflection;

namespace Reflektor.Controls;

public class InputTextString : InputText<string>
{
    public InputTextString(string label, MemberInfo? info, object sourceObj, GetSource getSource, SetSource? setSource = null) 
        : base(label, info, sourceObj, getSource, setSource)
    {
    }

    protected override string ToFormattedString(string inputVal)
    {
        return inputVal;
    }

    protected override bool TryParse(string inputStr, out string outputVal)
    {
        outputVal = inputStr;
        return true;
    }
}