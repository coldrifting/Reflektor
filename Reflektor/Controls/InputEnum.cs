using UnityEngine.UIElements;

namespace Reflektor.Controls;

public class InputEnum : InputBase
{
    private readonly EnumField _dropdownField;

    public InputEnum(Info info, Enum enumObj) : base(info)
    {
        _dropdownField = new EnumField(enumObj);
        
        _dropdownField.SetEnabled(Setter is not null);
        _dropdownField.RegisterValueChangedCallback(_ => PushChanges());
    
        Add(_dropdownField);
    }

    public override void PullChanges()
    {
        if (Getter.Invoke() is Enum valEnum)
        {
            _dropdownField.SetValueWithoutNotify(valEnum);
        }
    }

    private void PushChanges()
    { 
        Key.SetValue(Name, _dropdownField.value);
        Refresh();
    }
}