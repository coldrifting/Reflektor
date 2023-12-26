using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reflektor.Elements;

public class ECollection : EBase
{
    private bool expand;
    private readonly List<EBase> children = new();
    
    public ECollection(object parent, MemberInfo? memberInfo = null, int? indexer = null, object? key = null) 
        : base(parent, memberInfo, indexer, key)
    {
        Remove(_label);

        VisualElement line = new();
        line.Add(_label);
        
        VisualElement cont = new();
        Add(line);
        Add(cont);
        
        line.style.flexDirection = FlexDirection.Row;
        style.flexDirection = FlexDirection.Column;
        cont.style.flexDirection = FlexDirection.Column;
        
        if (MemInfo?.GetValue(parent) is IList l1)
        {
            Button expandBtn = new();
            expandBtn.text = $"Expand List | Size: {l1.Count}";
            expandBtn.clicked += () =>
            {
                expand = !expand;
                RefreshChildren();
            };
            line.Add(expandBtn);
            
            for (int i = 0; i < l1.Count; i++)
            {
                EBase b = Utils.GetElement(l1, indexer: i);
                children.Add(b);
                

                b.style.backgroundColor = i % 2 == 0
                    ? new Color(0, 0, 0.4f, 0.2f)
                    : new Color(0, 0, 0.4f, 0.1f);
                b.style.paddingLeft = Length.Percent(20);
                
                cont.Add(b);
            }

        }
        else if (MemInfo?.GetValue(parent) is IDictionary d1)
        {
            Button expandBtn = new();
            expandBtn.text = $"Expand Dictionary | Size: {d1.Count}";
            expandBtn.clicked += () =>
            {
                expand = !expand;
                RefreshChildren();
            };
            line.Add(expandBtn);

            int i = 0;
            foreach (object k in d1.Keys)
            {
                EBase b = Utils.GetElement(d1, key: k);
                children.Add(b);

                b.style.backgroundColor = i++ % 2 == 0
                    ? new Color(0, 0, 0.4f, 0.2f)
                    : new Color(0, 0, 0.4f, 0.1f);
                b.style.paddingLeft = Length.Percent(20);
                
                cont.Add(b);
            }

        }
        else if (Indexer is not null && Parent is IList l2)
        {
            Add(new Label($"List: {l2.Count}"));
            for (int i = 0; i < l2.Count; i++)
            {
                cont.Add(Utils.GetElement(l2, indexer: i));
            }
        }
        else if (Key is not null && Parent is IDictionary<object, IList> dict)
        {
            IList list2 = dict[Key];
            Add(new Label($"List: {list2.Count}"));
            int count = 0;
            foreach (object obj2 in list2)
            {
                cont.Add(Utils.GetElement(obj2, indexer: count++));
            }
        }
    }

    public override void GetValue()
    {
        RefreshChildren();
    }

    protected override void SetValue()
    {
    }

    private void RefreshChildren()
    {
        foreach (EBase e in children)
        {
            e.Refresh(expand ? DisplayFlags.All : DisplayFlags.None); //
            e.GetValue();
        }
    }
}