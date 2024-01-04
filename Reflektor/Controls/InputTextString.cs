namespace Reflektor.Controls;

public class InputTextString : InputText<string>
{
    public InputTextString(Info info) : base (info)
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