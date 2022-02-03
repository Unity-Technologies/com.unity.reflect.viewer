using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Unity.Serialization.Json
{
    unsafe partial struct JsonValidator
    {
        [BurstCompile]
        struct SimpleJsonValidationJob : IJob
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
                        case '=':
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
                        
                        default:
                        {
                            if (!IsExpected(JsonType.Value) && !IsExpected(JsonType.String))
                            {
                                Break(JsonType.Value);
                                return;
                            }
                            
                            var start = m_CharBufferPosition;
                            var result = ReadPrimitive();

                            if (result != k_ResultSuccess)
                            {
                                m_PartialTokenType = JsonType.String;
                                m_PartialTokenState = m_CharBufferPosition - start;
                                Break(result == k_ResultEndOfStream ? JsonType.EOF : JsonType.Undefined);
                                return;
                            }
                        }
                        break;
                    }

                    m_CharBufferPosition++;
                }

                m_PartialTokenType = JsonType.Undefined;
                Break(JsonType.EOF);
            }
            
            int ReadPrimitive()
            {
                while (m_CharBufferPosition < CharBufferLength)
                {
                    var c = CharBuffer[m_CharBufferPosition];

                    if (c == '\t' ||
                        c == '\r' ||
                        c == '\n' ||
                        c == ',' ||
                        c == ' ' ||
                        c == ':' ||
                        c == '=' ||
                        c == ']' ||
                        c == '}' ||
                        c == '{' ||
                        c == '[')
                    {
                        switch (m_Stack.Peek())
                        {
                            case JsonType.Undefined:
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
                        }
                        
                        m_CharBufferPosition--;
                        return k_ResultSuccess;
                    }

                    m_PrevChar = c;
                    m_CharBufferPosition++;
                }

                // Special case, we are looking at a partial value of a root un scoped object.
                // This is technically valid even though we may be receiving more data.
                if (m_Stack.Peek() == JsonType.String && m_Stack.Peek(1) == JsonType.Undefined)
                {
                    m_Expected |= JsonType.EOF;
                }

                m_PartialTokenType = JsonType.Value;
                return k_ResultEndOfStream;
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
                            case JsonType.Undefined:
                            case JsonType.BeginObject:
                            {
                                m_Stack.Push(JsonType.String);
                                m_Expected = JsonType.MemberSeparator | JsonType.EOF;
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
                                    case JsonType.Undefined:
                                        m_Expected = JsonType.String | JsonType.ValueSeparator | JsonType.EOF;
                                        break;
                                    case JsonType.BeginObject:
                                        m_Expected = JsonType.String | JsonType.ValueSeparator | JsonType.EndObject | JsonType.EOF;
                                        break;
                                    case JsonType.BeginArray:
                                        m_Expected = JsonType.ValueSeparator | JsonType.EndArray;
                                        break;
                                }
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
            
            bool IsExpected(JsonType type)
            {
                return (type & m_Expected) == type;
            }
        }
    }
}