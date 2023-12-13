using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reflektor;

public class LineItemQuat : LineItemBaseText<Quaternion>
{
    public LineItemQuat(object obj, PropertyInfo prop, VisualElement parent) : 
        base(obj, prop, parent, Extensions.TryParse, Extensions.ToSimpleString)
    {
    }
}

public class LineItemVec4 : LineItemBaseText<Vector4>
{
    public LineItemVec4(object obj, PropertyInfo prop, VisualElement parent) 
        : base(obj, prop, parent, Extensions.TryParse, Extensions.ToSimpleString)
    {
    }
}

public class LineItemVec3 : LineItemBaseText<Vector3>
{
    public LineItemVec3(object obj, PropertyInfo prop, VisualElement parent) 
        : base(obj, prop, parent, Extensions.TryParse, Extensions.ToSimpleString)
    {
    }
}

public class LineItemVec2 : LineItemBaseText<Vector2>
{
    public LineItemVec2(object obj, PropertyInfo prop, VisualElement parent) 
        : base(obj, prop, parent, Extensions.TryParse, Extensions.ToSimpleString)
    {
    }
}