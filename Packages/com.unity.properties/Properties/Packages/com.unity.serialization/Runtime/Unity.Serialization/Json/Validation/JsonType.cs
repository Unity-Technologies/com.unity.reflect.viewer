using System;

namespace Unity.Serialization.Json
{
    [Flags]
    enum JsonType
    {
        Undefined = 1 << 0,
        BeginObject = 1 << 1, // '{'
        EndObject = 1 << 2, // '}'
        BeginArray = 1 << 3, // '['
        EndArray = 1 << 4, // ']'
        MemberSeparator = 1 << 5, // ':'
        ValueSeparator = 1 << 6, // ','
        String = 1 << 7, // '"'..'".
        Number = 1 << 8, // '0'..'9', 'e', 'E', '-'
        Negative = 1 << 9,
        NaN = 1 << 10,
        Infinity = 1 << 11,
        True = 1 << 12, // 'true'
        False = 1 << 13, // 'false'
        Null = 1 << 14, // 'null'
        EOF = 1 << 15,

        // Any value type
        Value = BeginObject | BeginArray | String | Number | Negative | NaN | Infinity | True | False | Null
    }
}