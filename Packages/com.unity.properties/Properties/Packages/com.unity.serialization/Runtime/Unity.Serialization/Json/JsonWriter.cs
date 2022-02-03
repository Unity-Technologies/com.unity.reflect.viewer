using System;
using System.Diagnostics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Serialization.Json
{
    /// <summary>
    /// Structure used to define custom behavior when writing JSON using the <see cref="JsonWriter"/>.
    /// </summary>
    public struct JsonWriterOptions
    {
        /// <summary>
        /// Gets or sets the value indicating whether the <see cref="JsonWriter"/> should skip formatting the output. This skips indentation, newlines and whitespace.
        /// </summary>
        public bool Minified { get; set; }
        
        /// <summary>
        /// Gets or sets the value indicating whether the <see cref="JsonWriter"/> should use JSON formatting. This skips optional quotes and commas.
        /// </summary>
        public bool Simplified { get; set; }
    }
    
    /// <summary>
    /// The <see cref="JsonWriter"/> provides forward only writing of encoded JSON text.
    /// </summary>
    /// <remarks>
    /// A method that attempts to write invalid JSON throws an <see cref="InvalidOperationException"/> with a context-specific error message.
    /// </remarks>
    public struct JsonWriter : IDisposable
    {
        /// <summary>
        /// This object can be used to build a JSON string in an unsafe or bursted context.
        /// </summary>
        public unsafe struct Unsafe : IDisposable
        {
            /// <summary>
            /// Helper enum to keep track of the current state.
            /// </summary>
            enum StateType
            {
                Object,
                Array
            }

            /// <summary>
            /// Structure used to keep track of the current state. This is only used for validation.
            /// </summary>
            struct State
            {
                public StateType Type;
                public int Count;
            }
            
            /// <summary>
            /// Structure used to keep track of member variables. This is stored in unmanaged memory.
            /// </summary>
            struct Data
            {
                public bool Key;
                public bool End;
                public int Indent;
            }

            const char k_BeginObjectToken = '{';
            const char k_EndObjectToken = '}';
            const char k_BeginArrayToken = '[';
            const char k_EndArrayToken = ']';
            const char k_NewlineToken = '\n';
            const char k_QuoteToken = '"';
            const char k_SpaceToken = ' ';
            
            const int k_IndentSpaceCount = 4;

            /// <summary>
            /// The allocator used when initializing this object. Used when disposing memory.
            /// </summary>
            readonly Allocator m_Label;
            
            /// <summary>
            /// Pointer to member variables. To ensure we share the same data when passed by value.
            /// </summary>
            [NativeDisableUnsafePtrRestriction] Data* m_Data;

            /// <summary>
            /// Buffer used to store characters.
            /// </summary>
            NativeList<char> m_Buffer;
        
            /// <summary>
            /// Stack used to track object and collection scopes. This is only used for validation.
            /// </summary>
            NativeList<State> m_Stack;

            /// <summary>
            /// Custom behaviour options.
            /// </summary>
            readonly JsonWriterOptions m_Options;

            /// <summary>
            /// The current number of characters in the buffer.
            /// </summary>
            /// <value>The character count.</value>
            public int Length => m_Buffer.Length;
            
            /// <summary>
            /// Gets a pointer to the memory buffer containing the characters.
            /// </summary>
            /// <returns>A pointer to the memory buffer.</returns>
            public char* GetUnsafeReadOnlyPtr() => (char*) m_Buffer.GetUnsafeReadOnlyPtr();
            
            /// <summary>
            /// Initializes a new instance of <see cref="Unsafe"/>.
            /// </summary>
            /// <param name="initialCapacity">The initial capacity to use for the internal buffer.</param>
            /// <param name="label">The allocator label to use.</param>
            /// <param name="options">Options to define custom behaviour.</param>
            public Unsafe(int initialCapacity, Allocator label, JsonWriterOptions options = default)
            {
                m_Label = label;
                m_Data = (Data*) UnsafeUtility.Malloc(sizeof(Data), UnsafeUtility.AlignOf<Data>(), label);
                UnsafeUtility.MemClear(m_Data, sizeof(Data));
            
                m_Buffer = new NativeList<char>(initialCapacity, label);
                m_Stack = new NativeList<State>(label);
                m_Options = options;
            }

            /// <inheritdoc/>
            public void Dispose()
            {
                m_Buffer.Dispose();
                m_Stack.Dispose();

                UnsafeUtility.Free(m_Data, m_Label);
                m_Data = null;
            }
            
            /// <inheritdoc/>
            public override string ToString()
            {
                return new string((char*) m_Buffer.GetUnsafeReadOnlyPtr(), 0, m_Buffer.Length);
            }

            /// <summary>
            /// Clears the writer for re-use.
            /// </summary>
            public void Clear()
            {
                UnsafeUtility.MemClear(m_Data, sizeof(Data));
                m_Stack.Clear();
                m_Buffer.Clear();
            }

            /// <summary>
            /// Writes a keyed begin object token '"key": {' and starts an object scope.
            /// </summary>
            /// <param name="ptr">The pointer to the key.</param>
            /// <param name="length">The length of the key.</param>
            public void WriteBeginObject(char* ptr, int length)
            {
                WriteKey(ptr, length);
                WriteBeginObject();
            }

            /// <summary>
            /// Writes the begin object token '{' and starts an object scope.
            /// </summary>
            public void WriteBeginObject()
            {
                ValidateWriteBeginObject();
                
                if (m_Stack.Length != 0 && PeekState().Type == StateType.Array)
                    WriteMemberSeparator();

                m_Data->Key = false;
                Write(k_BeginObjectToken);
                PushState(StateType.Object);
                m_Data->Indent++;
            }

            /// <summary>
            /// Writes the end object token '}' and closes an object scope.
            /// </summary>
            public void WriteEndObject()
            {
                ValidateWriteEndObject();
                
                m_Data->Indent--;
                
                if (!m_Options.Minified && PeekState().Count > 0)
                    WriteIndent();
                
                Write(k_EndObjectToken);
                PopState();

                if (m_Stack.Length == 0)
                    m_Data->End = true;
            }
            
            /// <summary>
            /// Writes a keyed begin array token '"key": [' and starts an array scope.
            /// </summary>
            /// <param name="ptr">The pointer to the key.</param>
            /// <param name="length">The length of the key.</param>
            public void WriteBeginArray(char* ptr, int length)
            {
                WriteKey(ptr, length);
                WriteBeginArray();
            }
            
            /// <summary>
            /// Writes the begin array token '[' and starts an array scope.
            /// </summary>
            public void WriteBeginArray()
            {
                ValidateWriteBeginArray();
                
                if (m_Stack.Length != 0 && PeekState().Type == StateType.Array)
                    WriteMemberSeparator();
                
                m_Data->Key = false;
                Write(k_BeginArrayToken);
                PushState(StateType.Array);
                m_Data->Indent++;
            }

            /// <summary>
            /// Writes the end array token ']' and closes the array scope.
            /// </summary>
            public void WriteEndArray()
            {
                ValidateWriteEndArray();
                
                m_Data->Indent--;
                
                if (!m_Options.Minified && PeekState().Count > 0)
                    WriteIndent();
                
                Write(k_EndArrayToken);
                PopState();
                
                if (m_Stack.Length == 0)
                    m_Data->End = true;
            }

            /// <summary>
            /// Writes the specified key to the buffer with the correct formatting.
            /// </summary>
            /// <param name="ptr">The pointer to the key.</param>
            /// <param name="length">The length of the key.</param>
            public void WriteKey(char* ptr, int length)
            {
                ValidateWriteKey();
                WriteMemberSeparator();
                
                var useQuotes = !m_Options.Simplified || ContainsAnySpecialCharacters(ptr, length);

                if (useQuotes) 
                    Write(k_QuoteToken);
            
                Write(ptr, length);
            
                if (useQuotes) 
                    Write(k_QuoteToken);
            
                if (!m_Options.Minified && m_Options.Simplified)
                    Write(k_SpaceToken);
            
                Write(m_Options.Simplified ? '=' : ':');
            
                if (!m_Options.Minified) 
                    Write(k_SpaceToken);

                m_Data->Key = true;
            }

            /// <summary>
            /// Writes the specified 32-bit signed integer value to the buffer with the correct formatting.
            /// </summary>
            /// <param name="value">The value to write.</param>
            public void WriteValue(int value)
            {
                FixedString128 f = default;
                f.Append(value);
                WriteValue(f);
            }

            /// <summary>
            /// Writes the specified 64-bit signed integer value to the buffer with the correct formatting.
            /// </summary>
            /// <param name="value">The value to write.</param>
            public void WriteValue(long value)
            {
                FixedString128 f = default;
                f.Append(value);
                WriteValue(f);
            }

            /// <summary>
            /// Writes the specified 32-bit floating-point value to the buffer with the correct formatting.
            /// </summary>
            /// <param name="value">The value to write.</param>
            public void WriteValue(float value)
            {
                if (float.IsInfinity(value))
                {
                    if (float.IsNegativeInfinity(value))
                        WriteNegativeInfinity();
                    else
                        WritePositiveInfinity();
                }
                
                FixedString128 f = default;
                f.Append(value);
                WriteValue(f);
            }
            
            /// <summary>
            /// Writes the literal value 'null' to the buffer with the correct formatting.
            /// </summary>
            public void WriteNull()
            {
                var chars = stackalloc char[4] {'n', 'u', 'l', 'l'};
                WriteValueLiteral(chars, 4);
            }
            
            /// <summary>
            /// Writes the literal value 'infinity' to the buffer with the correct formatting.
            /// </summary>
            void WritePositiveInfinity()
            {
                var chars = stackalloc char[8] {'i', 'n', 'f', 'i', 'n', 'i', 't', 'y'};
                WriteValueLiteral(chars, 8);
            }
            
            /// <summary>
            /// Writes the literal value '-infinity' to the buffer with the correct formatting.
            /// </summary>
            void WriteNegativeInfinity()
            {
                var chars = stackalloc char[9] {'-', 'i', 'n', 'f', 'i', 'n', 'i', 't', 'y'};
                WriteValueLiteral(chars, 9);
            }
            
            /// <summary>
            /// Writes the specified char value to the buffer with the correct formatting.
            /// </summary>
            /// <param name="value">The value to write.</param>
            public void WriteValue(char value)
            {
                WriteValue(&value, 1);
            }
            
            /// <summary>
            /// Writes the specified string value to the buffer as an encoded JSON string.
            /// </summary>
            /// <param name="ptr">The pointer to the string value.</param>
            /// <param name="length">The length of the string value.</param>
            public void WriteValue(char* ptr, int length)
            {
                ValidateWriteValue();
                    
                // Special case where the entire object is represented as a single value.
                if (m_Stack.Length != 0)
                {
                    if (PeekState().Type == StateType.Array)
                        WriteMemberSeparator();
                
                    m_Data->Key = false;
                }
                else
                {
                    m_Data->End = true;
                }

                if (null == ptr)
                {
                    WriteNull();
                    return;
                }
                
                Write('"');

                for (var i=0; i<length; i++)
                {
                    var c = ptr[i];
                    switch (c)
                    {
                        case '\\':
                            Write('\\');
                            Write('\\');
                            break;
                        case '\"':
                            Write('\\');
                            Write('\"');
                            break;
                        case '\t':
                            Write('\\');
                            Write('t');
                            break;
                        case '\r':
                            Write('\\');
                            Write('r');
                            break;
                        case '\n':
                            Write('\\');
                            Write('n');
                            break;
                        case '\b':
                            Write('\\');
                            Write('b');
                            break;
                        case '\0':
                            Write('\\');
                            Write('0');
                            break;
                        default:
                            Write(c);
                            break;
                    }
                }

                Write('"');
            }
            
            /// <summary>
            /// Writes the specified fixed string to the buffer as a literal.
            /// </summary>
            /// <param name="value">The string value to write.</param>
            void WriteValue(FixedString128 value)
            {
                var value_ptr = UnsafeUtility.AddressOf(ref value);
                var value_len = *(ushort*) value_ptr;
                var value_bytes = (byte*) value_ptr + sizeof(ushort);
                var utf16_buffer = stackalloc char[value_len];

                // This is not actually correct -- We need Utf8ToUCS but that doesn't exist
                Unicode.Utf8ToUtf16(value_bytes, value_len, utf16_buffer, out var utf16_length, value_len);
                WriteValueLiteral(utf16_buffer, utf16_length);
            }

            /// <summary>
            /// Writes the specified char to the buffer as a literal.
            /// </summary>
            /// <remarks>
            /// If you need to write out a string value with quotes <seealso cref="WriteValue(char)"/>.
            /// </remarks>
            /// <param name="value">The value to write.</param>
            public void WriteValueLiteral(char value)
            {
                WriteValueLiteral(&value, 1);
            }
            
            /// <summary>
            /// Writes the specified string to the buffer as a literal.
            /// </summary>
            /// <remarks>
            /// If you need to write out a string value with quotes <seealso cref="WriteValue(char*,int)"/>.
            /// </remarks>
            /// <param name="ptr">The pointer to the string value.</param>
            /// <param name="length">The length of the string value.</param>
            /// <exception cref="InvalidOperationException">Validation is enabled, and the operation would result in writing invalid JSON.</exception>
            public void WriteValueLiteral(char* ptr, int length)
            {
                ValidateWriteValue();

                // Special case where the entire object is represented as a single value.
                if (m_Stack.Length != 0)
                {
                    if (PeekState().Type == StateType.Array)
                        WriteMemberSeparator();
                
                    m_Data->Key = false;
                }
                else
                {
                    m_Data->End = true;
                }
                
                if (null == ptr)
                {
                    WriteNull();
                    return;
                }
                
                Write(ptr, length);
            }
            
            /// <summary>
            /// Writes the specified key-value pair with to the buffer with the correct formatting.
            /// </summary>
            /// <param name="ptr">The pointer to the key.</param>
            /// <param name="length">The length of the key.</param>
            /// <param name="value">The value to write.</param>
            public void WriteKeyValue(char* ptr, int length, int value)
            {
                WriteKey(ptr, length);
                WriteValue(value);
            }
            
            /// <summary>
            /// Writes the specified key-value pair with to the buffer with the correct formatting.
            /// </summary>
            /// <param name="ptr">The pointer to the key.</param>
            /// <param name="length">The length of the key.</param>
            /// <param name="value">The value to write.</param>
            public void WriteKeyValue(char* ptr, int length, uint value)
            {
                WriteKey(ptr, length);
                WriteValue(value);
            }
            
            /// <summary>
            /// Writes the specified key-value pair with to the buffer with the correct formatting.
            /// </summary>
            /// <param name="ptr">The pointer to the key.</param>
            /// <param name="length">The length of the key.</param>
            /// <param name="value">The value to write.</param>
            public void WriteKeyValue(char* ptr, int length, long value)
            {
                WriteKey(ptr, length);
                WriteValue(value);
            }
            
            /// <summary>
            /// Writes the specified key-value pair with to the buffer with the correct formatting.
            /// </summary>
            /// <param name="ptr">The pointer to the key.</param>
            /// <param name="length">The length of the key.</param>
            /// <param name="value">The value to write.</param>
            public void WriteKeyValue(char* ptr, int length, ulong value)
            {
                WriteKey(ptr, length);
                WriteValue(value);
            }
            
            /// <summary>
            /// Writes the specified key-value pair with to the buffer with the correct formatting.
            /// </summary>
            /// <param name="ptr">The pointer to the key.</param>
            /// <param name="length">The length of the key.</param>
            /// <param name="value">The value to write.</param>
            public void WriteKeyValue(char* ptr, int length, float value)
            {
                WriteKey(ptr, length);
                WriteValue(value);
            }
            
            /// <summary>
            /// Writes the specified key-value pair with to the buffer with the correct formatting.
            /// </summary>
            /// <param name="keyPtr">The pointer to the key.</param>
            /// <param name="keyLength">The length of the key.</param>
            /// <param name="valuePtr">The pointer to the value.</param>
            /// <param name="valueLength">The length of the value.</param>
            public void WriteKeyValue(char* keyPtr, int keyLength, char* valuePtr, int valueLength)
            {
                WriteKey(keyPtr, keyLength);
                WriteValue(valuePtr, valueLength);
            }
            
            /// <summary>
            /// Writes the specified key-value pair with to the buffer with the correct formatting.
            /// </summary>
            /// <param name="keyPtr">The pointer to the key.</param>
            /// <param name="keyLength">The length of the key.</param>
            /// <param name="valuePtr">The pointer to the value.</param>
            /// <param name="valueLength">The length of the value.</param>
            public void WriteKeyValueLiteral(char* keyPtr, int keyLength, char* valuePtr, int valueLength)
            {
                WriteKey(keyPtr, keyLength);
                WriteValueLiteral(valuePtr, valueLength);
            }

            void WriteMemberSeparator()
            {
                var count = PeekState().Count;
            
                if (!m_Options.Simplified)
                {
                    if (count > 0) 
                        Write(',');

                    if (!m_Options.Minified)
                        WriteIndent();
                }
                else
                {
                    if (!m_Options.Minified)
                        WriteIndent();
                    else if (count > 0)
                        Write(' ');
                }
                
                IncrementElementCount();
            }
            
            void WriteIndent()
            {
                if (m_Options.Minified) 
                    return;
            
                Write(k_NewlineToken);
                
                for (var i = 0; i < k_IndentSpaceCount * m_Data->Indent; i++)
                    Write(k_SpaceToken);
            }
            
            void Write(char* ptr, int length)
            {
                m_Buffer.AddRange(ptr, length);
            }

            void Write(char value)
            {
                m_Buffer.Add(value);
            }

            State PeekState()
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if (m_Stack.Length == 0)
                    throw new InvalidOperationException();
#endif
                
                return m_Stack[m_Stack.Length - 1];
            }

            void IncrementElementCount()
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if (m_Stack.Length == 0)
                    throw new InvalidOperationException();
#endif
                ref var element = ref m_Stack.ElementAt(m_Stack.Length - 1);
                element.Count++;
            }

            void PushState(StateType type)
            {
                m_Stack.Add(new State { Type = type, Count = 0});
            }

            void PopState()
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if (m_Stack.Length == 0)
                    throw new InvalidOperationException();
#endif

                m_Stack.RemoveAt(m_Stack.Length - 1);
            }
            
            static bool ContainsAnySpecialCharacters(char* ptr, int length)
            {
                for (var i = 0; i < length; i++)
                {
                    var c = ptr[i];
                    if (c == ' ' ||
                        c == '\t' ||
                        c == '\r' ||
                        c == '\n' ||
                        c == '\0' ||
                        c == ',' ||
                        c == '[' ||
                        c == ']' ||
                        c == '{' ||
                        c == '}' ||
                        c == ':' ||
                        c == '=')
                        return true;
                }

                return false;
            }
            
            [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
            void ValidateWriteBeginObject()
            {
                if (m_Data->End || !(m_Stack.Length == 0 || m_Data->Key) && PeekState().Type != StateType.Array)
                    throw new InvalidOperationException($"{nameof(WriteBeginObject)} can only called as a root element, array element or after {nameof(WriteKey)}.");
            }
            
            [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
            void ValidateWriteEndObject()
            {
                if (m_Data->End || m_Stack.Length == 0 || m_Data->Key || PeekState().Type != StateType.Object)
                    throw new InvalidOperationException($"{nameof(WriteEndObject)} can only called after {nameof(WriteBeginObject)} or {nameof(WriteValue)}.");
            }
            
            [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
            void ValidateWriteBeginArray()
            {
                if (m_Data->End || !(m_Stack.Length == 0 || m_Data->Key) && PeekState().Type != StateType.Array)
                    throw new InvalidOperationException($"{nameof(WriteBeginArray)} can only called as a root element, array element, or after {nameof(WriteKey)}.");
            }
            
            [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
            void ValidateWriteEndArray()
            {
                if (m_Data->End || m_Stack.Length == 0 || m_Data->Key || PeekState().Type != StateType.Array)
                    throw new InvalidOperationException($"{nameof(WriteEndArray)} can only called after {nameof(WriteBeginArray)} or {nameof(WriteValue)}.");
            }
            
            [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
            void ValidateWriteKey()
            {
                if (m_Data->End || m_Stack.Length == 0 || m_Data->Key || PeekState().Type != StateType.Object)
                    throw new InvalidOperationException($"{nameof(WriteKey)} can only be called in an object scope.");
            }
            
            [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
            void ValidateWriteValue()
            {
                if (m_Data->End || !(m_Stack.Length == 0 || m_Data->Key) && PeekState().Type != StateType.Array)
                    throw new InvalidOperationException($"{nameof(WriteValue)} can only be called as a root element, array element, or after {nameof(WriteKey)}.");
            }
        }

        /// <summary>
        /// Disposable struct to manage opening and closing object scopes.
        /// </summary>
        public struct ObjectScope : IDisposable
        {
            JsonWriter Writer;

            /// <summary>
            /// Creates a new object scope for the given <see cref="JsonWriter"/>
            /// </summary>
            /// <param name="writer">The writer to write to.</param>
            /// <param name="key">The key to write.</param>
            public ObjectScope(JsonWriter writer, string key)
            {
                Writer = writer;
                Writer.WriteBeginObject(key);
            }

            /// <inheritdoc/>
            public void Dispose()
                => Writer.WriteEndObject();
        }
        
        /// <summary>
        /// Disposable struct to manage opening and closing array scopes.
        /// </summary>
        public struct ArrayScope : IDisposable
        {
            JsonWriter Writer;

            /// <summary>
            /// Creates a new array scope for the given <see cref="JsonWriter"/>
            /// </summary>
            /// <param name="writer">The writer to write to.</param>
            /// <param name="key">The key to write.</param>
            public ArrayScope(JsonWriter writer, string key = null)
            {
                Writer = writer;
                Writer.WriteBeginArray(key);
            }

            /// <inheritdoc/>
            public void Dispose()
                => Writer.WriteEndArray();
        }

        Unsafe m_Impl;
        
        /// <summary>
        /// Initializes a new instance of <see cref="JsonWriter"/>.
        /// </summary>
        /// <param name="label">The allocator label to use.</param>
        /// <param name="options">Options to define custom behaviour.</param>
        public JsonWriter(Allocator label, JsonWriterOptions options = default)
            => m_Impl = new Unsafe(32, label, options);
        
        /// <summary>
        /// Initializes a new instance of <see cref="JsonWriter"/>.
        /// </summary>
        /// <param name="initialCapacity">The initial capacity to use for the internal buffer.</param>
        /// <param name="label">The allocator label to use.</param>
        /// <param name="options">Options to define custom behaviour.</param>
        public JsonWriter(int initialCapacity, Allocator label, JsonWriterOptions options = default)
            => m_Impl = new Unsafe(initialCapacity, label, options);

        /// <inheritdoc/>
        public void Dispose()
            => m_Impl.Dispose();

        /// <inheritdoc/>
        public override string ToString()
            => m_Impl.ToString();

        /// <summary>
        /// Returns the unsafe writer which can be used in bursted jobs.
        /// </summary>
        /// <returns>The unsafe writer.</returns>
        public Unsafe AsUnsafe() => m_Impl;

        /// <summary>
        /// Clears the writer for re-use.
        /// </summary>
        public void Clear()
            => m_Impl.Clear();

        /// <summary>
        /// Creates an object scope which writes the beginning '"key": {' and ending '}' tokens.
        /// </summary>
        /// <param name="key">The key to write.</param>
        /// <returns>A disposable object scope.</returns>
        public ObjectScope WriteObjectScope(string key = null)
            => new ObjectScope(this, key);

        /// <summary>
        /// Creates a collection scope which writes the beginning '"key": [' and ending ']' tokens.
        /// </summary>
        /// <param name="key">The key to write.</param>
        /// <returns>A disposable collection scope.</returns>
        public ArrayScope WriteArrayScope(string key = null)
            => new ArrayScope(this, key);
        
        /// <summary>
        /// Writes a keyed object token '"key": [' and opens an object scope.
        /// </summary>
        /// <param name="key">The key to write.</param>
        public void WriteBeginObject(string key = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                m_Impl.WriteBeginObject();
                return;
            }
            
            unsafe
            {
                fixed (char* ptr = key)
                {
                    m_Impl.WriteBeginObject(ptr, key.Length);
                }
            }
        }
        
        /// <summary>
        /// Writes the end object token '}' and closes the object scope.
        /// </summary>
        public void WriteEndObject()
        {  
            m_Impl.WriteEndObject();
        }
        
        /// <summary>
        /// Writes a keyed array token '"key": [' and opens an array scope.
        /// </summary>
        /// <param name="key">The key to write.</param>
        public void WriteBeginArray(string key = null)
        {  
            if (string.IsNullOrEmpty(key))
            {
                m_Impl.WriteBeginArray();
                return;
            }
            
            unsafe
            {
                fixed (char* ptr = key)
                {
                    m_Impl.WriteBeginArray(ptr, key.Length);
                }
            }
        }
        
        /// <summary>
        /// Writes the end array token ']' and closes the array scope.
        /// </summary>
        public void WriteEndArray()
        {  
            m_Impl.WriteEndArray();
        }
        
        /// <summary>
        /// Writes the specified key to the buffer with the correct formatting.
        /// </summary>
        /// <param name="key">The key to write.</param>
        public void WriteKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            unsafe
            {
                fixed (char* ptr = key)
                {
                    m_Impl.WriteKey(ptr, key.Length);
                }
            }
        }

        /// <summary>
        /// Writes the specified 32-bit signed integer value to the buffer with the correct formatting.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteValue(int value)
            => m_Impl.WriteValue(value);

        /// <summary>
        /// Writes the specified 64-bit signed integer value to the buffer with the correct formatting.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteValue(long value)
            => m_Impl.WriteValue(value);

        /// <summary>
        /// Writes the specified 32-bit floating-point value to the buffer with the correct formatting.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteValue(float value)
            => m_Impl.WriteValue(value);
        
#if !NET_DOTS
        /// <summary>
        /// Writes the specified 32-bit unsigned integer value to the buffer with the correct formatting.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteValue(uint value)
        {
            var str = value.ToString();

            unsafe
            {
                fixed (char* ptr = str)
                {
                    m_Impl.WriteValueLiteral(ptr, str.Length);
                }
            }
        }

        /// <summary>
        /// Writes the specified 64-bit unsigned integer value to the buffer with the correct formatting.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteValue(ulong value)
        {
            var str = value.ToString();

            unsafe
            {
                fixed (char* ptr = str)
                {
                    m_Impl.WriteValueLiteral(ptr, str.Length);
                }
            }
        }
        
        /// <summary>
        /// Writes the specified 64-bit floating-point value to the buffer with the correct formatting.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteValue(double value)
        {
            var str = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            
            unsafe
            {
                fixed (char* ptr = str)
                {
                    m_Impl.WriteValueLiteral(ptr, str.Length);
                }
            }
        }
#endif
        
        /// <summary>
        /// Writes the literal value 'null' to the buffer with the correct formatting.
        /// </summary>
        public void WriteNull()
            => m_Impl.WriteNull();

        /// <summary>
        /// Writes the specified char value to the buffer with the correct formatting.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteValue(char value)
            => m_Impl.WriteValue(value);

        /// <summary>
        /// Writes the specified string value to the buffer with the correct formatting.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteValue(string value)
        {
            if (null == value)
            {
                WriteNull();
                return;
            }
            
            unsafe
            {
                fixed (char* ptr = value)
                {
                    m_Impl.WriteValue(ptr, value.Length);
                }
            }
        }

        /// <summary>
        /// Writes the specified char value to the buffer with the correct formatting.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteValueLiteral(char value)
            => m_Impl.WriteValueLiteral(value);

        /// <summary>
        /// Writes the specified string value to the buffer with the correct formatting.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteValueLiteral(string value)
        {
            if (null == value)
            {
                WriteNull();
                return;
            }
            
            unsafe
            {
                fixed (char* ptr = value)
                {
                    m_Impl.WriteValueLiteral(ptr, value.Length);
                }
            }
        }

        /// <summary>
        /// Writes the specified key-value pair with to the buffer with the correct formatting.
        /// </summary>
        /// <param name="key">The key to write.</param>
        /// <param name="value">The value to write.</param>
        public void WriteKeyValue(string key, int value)
        {
            WriteKey(key);
            WriteValue(value);
        }
        
        /// <summary>
        /// Writes the specified key-value pair with to the buffer with the correct formatting.
        /// </summary>
        /// <param name="key">The key to write.</param>
        /// <param name="value">The value to write.</param>
        public void WriteKeyValue(string key, long value)
        {
            WriteKey(key);
            WriteValue(value);
        }
        
        /// <summary>
        /// Writes the specified key-value pair with to the buffer with the correct formatting.
        /// </summary>
        /// <param name="key">The key to write.</param>
        /// <param name="value">The value to write.</param>
        public void WriteKeyValue(string key, float value)
        {
            WriteKey(key);
            WriteValue(value);
        }

#if !NET_DOTS
        /// <summary>
        /// Writes the specified key-value pair with to the buffer with the correct formatting.
        /// </summary>
        /// <param name="key">The key to write.</param>
        /// <param name="value">The value to write.</param>
        public void WriteKeyValue(string key, uint value)
        {
            WriteKey(key);
            WriteValue(value);
        }
        
        /// <summary>
        /// Writes the specified key-value pair with to the buffer with the correct formatting.
        /// </summary>
        /// <param name="key">The key to write.</param>
        /// <param name="value">The value to write.</param>
        public void WriteKeyValue(string key, ulong value)
        {
            WriteKey(key);
            WriteValue(value);
        }
        
        /// <summary>
        /// Writes the specified key-value pair with to the buffer with the correct formatting.
        /// </summary>
        /// <param name="key">The key to write.</param>
        /// <param name="value">The value to write.</param>
        public void WriteKeyValue(string key, double value)
        {
            WriteKey(key);
            WriteValue(value);
        }
#endif        
        
        /// <summary>
        /// Writes the specified key-value pair with to the buffer with the correct formatting.
        /// </summary>
        /// <param name="key">The key to write.</param>
        /// <param name="value">The value to write.</param>
        public void WriteKeyValue(string key, string value)
        {
            WriteKey(key);
            WriteValue(value);
        }      
        
        /// <summary>
        /// Writes the specified key-value pair with to the buffer with the correct formatting.
        /// </summary>
        /// <param name="key">The key to write.</param>
        /// <param name="value">The value to write.</param>
        public void WriteKeyValueLiteral(string key, string value)
        {
            WriteKey(key);
            WriteValueLiteral(value);
        }
    }
}