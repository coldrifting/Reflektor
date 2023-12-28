using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace Reflektor.Controls;

public static class InputAccess
{
    public static InputBase GetInput(object source, MemberInfo info)
    {
        InputBase.SetSource? setSource = info.IsReadOnly() ? null : SetSource;
        
        return GetInput(info.GetValue(source), info.GetNameHighlighted(), info, source, GetSource, setSource);

        object? GetSource() => info.GetValue(source);
        void SetSource(object v) => info.SetValue(source, v);
    }
    
    public static InputBase GetInput(IList source, int index, bool isReadOnly = false)
    {
        return GetInput(source[index], index.ToString(), null, source, GetSource, isReadOnly ? null : SetSource);

        object? GetSource() => source[index];
        void SetSource(object v) => source[index] = v;
    }
    
    public static InputBase GetInput(IDictionary source, object key)
    {
        return GetInput(source[key], key.ToString(), null, source, GetSource, SetSource);

        object? GetSource() => source[key];
        void SetSource(object v) => source[key] = v;
    }

    private static InputBase GetInput(object? value, string name, MemberInfo? info, object sourceObj, InputBase.GetSource getSource, InputBase.SetSource? setSource)
    {
        return value switch
        {
            short => new NumShort(name, info, sourceObj, getSource, setSource),
            ushort => new NumUShort(name, info, sourceObj, getSource, setSource),
            int => new NumInt(name, info, sourceObj, getSource, setSource),
            uint => new NumUInt(name, info, sourceObj, getSource, setSource),
            long => new NumLong(name, info, sourceObj, getSource, setSource),
            ulong => new NumULong(name, info, sourceObj, getSource, setSource),
            float => new NumFloat(name, info, sourceObj, getSource, setSource),
            double => new NumDouble(name, info, sourceObj, getSource, setSource),
            bool => new InputBool(name, info, sourceObj, getSource, setSource),
            string => new InputTextString(name, info, sourceObj, getSource, setSource),
            Transform => new InputObject(name, info, sourceObj, value, getSource), // Exclude Transforms from Collections
            Vector2 => new InputTextVec2(name, info, sourceObj, getSource, setSource),
            Vector3 => new InputTextVec3(name, info, sourceObj, getSource, setSource),
            Vector4 => new InputTextVec4(name, info, sourceObj, getSource, setSource),
            Quaternion => new InputTextQuaternion(name, info, sourceObj, getSource, setSource),
            Color => new InputTextColor(name, info, sourceObj, getSource, setSource),
            Enum enumVal => GetEnumType(name, enumVal, info, sourceObj, getSource, setSource),
            IEnumerable collection => new InputCollection(name, collection, sourceObj, info, getSource, null),
            MethodInfo methodInfo => new InputMethod(methodInfo, sourceObj, name, getSource),
            _ => new InputObject(name, info, sourceObj, value, getSource),
        };
    }

    private static InputBase GetEnumType(string name, Enum enumObj, MemberInfo? info, object sourceObj, InputBase.GetSource getSource, InputBase.SetSource? setSource)
    {
        bool hasFlags = enumObj.GetType().IsDefined(typeof(FlagsAttribute), false);
        if (hasFlags)
        {
            return new InputEnumFlags(name, enumObj, sourceObj, info, getSource, setSource);
        }

        return new InputEnum(name, enumObj, sourceObj, info, getSource, setSource);
    }
}