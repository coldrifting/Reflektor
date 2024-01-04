// ReSharper disable All
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

namespace Tests;

public class TestObjects
{
    public TestObjects()
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
        public SubClass2 Cls;

        public Struct1()
        {
            Cls = new();
        }

        public override string ToString()
        {
            return $"S1\\{Data}";
        }
    }

    public class SubClass2
    {
        public float F = -0.15f;
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