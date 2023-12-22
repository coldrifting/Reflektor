using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Reflektor.Elements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reflektor.Windows;

public class InspectorTab : VisualElement
{
    private readonly VisualElement _canvas = new();
    
    public InspectorTab(object obj)
    {
        ScrollView scrollView = new(ScrollViewMode.Vertical);
        int counter = 0;

        List<MemberInfo> members = new();
        members.AddRange(obj.GetType().GetAllProperties().Where(IgnoreCompilerAttributes));
        members.AddRange(obj.GetType().GetAllFields().Where(IgnoreCompilerAttributes));

        foreach (MemberInfo memberInfo in members)
        {
            try
            {
                BaseElement element;

                object? memberObj = memberInfo.GetValue(obj);
                if (memberObj is null)
                {
                    continue;
                }
                Type memberType = memberObj.GetType();
                if (memberType.IsEnum)
                {
                    element = new ElementEnum(obj, memberInfo);
                }
                else if (memberType.IsGenericList())
                {
                    element = new ElementCollection(obj, memberInfo);
                }
                else
                {
                    element = GetElement(obj, memberInfo);
                }

                element.style.backgroundColor = counter++ % 2 == 0
                    ? new Color(0.118f, 0.129f, 0.161f)
                    : new Color(0.129f, 0.149f, 0.18f);
                
                scrollView.Add(element);
            }
            catch (Exception e)
            {
                Reflektor.Log("Could not add an element...");
                Reflektor.Log(e.Message);
            }
        }

        scrollView.style.width = Length.Percent(100);
        scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
        _canvas.Add(scrollView);
        
        Add(scrollView);

        _canvas.style.height = Length.Percent(100);
        _canvas.style.width = Length.Percent(100);
        style.height = Length.Percent(100);
    }

    private static bool IgnoreCompilerAttributes(MemberInfo m)
    {
        return m.GetCustomAttribute<CompilerGeneratedAttribute>() == null;
    }

    private static BaseElement GetElement(object obj, MemberInfo memberInfo)
    {
        BaseElement x = memberInfo.GetValue(obj) switch
        {
            int => new ElementInt(obj, memberInfo),
            float => new ElementFloat(obj, memberInfo),
            double => new ElementDouble(obj, memberInfo),
            bool => new ElementBool(obj, memberInfo),
            string => new ElementString(obj, memberInfo),
            Vector2 => new ElementVector2(obj, memberInfo),
            Vector3 => new ElementVector3(obj, memberInfo),
            Vector4 => new ElementVector4(obj, memberInfo),
            Quaternion => new ElementQuaternion(obj, memberInfo),
            Color => new ElementColor(obj, memberInfo),
            _ => new ElementObject(obj, memberInfo)
        };
        return x;
    }
}