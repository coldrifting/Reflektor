using System.Collections.Generic;

namespace ReflektorTests;

class Root
{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    public Root()
    {
        Visuals.Add(new Struct1());

        Dict.Add("123.ABC", new Struct1());
    }

    public List<Struct1> Visuals = new();
    public Dictionary<string, Struct1> Dict = new();
    public Struct1[] Arr = { new() };
    public Struct2 Nest = new();
    public Struct3 S = new();

    public struct Struct1
    {
        public Struct2 Data;
        
        public override string ToString()
        {
            return $"S1\\{Data}";
        }
    }

    public struct Struct2
    {
        public Struct3 Sprite;
        
        public override string ToString()
        {
            return $"S2\\{Sprite}";
        }
    }

    public struct Struct3
    {
        public int Color;

        public override string ToString()
        {
            return $"S3:Color {Color}";
        }
    }
}