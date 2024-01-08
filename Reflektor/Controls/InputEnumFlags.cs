using UitkForKsp2.API;
using UnityEngine.UIElements;

namespace Reflektor.Controls;

public class InputEnumFlags : InputBase
{
    private readonly Type _enumType;
    
    private bool _initialized;
    private readonly Dictionary<Enum, Toggle> _flagToggles = new();

    private readonly VisualElement _container = new();
    private readonly VisualElement _containerExpanded = new();
    private readonly Button _expandBtn = new();
    private readonly Button _applyBtn = new();
    
    private bool _expand;

    public InputEnumFlags(Info info, Enum enumObj) : base(info)
    {        
        _enumType = enumObj.GetType();
        
        BuildEnum();
    }

    private void BuildEnum()
    {
        Add(_container);
        _expandBtn.clicked += () =>
        {
            _expand = !_expand;
            if (_expand)
            {
                _containerExpanded.Show();
            }
            else
            {
                _containerExpanded.Hide();
            }
        };
        _container.Add(_expandBtn);
        _container.Add(_containerExpanded);
        _containerExpanded.AddToClassList("container-expanded");
        foreach (object? v in Enum.GetValues(_enumType))
        {
            if (v is Enum e)
            {
                string eName = e.ToString();
                Toggle t = new(eName);
                t.AddToClassList("toggle");
                t.SetEnabled(Setter is not null);
                _flagToggles.Add(e, t);
                    
                _containerExpanded.Add(t);
            }
        }

        _applyBtn.text = "Apply";
        _applyBtn.SetEnabled(Setter is not null);
        _applyBtn.clicked += PushChanges;
        _containerExpanded.Add(_applyBtn);
        _containerExpanded.Hide();
        _initialized = true;
    }

    public override void PullChanges()
    {
        if (Getter.Invoke() is Enum valEnum)
        {
            if (!_initialized)
            {
                BuildEnum();
            }
            
            foreach (object? v in Enum.GetValues(_enumType))
            {
                if (v is Enum enumVal)
                {
                    _flagToggles[enumVal].SetValueWithoutNotify(valEnum.HasFlag(enumVal));
                }
            }

            _expandBtn.text = valEnum.ToString();
        }
    }

    private void PushChanges()
    {
        Enum resultEnum = Reset();
        foreach ((Enum flag, Toggle toggle) in _flagToggles)
        {
            if (toggle.value)
            {
                SetFlag(ref resultEnum, flag);
            }
        }

        Setter?.Invoke(resultEnum);
        Refresh();
    }

    private Enum Reset()
    {
        return (Enum)Enum.ToObject(_enumType, 0);
    }
    
    private void SetFlag(ref Enum value, Enum flag)
    {
        // 'long' can hold all possible values, except those which 'ulong' can hold.
        if (Enum.GetUnderlyingType(_enumType) == typeof(ulong))
        {
            ulong numericValue = Convert.ToUInt64(value);
            numericValue |= Convert.ToUInt64(flag);
            value = (Enum)Enum.ToObject(_enumType, numericValue);
        }
        else
        {
            long numericValue = Convert.ToInt64(value);
            numericValue |= Convert.ToInt64(flag);
            value = (Enum)Enum.ToObject(_enumType, numericValue);
        }
    }
}