using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.UIElements;

namespace Reflektor.Elements;

public class EText<T> : EBase
{
    private readonly TextField _textField;

    protected delegate string Normalizer(T? input);
    protected delegate bool Validator(string value, [NotNullWhen(true)] out T output);

    private readonly Normalizer n;
    private readonly Validator v;

    protected EText(Normalizer n, Validator v, object parent, MemberInfo? memberInfo = null, int? indexer = null, object? key = null) 
        : base(parent, memberInfo, indexer, key)
    {
        this.n = n;
        this.v = v;
        _textField = new TextField();
        if (IsReadOnly)
        {
            _textField.isReadOnly = true;
            _textField.AddToClassList("readonly");
        }
        
        _textField.RegisterValueChangedCallback(_ => SetValue());
        
        Add(_textField);
    }

    public override void GetValue()
    {
        T? value;
        if (MemInfo is not null)
        {
            value = (T)(MemInfo.GetValue(Parent) ?? false);
        }
        else if (Indexer is not null && Parent is IList list)
        {
            value = (T)list[Indexer.Value];
        }
        else if (Key is not null && Parent is IDictionary dict)
        {
            value = (T)dict[Key];
        }
        else
        {
            value = default;
        }
        
        _textField.SetValueWithoutNotify(n.Invoke(value));
    }

    protected override void SetValue()
    {
        if (IsReadOnly)
        {
            return;
        }
        
        if (!v.Invoke(_textField.value, out T result))
        {
            return;
        }
        
        if (MemInfo is not null)
        {
            MemInfo.SetValue(Parent, result);
        }
        else if (Indexer is not null && Parent is IList<T> list)
        {
            list[Indexer.Value] = result;
        }
        else if (Key is not null && Parent is IDictionary<object, T> dict)
        {
            dict[Key] = result;
        }

        Reflektor.FirePropertyChangedEvent(Parent);
    }
}