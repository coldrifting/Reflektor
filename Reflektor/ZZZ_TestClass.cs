using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable All
#pragma warning disable CS0414 // Field is assigned but its value is never used

namespace Reflektor;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "CollectionNeverQueried.Local")]
public class ZZZ_TestClass : MonoBehaviour
{
    private Vector3 avec = new Vector3(1, 2, 3);
    
    private Color color = new Color(1, 0, 0.5f, 0.5f);
    private Color colorProp { get; set; } = new Color(1f, 0.5f, 0.25f, 0.5f);
    
    private EasingMode _enumBasic = EasingMode.Linear;
    public DisplayFlags EnumFlags = DisplayFlags.Methods | DisplayFlags.Properties;
    
    private List<int> intList { get; set; } = new();
    private List<int> intListNoSet { get; } = new();
    private List<int> intListField = new();
    private readonly List<float> _floatList = new();
    private readonly List<string> _stringList = new();
    private readonly HashSet<string> _stringHashSet = new();
    private readonly Dictionary<string, int> _stringDict = new();
    private readonly IReadOnlyList<double> _readonlyDoubleList;

    private string[] values = new string[] { "A", "KSP 2", "test" };

    private int testInt = 4;
    private float testFloat = -0.15f;
    private bool boolean = true;
    
    public ZZZ_TestClass()
    {
        intList.Add(1);
        intList.Add(0);
        intList.Add(36);

        intListNoSet.Add(45);

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