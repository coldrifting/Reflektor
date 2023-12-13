using System;
using System.Reflection;

namespace Reflektor;

public abstract class LineItemBase
{
    // Data
    protected readonly object _object;
    protected readonly PropertyInfo _propertyInfo;

    protected LineItemBase(object obj, PropertyInfo propertyInfo)
    {
        _object = obj;
        _propertyInfo = propertyInfo;
    }

    protected string GetValue<T>(Func<T, string> normalizer)
    {
        if (_propertyInfo.GetValue(_object) is T newVal)
        {
            return normalizer.Invoke(newVal);
        }

        return "";
    }
}