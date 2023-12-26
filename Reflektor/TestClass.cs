using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Reflektor;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "CollectionNeverQueried.Local")]
public class TestClass
{
    private List<int> intList { get; set; } = new();
    private List<int> intListField = new();
    private readonly List<float> _floatList = new();
    private readonly List<string> _stringList = new();
    private readonly HashSet<string> _stringHashSet = new();
    private readonly Dictionary<string, int> _stringDict = new();
    private readonly IReadOnlyList<double> _readonlyDoubleList;

    private string[] values = new string[] { "A", "KSP 2", "test" };

    private bool boolean = true;
    
    public TestClass()
    {
        intList.Add(1);
        intList.Add(0);
        intList.Add(36);

        _floatList.Add(3.0f);
        _floatList.Add(-1.5f);

        _stringList.Add("A");
        _stringList.Add("B");
        _stringList.Add("C");
        _stringList.Add("D");

        _stringHashSet.Add("Hash");
        _stringHashSet.Add("Set");

        _stringDict.Add("A", 1);
        _stringDict.Add("B", 2);

        _readonlyDoubleList = new List<double> {3};
    }

    private static bool StaticBoolFalseMethod()
    {
        return false;
    }
    
    private bool InstanceBoolTrueMethod()
    {
        return boolean;
    }
    
    public void InstanceVoidMethod()
    {
        Debug.Log("Nothing to see here...");
    }
}