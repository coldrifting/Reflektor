using System;

namespace Reflektor;

[Flags]
public enum DisplayFlags
{
    None = 0,
    ReadOnly = 1,
    Properties = 2,
    Fields = 4,
    Methods = 8,
    All = ReadOnly | Properties | Fields | Methods
}