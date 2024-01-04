using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Reflektor.Controls;

public static class InputAccess
{
    public static List<InputBase> GetInputs(SelectKey key)
    {
        List<MemberInfo> members = key.Target.GetType().GetMembersFiltered();

        int rowCount = 0;
        int maxWidth = 0;
        List<InputBase> output = new();
        foreach (MemberInfo mb in members)
        {
            try
            {
                InputBase input;
                bool ignoreInLengthCalc = false;
                string prefix = $"<color=#156F50>{mb.DeclaringType?.Name}</color>.";
                string name = prefix + GetNameHighlighted(mb);

                switch (mb)
                {
                    case PropertyInfo p:
                        input = GetInput(key, name, p.GetValue(key.Target), p);
                        break;
                    case FieldInfo f:
                        input = GetInput(key, name, f.GetValue(key.Target), f);
                        break;
                    case MethodInfo m:
                        input = GetInput(key, name, m, m);
                        if (m.GetParameters().Length != 0)
                        {
                            ignoreInLengthCalc = true;
                        }

                        break;
                    default:
                        throw new Exception("Unexpected member type encountered");
                }

                if (rowCount++ % 2 == 0)
                {
                    input.style.backgroundColor = new Color(0, 0, 0, 0.1f);
                }

                output.Add(input);

                if (ignoreInLengthCalc)
                {
                    continue;
                }

                int length = name.StripColor().Length;
                if (length > maxWidth)
                {
                    maxWidth = length;
                }
            }
            catch (Exception e)
            {
                Reflektor.Log(e);
            }
        }

        foreach (var input in output)
        {
            input.SetLabelWidth(maxWidth);
        }

        return output;
    }

    public static InputBase GetInput(SelectKey key, string name, object value, MemberInfo? memberInfo = null)
    {
        Info info = new Info(key, name, memberInfo);
        InputBase b = value switch
        {
            short => new NumShort(info),
            ushort => new NumUShort(info),
            int => new NumInt(info),
            uint => new NumUInt(info),
            long => new NumLong(info),
            ulong => new NumULong(info),
            float => new NumFloat(info),
            double => new NumDouble(info),
            bool => new InputBool(info),
            string => new InputTextString(info),
            Vector2 => new InputTextVec2(info),
            Vector3 => new InputTextVec3(info),
            Vector4 => new InputTextVec4(info),
            Quaternion => new InputTextQuaternion(info),
            Color => new InputTextColor(info),
            Enum enumVal => GetEnumType(info, enumVal),
            
            // Exclude Transforms from Collections
            Transform => new InputObject(info), 
            
            IEnumerable enumerable => new InputCollection(info, enumerable),
            MethodInfo m => new InputMethod(info, m),
            
            _ => new InputObject(info),
        };
        
        // Initialize outside of constructor to avoid virtual calls
        b.PullChanges();
        return b;
    }

    private static InputBase GetEnumType(Info info, Enum enumObj)
    {
        bool hasFlags = enumObj.GetType().IsDefined(typeof(FlagsAttribute), false);
        if (hasFlags)
        {
            return new InputEnumFlags(info, enumObj);
        }

        return new InputEnum(info, enumObj);
    }

    private static string GetNameHighlighted(MemberInfo memInfo)
    {
        string color = memInfo switch
        {
            PropertyInfo => "55A38E",
            FieldInfo => "B05DE7",
            MethodInfo => "FF8000",
            _ => "FFFFFF"
        };
        string name = memInfo switch
        {
            PropertyInfo propertyInfo => propertyInfo.Name,
            FieldInfo fieldInfo => fieldInfo.Name,
            MethodInfo methodInfo => GetMethodNameHighlighted(methodInfo),
            _ => "[n]"
        };
        
        return $"<color=#{color}>{name}</color>";
    }

    private static string GetMethodNameHighlighted(MethodBase method)
    {
        if (method.ReflectedType == null)
        {
            return "";
        }
        
        string name = method.Name;

        List<string> parameters = new();
        foreach (ParameterInfo parameterInfo in method.GetParameters())
        {
            string parameter = parameterInfo.ParameterType.Name;
            if (parameter.EndsWith("&"))
            {
                parameter = "<color=#158B2E>ref</color> " + parameter[..^1];
            }

            parameters.Add($"<color=#4389BB>{parameter}</color>");
        }

        return name + "(" + string.Join(",", parameters) + ")";
    }
}