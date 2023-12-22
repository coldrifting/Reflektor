using System.Globalization;
using System.Reflection;
using UnityEngine;

namespace Reflektor.Elements;

public class ElementColor : BaseElementText<Color>
{
    public ElementColor(object obj, MemberInfo memberInfo) : 
        base(obj, memberInfo, Utils.TryParse, Utils.ToSimpleString)
    {
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