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
    
    public static string ToSimpleString(Vector2 vec)
    {
        return $"{vec.x} {vec.y}";
    }
    
    public static string ToSimpleString(Vector3 vec)
    {
        return $"{vec.x} {vec.y} {vec.z}";
    }
}