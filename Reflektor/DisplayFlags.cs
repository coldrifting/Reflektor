using System;

namespace Reflektor;

[Flags]
public enum DisplayFlags
{
    None = 0,
    Properties = 1,
    Fields = 2,
    Methods = 4,
    All = None | Properties | Fields | Methods
}