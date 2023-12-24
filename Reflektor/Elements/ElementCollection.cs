using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UIElements;

namespace Reflektor.Elements;

public class ElementCollection : BaseElement
{
    public ElementCollection(object obj, MemberInfo memberInfo, bool isArray = false) : base(obj, memberInfo)
    {
        Label l = new Label(memberInfo.Name);
        ListView list = new();
        Add(l);
        Add(list);

        List<object> objs = new();
        switch (memberInfo)
        {
            case PropertyInfo propertyInfo:
            {
                if (propertyInfo.GetValue(Obj) is IEnumerable newVal)
                {
                    foreach (var v in newVal)
                    {
                        objs.Add(v);
                    }
                }

                break;
            }
            case FieldInfo fieldInfo:
            {
                if (fieldInfo.GetValue(Obj) is IEnumerable newVal)
                {
                    foreach (var v in newVal)
                    {
                        objs.Add(v);
                    }
                }

                break;
            }
        }

        list.itemsSource = objs;
        list.makeItem += () => new Label();
        list.bindItem += (element, i) =>
        {
            if (element is not Label label)
            {
                return;
            }

            label.text = objs[i].ToString();
            label.RegisterCallback((MouseDownEvent evt) =>
            {
                if (evt.button == 1)
                {
                    Reflektor.Inspect(objs[i]);
                }
            });
        };
    }

    protected override void SetFieldValue()
    {
        // TODO
    }
}