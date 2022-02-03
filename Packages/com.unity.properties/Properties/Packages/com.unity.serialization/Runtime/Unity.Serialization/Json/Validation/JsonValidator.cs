using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Unity.Serialization.Json
{
    unsafe partial struct JsonValidator : IDisposable
    {
        const int k_ResultSuccess = 0;
        const int k_ResultEndOfStream = -1;
        const int k_ResultInvalidJson = -2;
        const int k_DefaultDepthLimit = 128;
        
        struct Data
        {
            public JsonTypeStack Stack;
            public int CharBufferPosition;
            public ushort PrevChar;
            public JsonType Expected;
            public JsonType Actual;
            public int LineCount;
            public int LineStart;
            public int CharCount;
            public ushort Char;
            public JsonType PartialTokenType;
            public int PartialTokenState;
        }
        
        readonly JsonValidationType m_ValidationType;
        readonly Allocator m_Label;
        Data* m_Data;
        
        public JsonValidator(JsonValidationType validationType, Allocator label = SerializationConfiguration.DefaultAllocatorLabel)
        {
            m_ValidationType = validationType;
            m_Label = label;
            m_Data = (Data*) UnsafeUtility.Malloc(sizeof(Data), UnsafeUtility.AlignOf<Data>(), label);
            m_Data->Stack = new JsonTypeStack(k_DefaultDepthLimit, label);
            Reset();
        }
        
        public void Dispose()
        {
            if (null != m_Data)
            {
                m_Data->Stack.Dispose();
                UnsafeUtility.Free(m_Data, m_Label);
                m_Data = null; 
            }
        }

        public void Reset()
        {
            m_Data->Stack.Clear();
            m_Data->CharBufferPosition = 0;
            m_Data->PrevChar = '\0';
            m_Data->Expected = JsonType.Value;
            m_Data->Actual = JsonType.Undefined;
            m_Data->LineCount = 1;
            m_Data->LineStart = -1;
            m_Data->CharCount = 1;
            m_Data->Char = '\0';
            m_Data->PartialTokenType = JsonType.Undefined;
            m_Data->PartialTokenState = 0;
        }
        
        public JobHandle ScheduleValidation(UnsafeBuffer<char> buffer, int start, int count, JobHandle dependsOn = default)
        {
            m_Data->CharBufferPosition = start;

            switch (m_ValidationType)
            {
                case JsonValidationType.None:
                    return default;
                
                case JsonValidationType.Standard:
                {
                    return new StandardJsonValidationJob
                    {
                        Data = m_Data,
                        CharBuffer = (ushort*) buffer.Buffer,
                        CharBufferLength = start + count,
                    }.Schedule(dependsOn);
                }

                case JsonValidationType.Simple:
                {
                    return new SimpleJsonValidationJob
                    {
                        Data = m_Data,
                        CharBuffer = (ushort*) buffer.Buffer,
                        CharBufferLength = start + count,
                    }.Schedule(dependsOn);
                }
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public JsonValidationResult Validate(UnsafeBuffer<char> buffer, int start, int count)
        {
            ScheduleValidation(buffer, start, count).Complete();
            return GetResult();
        }
        
        public JsonValidationResult GetResult()
        {
            return new JsonValidationResult
            {
                ValidationType = m_ValidationType,
                ExpectedType = m_Data->Expected,
                ActualType = m_Data->Actual,
                Char = (char) m_Data->Char,
                LineCount = m_Data->LineCount,
                CharCount = m_Data->CharCount
            };
        }
    }
}