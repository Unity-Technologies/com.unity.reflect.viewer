using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Unity.Serialization.Json
{
    unsafe partial struct JsonValidator
    {
        [BurstCompile]
        struct StandardJsonValidationJob : IJob
        {
            // ReSharper disable once MemberHidesStaticFromOuterClass
            [NativeDisableUnsafePtrRestriction] public Data* Data;

            [NativeDisableUnsafePtrRestriction] public ushort* CharBuffer;
            public int CharBufferLength;

            int m_CharBufferPosition;
            JsonTypeStack m_Stack;
            ushort m_PrevChar;
            JsonType m_Expected;
            int m_LineCount;
            int m_LineStart;
            JsonType m_PartialTokenType;
            int m_PartialTokenState;

            void Break(JsonType actual)
            {
                var charCount = m_CharBufferPosition - m_LineStart;

                // Copy back locals to data ptr
                Data->CharBufferPosition = m_CharBufferPosition;
                Data->Stack = m_Stack;
                Data->PrevChar = m_PrevChar;
                Data->Expected = m_Expected;
                Data->Actual = actual;
                Data->LineCount = m_LineCount;
                Data->LineStart = -charCount;
                Data->CharCount = charCount;
                Data->Char = m_CharBufferPosition < CharBufferLength ? CharBuffer[m_CharBufferPosition] : '\0';
                Data->PartialTokenType = m_PartialTokenType;
                Data->PartialTokenState = m_PartialTokenState;
            }

            public void Execute()
            {
                // Copy to locals from data ptr
                m_CharBufferPosition = Data->CharBufferPosition;
                m_Stack = Data->Stack;
                m_PrevChar = Data->PrevChar;
                m_Expected = Data->Expected;
                m_LineCount = Data->LineCount;
                m_LineStart = Data->LineStart;
                m_PartialTokenType = Data->PartialTokenType;
                m_PartialTokenState = Data->PartialTokenState;

                switch (m_PartialTokenType)
                {
                    case JsonType.String:
                    {
                        var result = ReadString();
                        if (result != k_ResultSuccess)
                        {
                            m_PartialTokenType = JsonType.String;
                            m_PartialTokenState = m_PartialTokenState + m_CharBufferPosition;
                            Break(result == k_ResultEndOfStream ? JsonType.EOF : JsonType.Undefined);
                            return;
                        }

                        m_PartialTokenType = JsonType.Undefined;
                        m_PartialTokenState = 0;
                        m_CharBufferPosition++;
                    }
                        break;

                    case JsonType.Number:
                    {
                        var state = m_PartialTokenState;
                        var result = ReadNumber(ref state);

                        if (result != k_ResultSuccess)
                        {
                            m_PartialTokenType = JsonType.Number;
                            m_PartialTokenState = state;
                            Break(result == k_ResultEndOfStream ? JsonType.EOF : JsonType.Undefined);
                            return;
                        }

                        m_PartialTokenType = JsonType.Undefined;
                        m_PartialTokenState = 0;
                    }
                        break;

                    case JsonType.True:
                    {
                        var result = ReadTrue(m_PartialTokenState);
                        if (result != k_ResultSuccess)
                        {
                            m_PartialTokenType = JsonType.True;
                            m_PartialTokenState += m_CharBufferPosition;
                            Break(result == k_ResultEndOfStream ? JsonType.EOF : JsonType.Undefined);
                            return;
                        }

                        m_PartialTokenType = JsonType.Undefined;
                        m_PartialTokenState = 0;
                    }
                        break;

                    case JsonType.False:
                    {
                        var result = ReadFalse(m_PartialTokenState);
                        if (result != k_ResultSuccess)
                        {
                            m_PartialTokenType = JsonType.False;
                            m_PartialTokenState += m_CharBufferPosition;
                            Break(result == k_ResultEndOfStream ? JsonType.EOF : JsonType.Undefined);
                            return;
                        }


                        m_PartialTokenType = JsonType.Undefined;
                        m_PartialTokenState = 0;
                    }
                        break;
                    
                    case JsonType.NaN | JsonType.Null:
                    {
                        // First try reading the value as `null`
                        var start = m_CharBufferPosition;
                        var result = ReadNull(m_PartialTokenState);

                        if (result == k_ResultSuccess)
                        {
                            m_PartialTokenType = JsonType.Undefined;
                            m_PartialTokenState = 0;
                            break;
                        }

                        if (result == k_ResultEndOfStream)
                        {
                            // Otherwise we know it can only be `null`
                            m_PartialTokenState += m_CharBufferPosition;
                            m_PartialTokenType = JsonType.Null;
                            Break(JsonType.EOF);
                            return;
                        }

                        // The value can not be `null` at this point.
                        // Check for `nan`
                        m_CharBufferPosition = start;
                        result = ReadNaN(m_PartialTokenState);
                            
                        if (result == k_ResultSuccess)
                        {
                            m_PartialTokenType = JsonType.Undefined;
                            m_PartialTokenState = 0;
                            break;
                        }
                        
                        if (result == k_ResultEndOfStream)
                        {
                            m_PartialTokenState += m_CharBufferPosition;
                            m_PartialTokenType = JsonType.NaN;
                            Break(JsonType.EOF);
                            return;
                        }

                        Break(JsonType.Undefined);
                    }
                        break;

                    case JsonType.Null:
                    {
                        var result = ReadNull(m_PartialTokenState);
                        if (result != k_ResultSuccess)
                        {
                            m_PartialTokenType = JsonType.Null;
                            m_PartialTokenState += m_CharBufferPosition;
                            Break(result == k_ResultEndOfStream ? JsonType.EOF : JsonType.Undefined);
                            return;
                        }

                        m_PartialTokenType = JsonType.Undefined;
                        m_PartialTokenState = 0;
                    }
                        break;
                    
                    case JsonType.NaN:
                    {
                        var result = ReadNaN(m_PartialTokenState);
                        if (result != k_ResultSuccess)
                        {
                            m_PartialTokenType = JsonType.NaN;
                            m_PartialTokenState += m_CharBufferPosition;
                            Break(result == k_ResultEndOfStream ? JsonType.EOF : JsonType.Undefined);
                            return;
                        }

                        m_PartialTokenType = JsonType.Undefined;
                        m_PartialTokenState = 0;
                    }
                        break;
                    
                    case JsonType.Infinity:
                    {
                        var result = ReadInfinity(m_PartialTokenState);
                        if (result != k_ResultSuccess)
                        {
                            m_PartialTokenType = JsonType.Infinity;
                            m_PartialTokenState += m_CharBufferPosition;
                            Break(result == k_ResultEndOfStream ? JsonType.EOF : JsonType.Undefined);
                            return;
                        }

                        m_PartialTokenType = JsonType.Undefined;
                        m_PartialTokenState = 0;
                    }
                        break;
                }

                while (m_CharBufferPosition < CharBufferLength)
                {
                    var c = CharBuffer[m_CharBufferPosition];

                    switch (c)
                    {
                        case '{':
                        {
                            if (!IsExpected(JsonType.BeginObject))
                            {
                                Break(JsonType.BeginObject);
                                return;
                            }

                            m_Stack.Push(JsonType.BeginObject);
                            m_Expected = JsonType.String | JsonType.EndObject;
                        }
                            break;

                        case '[':
                        {
                            if (!IsExpected(JsonType.BeginArray))
                            {
                                Break(JsonType.BeginArray);
                                return;
                            }

                            m_Stack.Push(JsonType.BeginArray);
                            m_Expected = JsonType.Value | JsonType.EndArray;
                        }
                            break;

                        case '}':
                        {
                            if (!IsExpected(JsonType.EndObject))
                            {
                                Break(JsonType.EndObject);
                                return;
                            }

                            m_Stack.Pop();

                            if (m_Stack.Peek() == JsonType.String)
                            {
                                m_Stack.Pop();
                            }

                            switch (m_Stack.Peek())
                            {
                                case JsonType.BeginObject:
                                    m_Expected = JsonType.ValueSeparator | JsonType.EndObject;
                                    break;
                                case JsonType.BeginArray:
                                    m_Expected = JsonType.ValueSeparator | JsonType.EndArray;
                                    break;
                                default:
                                    m_Expected = JsonType.EOF;
                                    break;
                            }
                        }
                            break;

                        case ']':
                        {
                            if (!IsExpected(JsonType.EndArray))
                            {
                                Break(JsonType.EndArray);
                                return;
                            }

                            m_Stack.Pop();

                            if (m_Stack.Peek() == JsonType.String)
                            {
                                m_Stack.Pop();
                            }

                            switch (m_Stack.Peek())
                            {
                                case JsonType.BeginObject:
                                    m_Expected = JsonType.ValueSeparator | JsonType.EndObject;
                                    break;
                                case JsonType.BeginArray:
                                    m_Expected = JsonType.ValueSeparator | JsonType.EndArray;
                                    break;
                                default:
                                    m_Expected = JsonType.EOF;
                                    break;
                            }
                        }
                            break;

                        case ' ':
                        case '\t':
                        case '\r':
                            break;

                        case '\n':
                        {
                            m_LineCount++;
                            m_LineStart = m_CharBufferPosition;
                        }
                            break;

                        case ':':
                        {
                            if (!IsExpected(JsonType.MemberSeparator))
                            {
                                Break(JsonType.MemberSeparator);
                                return;
                            }

                            m_Expected = JsonType.Value;
                        }
                            break;

                        case ',':
                        {
                            if (!IsExpected(JsonType.ValueSeparator))
                            {
                                Break(JsonType.ValueSeparator);
                                return;
                            }

                            switch (m_Stack.Peek())
                            {
                                case JsonType.BeginObject:
                                    m_Expected = JsonType.String;
                                    break;
                                case JsonType.BeginArray:
                                    m_Expected = JsonType.Value;
                                    break;
                                default:
                                    m_Expected = JsonType.Undefined;
                                    break;
                            }
                        }
                            break;

                        case '"':
                        {
                            if (!IsExpected(JsonType.String))
                            {
                                Break(JsonType.String);
                                return;
                            }

                            var start = m_CharBufferPosition;

                            m_CharBufferPosition++;

                            var result = ReadString();

                            if (result != k_ResultSuccess)
                            {
                                m_PartialTokenType = JsonType.String;
                                m_PartialTokenState = m_CharBufferPosition - start;
                                Break(result == k_ResultEndOfStream ? JsonType.EOF : JsonType.Undefined);
                                return;
                            }
                        }
                            break;

                        case '-':
                        {
                            if (!IsExpected(JsonType.Negative))
                            {
                                Break(JsonType.Negative);
                                return;
                            }
                            
                            m_Expected = JsonType.Number | JsonType.Infinity;
                        }
                            break;

                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                        {
                            if (!IsExpected(JsonType.Number))
                            {
                                Break(JsonType.Number);
                                return;
                            }

                            var state = 0;
                            var result = ReadNumber(ref state);

                            if (result != k_ResultSuccess)
                            {
                                m_PartialTokenType = JsonType.Number;
                                m_PartialTokenState = state;
                                Break(result == k_ResultEndOfStream ? JsonType.EOF : JsonType.Undefined);
                                return;
                            }

                            m_CharBufferPosition--;
                        }
                            break;

                        case 'T':
                        case 't':
                        {
                            if (!IsExpected(JsonType.True))
                            {
                                Break(JsonType.True);
                                return;
                            }

                            var start = m_CharBufferPosition;
                            var result = ReadTrue(0);

                            if (result != k_ResultSuccess)
                            {
                                m_PartialTokenType = JsonType.True;
                                m_PartialTokenState = m_CharBufferPosition - start;
                                Break(result == k_ResultEndOfStream ? JsonType.EOF : JsonType.Undefined);
                                return;
                            }

                            m_CharBufferPosition--;
                        }
                            break;

                        case 'F':
                        case 'f':
                        {
                            if (!IsExpected(JsonType.False))
                            {
                                Break(JsonType.False);
                                return;
                            }

                            var start = m_CharBufferPosition;
                            var result = ReadFalse(0);

                            if (result != k_ResultSuccess)
                            {
                                m_PartialTokenType = JsonType.False;
                                m_PartialTokenState = m_CharBufferPosition - start;
                                Break(result == k_ResultEndOfStream ? JsonType.EOF : JsonType.Undefined);
                                return;
                            }

                            m_CharBufferPosition--;
                        }
                            break;

                        case 'N':
                        case 'n':
                        {
                            if (!IsExpected(JsonType.Null) && !IsExpected(JsonType.NaN))
                            {
                                Break(JsonType.Null | JsonType.NaN);
                                return;
                            }
                            
                            // First try reading the value as `null`
                            var start = m_CharBufferPosition;
                            var result = ReadNull(0);

                            if (result == k_ResultSuccess)
                            {
                                m_CharBufferPosition--;
                                break;
                            }

                            if (result == k_ResultEndOfStream)
                            {
                                if (m_CharBufferPosition - start == 1)
                                {
                                    // Very special case. We only looked at the first character "n" then hit EndOfStream
                                    // which means it could still be a `NaN` or `Null`
                                    m_PartialTokenState = m_CharBufferPosition - start;
                                    m_PartialTokenType = JsonType.Null | JsonType.NaN;
                                    Break(JsonType.EOF);
                                }
                                else
                                {
                                    // Otherwise we know it can only be `null`
                                    m_PartialTokenState = m_CharBufferPosition - start;
                                    m_PartialTokenType = JsonType.Null;
                                    Break(JsonType.EOF);
                                }
                                
                                return;
                            }

                            // The value can not be `null` at this point.
                            // Check for `nan`
                            m_CharBufferPosition = start;
                            result = ReadNaN(0);
                            
                            if (result == k_ResultSuccess)
                            {
                                m_CharBufferPosition--;
                                break;
                            }
                            
                            if (result == k_ResultEndOfStream)
                            {
                                m_PartialTokenState = m_CharBufferPosition - start;
                                m_PartialTokenType = JsonType.NaN;
                                Break(JsonType.EOF);
                                return;
                            }

                            Break(JsonType.Undefined);
                        }
                            break;

                        case 'I':
                        case 'i':
                        {
                            if (!IsExpected(JsonType.Infinity))
                            {
                                Break(JsonType.Infinity);
                                return;
                            }

                            var start = m_CharBufferPosition;
                            var result = ReadInfinity(0);

                            if (result != k_ResultSuccess)
                            {
                                m_PartialTokenType = JsonType.Infinity;
                                m_PartialTokenState = m_CharBufferPosition - start;
                                Break(result == k_ResultEndOfStream ? JsonType.EOF : JsonType.Undefined);
                                return;
                            }

                            m_CharBufferPosition--;
                        }
                            break;

                        default:
                        {
                            Break(JsonType.Undefined);
                            return;
                        }
                    }

                    m_CharBufferPosition++;
                }

                m_PartialTokenType = JsonType.Undefined;
                Break(JsonType.EOF);
            }

            int ReadString()
            {
                m_PrevChar = '\0';

                while (m_CharBufferPosition < CharBufferLength)
                {
                    var c = CharBuffer[m_CharBufferPosition];

                    if (c == '"' && m_PrevChar != '\\')
                    {
                        switch (m_Stack.Peek())
                        {
                            case JsonType.BeginObject:
                            {
                                m_Stack.Push(JsonType.String);
                                m_Expected = JsonType.MemberSeparator;
                            }
                                break;

                            case JsonType.BeginArray:
                            {
                                m_Expected = JsonType.ValueSeparator | JsonType.EndArray;
                            }
                                break;

                            case JsonType.String:
                            {
                                m_Stack.Pop();

                                switch (m_Stack.Peek())
                                {
                                    case JsonType.BeginObject:
                                        m_Expected = JsonType.ValueSeparator | JsonType.EndObject;
                                        break;
                                    case JsonType.BeginArray:
                                        m_Expected = JsonType.ValueSeparator | JsonType.EndArray;
                                        break;
                                    default:
                                        m_Expected = JsonType.Undefined;
                                        break;
                                }
                            }
                                break;

                            case JsonType.Undefined:
                            {
                                // Special case of reading a string at the root. The assumption is this is a single string value and can not be followed by anything.
                                m_Expected = JsonType.EOF;
                            }
                                break;
                        }

                        return k_ResultSuccess;
                    }

                    m_PrevChar = c;
                    m_CharBufferPosition++;
                }

                return k_ResultEndOfStream;
            }

            int ReadNumber(ref int state)
            {
                const int kStateStart = 0;
                const int kStateIntegerPart = 1;
                const int kStateDecimalPart = 2;
                const int kStateEPart = 3;

                m_PrevChar = '\0';

                while (m_CharBufferPosition < CharBufferLength)
                {
                    var c = CharBuffer[m_CharBufferPosition];

                    if (c == '\t' ||
                        c == '\r' ||
                        c == '\n' ||
                        c == ' ' ||
                        c == ',' ||
                        c == ']' ||
                        c == '}')
                    {
                        break;
                    }

                    switch (c)
                    {
                        case '+':
                        case '-':
                        {
                            if (state == kStateEPart)
                            {
                                break;
                            }

                            if (state != kStateStart)
                            {
                                return k_ResultInvalidJson;
                            }

                            state = kStateIntegerPart;
                        }
                            break;

                        case '.':
                        {
                            if (state != kStateIntegerPart)
                            {
                                return k_ResultInvalidJson;
                            }

                            state = kStateDecimalPart;
                        }
                            break;

                        case 'e':
                        case 'E':
                        {
                            if (m_PrevChar == '-' || m_PrevChar == '.' || state != kStateDecimalPart && state != kStateIntegerPart)
                            {
                                return k_ResultInvalidJson;
                            }

                            state = kStateEPart;
                        }
                            break;

                        default:
                        {
                            if (c < '0' || c > '9')
                            {
                                return k_ResultInvalidJson;
                            }

                            if (state == kStateStart)
                            {
                                state = kStateIntegerPart;
                            }
                        }
                            break;
                    }

                    m_PrevChar = c;
                    m_CharBufferPosition++;
                }

                if (m_CharBufferPosition >= CharBufferLength)
                {
                    return k_ResultEndOfStream;
                }

                if (m_PrevChar == 'e' || m_PrevChar == 'E' || m_PrevChar == '-' || m_PrevChar == '.')
                {
                    return k_ResultInvalidJson;
                }

                if (m_Stack.Peek() == JsonType.String)
                {
                    m_Stack.Pop();
                }

                switch (m_Stack.Peek())
                {
                    case JsonType.BeginObject:
                        m_Expected = JsonType.ValueSeparator | JsonType.EndObject;
                        break;
                    case JsonType.BeginArray:
                        m_Expected = JsonType.ValueSeparator | JsonType.EndArray;
                        break;
                    default:
                        m_Expected = JsonType.Undefined;
                        break;
                }

                return k_ResultSuccess;
            }

            int ReadTrue(int start)
            {
                var expected = stackalloc ushort[4] {'t', 'r', 'u', 'e'};
                return ReadPrimitive(expected, start, 4);
            }

            int ReadFalse(int start)
            {
                var expected = stackalloc ushort[5] {'f', 'a', 'l', 's', 'e'};
                return ReadPrimitive(expected, start, 5);
            }

            int ReadNull(int start)
            {
                var expected = stackalloc ushort[4] {'n', 'u', 'l', 'l'};
                return ReadPrimitive(expected, start, 4);
            }

            int ReadNaN(int start)
            {
                var expected = stackalloc ushort[3] {'n', 'a', 'n'};
                return ReadPrimitive(expected, start, 3);
            }
            
            int ReadInfinity(int start)
            {
                var expected = stackalloc ushort[8] {'i', 'n', 'f', 'i', 'n', 'i', 't', 'y'};
                return ReadPrimitive(expected, start, 8);
            }
            
            int ReadPrimitive(ushort* expected, int start, int length)
            {
                for (var i = start; i < length && m_CharBufferPosition < CharBufferLength; i++)
                {
                    var c = CharBuffer[m_CharBufferPosition] | 32; // to lowercase

                    if (c != expected[i])
                    {
                        return k_ResultInvalidJson;
                    }

                    m_CharBufferPosition++;
                }

                if (m_CharBufferPosition >= CharBufferLength)
                {
                    return k_ResultEndOfStream;
                }

                if (m_Stack.Peek() == JsonType.String)
                {
                    m_Stack.Pop();
                }

                switch (m_Stack.Peek())
                {
                    case JsonType.BeginObject:
                        m_Expected = JsonType.ValueSeparator | JsonType.EndObject;
                        break;
                    case JsonType.BeginArray:
                        m_Expected = JsonType.ValueSeparator | JsonType.EndArray;
                        break;
                    default:
                        m_Expected = JsonType.Undefined;
                        break;
                }

                return k_ResultSuccess;
            }

            bool IsExpected(JsonType type)
            {
                return (type & m_Expected) == type;
            }
        }
    }
}