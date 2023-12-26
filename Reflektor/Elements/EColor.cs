using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reflektor.Elements;

public class EColor : EText<Color>
{
    private readonly VisualElement _preview = new();

    public EColor(object obj, MemberInfo? memberInfo = null, int? indexer = null, object? key = null) :
        base(Utils.ToSimpleString, Utils.TryParse, obj, memberInfo, indexer, key)
    {
        _preview.AddToClassList("ColorPreview");

        // TODO switch to scss
        _preview.style.width = 20;
        _preview.style.height = 20;
        _preview.style.borderBottomLeftRadius = 3;
        _preview.style.borderBottomRightRadius = 3;
        _preview.style.borderTopLeftRadius = 3;
        _preview.style.borderTopRightRadius = 3;
        _preview.style.borderTopColor = new Color(0.3f, 0.3f, 0.3f, 0.75f);
        _preview.style.borderTopWidth = 1;
        _preview.style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f, 0.75f);
        _preview.style.borderBottomWidth = 1;
        _preview.style.borderLeftColor = new Color(0.3f, 0.3f, 0.3f, 0.75f);
        _preview.style.borderLeftWidth = 1;
        _preview.style.borderRightColor = new Color(0.3f, 0.3f, 0.3f, 0.75f);
        _preview.style.borderRightWidth = 1;
        _preview.style.marginTop = 8;
        _preview.style.marginLeft = 12;

        SetColorPreview();

        Reflektor.PropertyChangedEvent += evtObj =>
        {
            if (evtObj == Parent)
            {
                SetColorPreview();
            }
        };

        Add(_preview);
    }

    private void SetColorPreview()
    {
        if (MemInfo is not null)
        {
            if (MemInfo.GetValue(Parent) is Color color)
            {
                _preview.style.backgroundColor = new Color(color.r, color.g, color.b, 1);
            }
        }
        else if (Indexer is not null && Parent is IList<Color> list)
        {
            Color color = list[Indexer.Value];
            _preview.style.backgroundColor = new Color(color.r, color.g, color.b, 1);
        }
        else if (Key is not null && Parent is IDictionary<object, Color> dict)
        {
            Color color = dict[Key];
            _preview.style.backgroundColor = new Color(color.r, color.g, color.b, 1);
        }
    }
}
