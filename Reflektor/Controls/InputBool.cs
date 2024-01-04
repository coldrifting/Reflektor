using UnityEngine.UIElements;

namespace Reflektor.Controls;

public class InputBool : InputBase
{
    private readonly Toggle _toggle = new();

    public InputBool(Info info) : base(info)
    {
        _toggle.AddToClassList("toggle");
        Add(_toggle);

        _toggle.SetEnabled(Setter is not null);
        _toggle.RegisterValueChangedCallback(_ => Setter?.Invoke(_toggle.value));
    }

    public override void PullChanges()
    {
        if (Getter.Invoke() is bool newBool)
        {
            _toggle.SetValueWithoutNotify(newBool);
        }
    }
}