using System;
using System.Collections;
using System.Collections.Generic;

namespace Reflektor;

public class SelectKey
{
    public object Root { get; }
    public string Path { get; }

    public object Target => GetTargetObj();
    private Type TargetType { get; }

    private readonly object? _cache;
    private readonly object? _index;

    public SelectKey(object root, string path = "", object? index = null)
    {
        if (root.GetType().IsValueType && root is not IEnumerable)
        {
            throw new ArgumentException("Root object cannot be a struct");
        }
        
        Root = root;
        Path = path;
        
        _index = index;

        object obj = Target;
        TargetType = obj.GetType();
        if (!TargetType.IsValueType)
        {
            _cache = obj;
        }
    }

    private object GetTargetObj()
    {
        if (_cache is not null)
        {
            return _cache;
        }

        object cur = Root switch {
            IList l when _index is int i => l[i] ?? throw new ArgumentException(),
            IDictionary d when _index is not null => d[_index] ?? throw new ArgumentException(),
            IEnumerable e when _index is int i => e.Get(i) ?? throw new ArgumentException(),
            _ => Root
        };

        var comps = Path.Split('.', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (string s in comps)
        {
            cur = cur.GetValue(s) ?? throw new NullReferenceException("Can't find target object");
        }

        return cur;
    }

    public object? GetValue(string name)
    {
        if (name == "" && _index is not null)
        {
            switch (Root)
            {
                case IList list when _index is int i:
                    return list[i];
                case IDictionary dict:
                    return dict[_index];
                case IEnumerable e when _index is int i:
                    return e.Get(i);
            }
        }
        
        return Target.GetValue(name);
    }

    public void SetValue(string name, object? value)
    {
        //Reflektor.Log($"set: n: {name} v: {value}");
        if (name == "" && _index is not null)
        {
            switch (Root)
            {
                case IList list when _index is int i:
                    list[i] = value;
                    return;
                case IDictionary dict:
                    dict[_index] = value;
                    return;
            }
        }
        
        SelectKey key = this;
        object? index = _index;
        int recursionTimeout = 0;
        while (recursionTimeout < 250)
        {
            recursionTimeout++;
            object cur = key.Target;
            switch (cur)
            {
                case IList l when index is int i:
                    l[i] = value;
                    return;
                case IDictionary dict when index is not null:
                    dict[index] = value;
                    return;
                default:
                    cur.SetValue(name, value);

                    // Invariant - root is not a struct
                    if (!cur.GetType().IsValueType)
                    {
                        return;
                    }

                    int lastOccurs = key.Path.LastIndexOf('.');
                    (string newPath, string newName) x;
                    if (lastOccurs != -1)
                    {
                        x.newPath = key.Path[..lastOccurs];
                        x.newName = key.Path[(lastOccurs + 1)..];
                    }
                    else
                    {
                        x.newPath = "";
                        x.newName = key.Path;
                    }
                    
                    SelectKey k = new SelectKey(key.Root, x.newPath, x.newPath == key.Path ? null : key._index);

                    key = k;
                    name = x.newName;
                    value = cur;

                    continue;
            }
        }

        throw new TimeoutException("Timed out while setting value");
    }

    public bool CanSetValue(string name)
    {
        if (name == "" && Root is IList or IDictionary)
        {
            return true;
        }
        
        if (Root is IEnumerable and (not IList or IDictionary) && _index is not null)
        {
            return false;
        }
        
        if (TargetType.GetProperty(name) is { } p)
        {
            if (typeof(ICollection).IsAssignableFrom(p.PropertyType))
            {
                return typeof(IList).IsAssignableFrom(p.PropertyType) ||
                       typeof(IDictionary).IsAssignableFrom(p.PropertyType);
            }

            if (p.GetSetMethod() is null)
            {
                return false;
            }
        }

        if (TargetType.GetField(name) is { } f)
        {
            if (typeof(ICollection).IsAssignableFrom(f.FieldType))
            {
                return typeof(IList).IsAssignableFrom(f.FieldType) ||
                       typeof(IDictionary).IsAssignableFrom(f.FieldType);
            }

            if (f.IsInitOnly || f.IsLiteral)
            {
                return false;
            }
        }

        return true;
    }

    // Returns a select key to the given path, concatenating with the current sub key path
    // Tries to find a root object as close to the target object as possible.
    // If no better option is found, the root will be the same as the current sub key
    // This allows us to always walk up the tree of objects when setting values,
    // in order to allow for setting struct values
    // Invariants are
    // 1: the root is always a class
    // 2: whenever we encounter a class (such as an array), it becomes the new root.
    public SelectKey GetSubKey(string targetPath)
    {
        object root = Root;
        object cur = Root switch
        {
            IList l when _index is int i => l[i],
            IDictionary dict when _index is not null => dict[_index],
            IEnumerable e when _index is int i => e.Get(i),
            _ => Root
        };

        if (_index is not null)
        {
            if (targetPath.StartsWith(_index.ToString()))
            {
                targetPath = targetPath[_index.ToString().Length..];
            }
        }

        object? newIndex = _index;
        List<string> newPathList = new();
        string[]? pathComps = $"{Path}.{targetPath}".Split('.', StringSplitOptions.RemoveEmptyEntries);
        foreach (var pathComp in pathComps)
        {
            cur = cur.GetValue(pathComp) ?? throw new NullReferenceException($"Could not find target because a path component was null: [{Path}.{targetPath}]");
            if (cur.GetType().IsValueType)
            {
                newPathList.Add(pathComp);
            }
            else
            {
                root = cur;
                newIndex = null;
                newPathList.Clear();
            }
        }

        string newPath = string.Join('.', newPathList);

        return new SelectKey(root, newPath, newIndex);
    }

    public override bool Equals(object? obj)
    {
        if (obj is SelectKey otherKey)
        {
            return otherKey.Root.Equals(Root) &&
                   otherKey.Path.Equals(Path) &&
                   Equals(otherKey._index, _index);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return Root.GetHashCode() - Path.GetHashCode() + _index?.GetHashCode() ?? 0;
    }
}