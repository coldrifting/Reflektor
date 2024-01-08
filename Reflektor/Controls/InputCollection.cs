using System.Collections;
using Reflektor.Windows;
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
    private bool _expanded;
    private readonly List<InputBase> _elements = new();
    
    public InputCollection(Info info, IEnumerable collection) : base(info)
    {
        Add(_container);
        _expandBtn.clicked += () =>
        {
            _expanded = !_expanded;
            RefreshChildren();
        };
        _expandBtn.RegisterCallback((MouseDownEvent evt) =>
        {
            if (evt.button == 1)
            {
                Inspector.SwitchTab(new SelectKey(collection));
            }
        });
        _container.Add(_expandBtn);
        _container.Add(_containerExpanded);
        _containerExpanded.AddToClassList("container-expanded");

        Setup(collection);
    }

    private void Setup(IEnumerable collection)
    {
        switch (collection)
        {
            case IList list:
            {
                _expandBtn.text = $"Expand List | Size: {list.Count}";
                
                for (int i = 0; i < list.Count; i++)
                {
                    InputBase b = InputAccess.GetInput(new SelectKey(list, "", i), i.ToString(), list[i]);
                    _elements.Add(b);
                }

                break;
            }
            case IDictionary dictionary:
            {
                _expandBtn.text = $"Expand Dictionary | Size: {dictionary.Count}";

                foreach (object key in dictionary.Keys)
                {
                    InputBase b = InputAccess.GetInput(new SelectKey(dictionary, "", key), key.ToString(), dictionary[key]);
                    _elements.Add(b);
                }

                break;
            }
            default:
                int c = 0;
                foreach (object item in collection)
                {
                    InputBase b = InputAccess.GetInput(new SelectKey(collection, "", c), c++.ToString(), item);
                    _elements.Add(b);
                }
                _expandBtn.text = $"Expand Enumerable | Size: {c}";
                break;
        }

        bool even = false;
        foreach (InputBase c in _elements)
        {
            c.Hide();
            c.style.backgroundColor = even
                ? new Color(0, 0, 0, 0.1f)
                : new Color(0, 0, 0, 0);
            even = !even;
            _containerExpanded.Add(c);
        }
    }

    private void RefreshChildren()
    {
        foreach (InputBase e in _elements)
        {
            if (_expanded)
            {
                e.Show();
            }
            else
            {
                e.Hide();
            }
        }
    }

    public override void PullChanges()
    {
        foreach (InputBase b in _elements)
        {
            b.PullChanges();
        }
    }
}