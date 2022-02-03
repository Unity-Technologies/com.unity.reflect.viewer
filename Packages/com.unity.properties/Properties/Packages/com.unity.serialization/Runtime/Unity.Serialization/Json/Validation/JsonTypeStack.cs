using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Serialization.Json
{
    unsafe struct JsonTypeStack : IDisposable
    {
        readonly Allocator m_Label;
        [NativeDisableUnsafePtrRestriction] JsonType* m_Stack;
        int m_Length;
        int m_Position;

        public JsonTypeStack(int length, Allocator label)
        {
            m_Label = label;
            m_Stack = (JsonType*) UnsafeUtility.Malloc(length * sizeof(JsonType), UnsafeUtility.AlignOf<JsonType>(), label);
            m_Length = length;
            m_Position = -1;
        }

        public void Push(JsonType type)
        {
            if (m_Position + 1 >= m_Length)
            {
                Resize(m_Length * 2);
            }

            m_Stack[++m_Position] = type;
        }

        public void Pop()
        {
            m_Position--;
        }

        public JsonType Peek()
        {
            return m_Position < 0 ? JsonType.Undefined : m_Stack[m_Position];
        }
        
        public JsonType Peek(int offset)
        {
            var position = m_Position - offset;
            return position < 0 ? JsonType.Undefined : m_Stack[position];
        }

        public void Clear()
        {
            m_Position = -1;
        }

        void Resize(int length)
        {
            var buffer = UnsafeUtility.Malloc(length * sizeof(JsonType), UnsafeUtility.AlignOf<JsonType>(), m_Label);
            UnsafeUtility.MemCpy(buffer, m_Stack, m_Length * sizeof(JsonType));
            UnsafeUtility.Free(m_Stack, m_Label);
            m_Stack = (JsonType*) buffer;
            m_Length = length;
        }

        public void Dispose()
        {
            UnsafeUtility.Free(m_Stack, m_Label);
            m_Stack = null;
        }
    }
}