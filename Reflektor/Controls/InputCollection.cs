using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reflektor.Controls;

public sealed class InputCollection : InputBase
{
    // GUI
    private readonly VisualElement _container = new();
    private readonly VisualElement _containerExpanded = new();
    private readonly Button _expandBtn = new();
    
    // Data
    private bool expanded;
    private readonly List<InputBase> elements = new();
    
    public InputCollection(string label, IEnumerable collection, object sourceObj, MemberInfo? info, GetSource getSource, SetSource? setSource) 
        : base(label, info, sourceObj, getSource, setSource)
    {
        Add(_container);
        _expandBtn.clicked += () =>
        {
            expanded = !expanded;
            RefreshChildren();
        };
        _expandBtn.RegisterCallback((MouseDownEvent evt) =>
        {
            if (evt.button == 1)
            {
                Reflektor.Inspect(collection);
            }
        });
        _container.Add(_expandBtn);
        _container.Add(_containerExpanded);
        _containerExpanded.AddToClassList("container-expanded");

        Setup(collection);
        Init();
    }
    
    public void Setup(IEnumerable collection)
    {
        switch (collection)
        {
            case IList list:
            {
                _expandBtn.text = $"Expand List | Size: {list.Count}";
                
                for (int i = 0; i < list.Count; i++)
                {
                    InputBase b = InputAccess.GetInput(list, i);
                    elements.Add(b);
                }

                break;
            }
            case IDictionary dictionary:
            {
                _expandBtn.text = $"Expand Dictionary | Size: {dictionary.Count}";

                foreach (object key in dictionary.Keys)
                {
                    InputBase b = InputAccess.GetInput(dictionary, key);
                    elements.Add(b);
                }

                break;
            }
            default:
                // Use temporary array?
                var l = new List<object>();
                foreach (object item in collection)
                {
                    l.Add(item);
                    InputBase b = InputAccess.GetInput(l, l.Count - 1, true);
                    elements.Add(b);
                }
                _expandBtn.text = $"Expand Enumerable | Size: {l.Count}";
                break;
        }

        bool even = false;
        foreach (InputBase c in elements)
        {
            c.Hide();
            c.style.backgroundColor = even
                ? new Color(0, 0, 0, 0.1f)
                : new Color(0, 0, 0, 0);
            even = !even;
            _containerExpanded.Add(c);
        }
    }

    protected override void SetField(object? value)
    {
    }

    private void RefreshChildren()
    {
        foreach (InputBase e in elements)
        {
            if (expanded)
            {
                e.Show();
            }
            else
            {
                e.Hide();
            }
        }
    }
}