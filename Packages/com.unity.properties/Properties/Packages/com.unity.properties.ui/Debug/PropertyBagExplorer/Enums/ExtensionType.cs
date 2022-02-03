using System;

namespace Unity.Properties.Debug
{
    [Flags]
    public enum ExtensionType
    {
        Serialization = 1 << 1,
        UI = 1 << 2,
        Properties = 1 << 3,
    }
}
