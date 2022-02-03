using System;

namespace Unity.Properties.Debug
{
    [Flags]
    enum TypeTraits
    {
        Struct = 1 << 1,
        Class = 1 << 2,
        Unmanaged = 1 << 3,
        Blittable = 1 << 4,
        Generic = 1 << 5
    }
}
