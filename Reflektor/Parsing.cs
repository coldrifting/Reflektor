using System;
using System.Collections.Generic;
using UnityEngine;

namespace Reflektor;

public static class Parsing
{
   // String validation
   private static bool TryParseGeneric(string input, int length, float defaultVal, out Vector4 output)
   {
       string[] inputs = input.Split(new[]{',', ' '}, StringSplitOptions.RemoveEmptyEntries);
       List<float> comps = new();
       for (int i = 0; i < Math.Min(length, inputs.Length); i++)
       {
           if (float.TryParse(inputs[i], out float f))
           {
               comps.Add(f);
           }
           else
           {
               output = Vector4.zero;
               return false;
           }
       }

       output = comps.Count switch
       {
           >= 4 => new Vector4(comps[0], comps[1], comps[2], comps[3]),
           >= 3 => new Vector4(comps[0], comps[1], comps[2], defaultVal),
           >= 2 => new Vector4(comps[0], comps[1], defaultVal, defaultVal),
           >= 1 => new Vector4(comps[0], comps[0], comps[0], comps[0]),
           _ => Vector4.zero
       };
       return true;
   }
   
    public static bool TryParse(string value, out Color output)
    {
        if (TryParseGeneric(value, 4, 1.0f, out Vector4 c))
        {
            output = Clamp(c, 0.0f, 1.0f);
            return true;
        }

        output = new Color();
        return false;
    }
    
    public static bool TryParse(string value, out Quaternion output)
    {
        if (TryParseGeneric(value, 4, 0, out Vector4 outVec))
        {
            output = new Quaternion(outVec.x, outVec.y, outVec.z, outVec.w);
            return true;
        }

        output = Quaternion.identity;
        return false;
    }

    public static bool TryParse(string input, out Vector4 output)
    {
        if (TryParseGeneric(input, 4, 0, out Vector4 vec4))
        {
            output = vec4;
            return true;
        }
        
        output = Vector3.zero;
        return true;
    }
    
    public static bool TryParse(string input, out Vector3 output)
    {
        if (TryParseGeneric(input, 3, 0, out Vector4 vec4))
        {
            output = vec4;
            return true;
        }
        
        output = Vector3.zero;
        return true;
    }
    
    public static bool TryParse(string input, out Vector2 output)
    {
        if (TryParseGeneric(input, 2, 0, out Vector4 vec4))
        {
            output = vec4;
            return true;
        }
        
        output = Vector2.zero;
        return true;
    }
    
    public static string ToSimpleString(Color color)
    {
        return $"{color.r} {color.g} {color.b} {color.a}";
    }
    
    public static string ToSimpleString(Vector2 vec)
    {
        return $"{vec.x} {vec.y}";
    }
    
    public static string ToSimpleString(Vector3 vec)
    {
        return $"{vec.x} {vec.y} {vec.z}";
    }
    
    public static string ToSimpleString(Vector4 vec)
    {
        return $"{vec.x} {vec.y} {vec.z} {vec.w}";
    }
    
    public static string ToSimpleString(Quaternion quaternion)
    {
        return $"{quaternion.x} {quaternion.y} {quaternion.z} {quaternion.w}";
    }
    
    // Vectors
    private static Vector4 Clamp(Vector4 vec, float min, float max)
    {
        return new Vector4(
            Math.Clamp(vec.x, min, max),
            Math.Clamp(vec.y, min, max),
            Math.Clamp(vec.z, min, max),
            Math.Clamp(vec.w, min, max));
    }
}