using System.Globalization;
using System.Reflection;
using RTG;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reflektor.Elements;

public class ElementColor : BaseElementText<Color>
{
    public ElementColor(object obj, MemberInfo memberInfo) : 
        base(obj, memberInfo, Utils.TryParse, Utils.ToSimpleString)
    {
        VisualElement preview = new VisualElement();
        preview.style.width = 20;
        preview.style.height = 20;
        preview.style.borderBottomLeftRadius = 3;
        preview.style.borderBottomRightRadius = 3;
        preview.style.borderTopLeftRadius = 3;
        preview.style.borderTopRightRadius = 3;
        preview.style.borderTopColor = new Color(0.3f, 0.3f, 0.3f, 0.75f);
        preview.style.borderTopWidth = 1;
        preview.style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f, 0.75f);
        preview.style.borderBottomWidth = 1;
        preview.style.borderLeftColor = new Color(0.3f, 0.3f, 0.3f, 0.75f);
        preview.style.borderLeftWidth = 1;
        preview.style.borderRightColor = new Color(0.3f, 0.3f, 0.3f, 0.75f);
        preview.style.borderRightWidth = 1;
        preview.style.marginTop = 8;
        preview.style.marginLeft = 12;
        if (MemInfo.GetValue(Obj) is Color colorA)
        {
            preview.style.backgroundColor = colorA.KeepAllButAlpha(1);
        }
        
        Reflektor.PropertyChangedEvent += evtObj =>
        {
            if (evtObj == Obj)
            {
                if (MemInfo.GetValue(Obj) is Color color)
                {
                    preview.style.backgroundColor = color.KeepAllButAlpha(1);
                }
            }
        };

        Add(preview);
    }
}

public class ElementQuaternion : BaseElementText<Quaternion>
{
    public ElementQuaternion(object obj, MemberInfo memberInfo) : 
        base(obj, memberInfo, Utils.TryParse, Utils.ToSimpleString)
    {
    }
}

public class ElementVector4 : BaseElementText<Vector4>
{
    public ElementVector4(object obj, MemberInfo memberInfo) 
        : base(obj, memberInfo, Utils.TryParse, Utils.ToSimpleString)
    {
    }
}

public class ElementVector3 : BaseElementText<Vector3>
{
    public ElementVector3(object obj, MemberInfo memberInfo) 
        : base(obj, memberInfo, Utils.TryParse, Utils.ToSimpleString)
    {
    }
}

public class ElementVector2 : BaseElementText<Vector2>
{
    public ElementVector2(object obj, MemberInfo memberInfo) 
        : base(obj, memberInfo, Utils.TryParse, Utils.ToSimpleString)
    {
    }
}

public class ElementInt : BaseElementText<int>
{
    public ElementInt(object obj, MemberInfo memberInfo) : 
        base(obj, memberInfo, int.TryParse, i => i.ToString())
    {
    }
}

public class ElementFloat : BaseElementText<float>
{
    public ElementFloat(object obj, MemberInfo memberInfo) : 
        base(obj, memberInfo, float.TryParse, i => i.ToString(CultureInfo.InvariantCulture))
    {
    }
}

public class ElementDouble : BaseElementText<double>
{
    public ElementDouble(object obj, MemberInfo memberInfo) : 
        base(obj, memberInfo, double.TryParse, i => i.ToString(CultureInfo.InvariantCulture))
    {
    }
}

public class ElementString : BaseElementText<string>
{
    public ElementString(object obj, MemberInfo memberInfo) :
        base(obj, memberInfo, (string s, out string output) => { output = s; return true; }, s => s)
    {
    }
}