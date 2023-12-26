using System.Globalization;
using System.Reflection;
using UnityEngine;

namespace Reflektor.Elements;

public class EQuaternion : EText<Quaternion>
{
    public EQuaternion(object obj, MemberInfo? memberInfo = null, int? indexer = null, object? key = null) : 
        base(Utils.ToSimpleString, Utils.TryParse, obj, memberInfo, indexer, key)
    {
    }
}

public class EVec4 : EText<Vector4>
{
    public EVec4(object obj, MemberInfo? memberInfo = null, int? indexer = null, object? key = null) : 
        base(Utils.ToSimpleString, Utils.TryParse, obj, memberInfo, indexer, key)
    {
    }
}

public class EVec3 : EText<Vector3>
{
    public EVec3(object obj, MemberInfo? memberInfo = null, int? indexer = null, object? key = null) : 
        base(Utils.ToSimpleString, Utils.TryParse, obj, memberInfo, indexer, key)
    {
    }
}

public class EVec2 : EText<Vector2>
{
    public EVec2(object obj, MemberInfo? memberInfo = null, int? indexer = null, object? key = null) : 
        base(Utils.ToSimpleString, Utils.TryParse, obj, memberInfo, indexer, key)
    {
    }
}

public class EInt : EText<int>
{
    public EInt(object obj, MemberInfo? memberInfo = null, int? indexer = null, object? key = null) : 
        base(i => i.ToString(), int.TryParse, obj, memberInfo, indexer, key)
    {
    }
}

public class EFloat : EText<float>
{
    public EFloat(object obj, MemberInfo? memberInfo = null, int? indexer = null, object? key = null) : 
        base(i => i.ToString(CultureInfo.InvariantCulture), float.TryParse, obj, memberInfo, indexer, key)
    {
    }
}

public class EDouble : EText<double>
{
    public EDouble(object obj, MemberInfo? memberInfo = null, int? indexer = null, object? key = null) : 
        base(i => i.ToString(CultureInfo.InvariantCulture), double.TryParse, obj, memberInfo, indexer, key)
    {
    }
}

public class EString : EText<string>
{
    public EString(object obj, MemberInfo? memberInfo = null, int? indexer = null, object? key = null) :
        base(s => s ?? "", (string v, out string o) => { o = v; return true; }, obj, memberInfo, indexer, key)
    {
    }
}