#if !NET_DOTS
using System;
using System.Collections.Generic;

namespace Unity.Serialization.Json.Adapters
{
    struct JsonAdapterCollection
    {
        /// <summary>
        /// Enumerates a set of adapters in a pre-defined order. This will iterate user, global and finally internal.
        /// </summary>
        public struct Enumerator
        {
            enum State
            {
                User,
                Global,
                Internal,
                End
            }
            
            readonly List<IJsonAdapter> m_User;
            readonly List<IJsonAdapter> m_Global;
            readonly JsonAdapter m_Internal;
            
            IJsonAdapter m_Current;
            State m_State;
            int m_Index;

            public Enumerator(List<IJsonAdapter> user, List<IJsonAdapter> global, JsonAdapter @internal)
            {
                m_User = user;
                m_Global = global;
                m_Internal = @internal;
                m_Current = null;
                m_State = null != user ? State.User : null != global ? State.Global : State.Internal;
                m_Index = -1;
            }

            public IJsonAdapter Current => m_Current;

            public bool MoveNext()
            {
                for (;;)
                {
                    m_Index++;
                    
                    switch (m_State)
                    {
                        case State.User:
                            if (m_Index < m_User.Count)
                            {
                                m_Current = m_User[m_Index];
                                return true;
                            }
                            m_State = State.Global;
                            m_Index = -1;
                            break;
                        case State.Global:
                            if (m_Index < m_Global.Count)
                            {
                                m_Current = m_Global[m_Index];
                                return true;
                            }
                            m_State = State.Internal;
                            m_Index = -1;
                            break;
                        case State.Internal:
                            m_Current = m_Internal;
                            m_State = State.End;
                            m_Index = -1;
                            return true;
                        case State.End:
                            m_Index = -1;
                            return false;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }
        
        public JsonAdapter InternalAdapter;
        public List<IJsonAdapter> Global;
        public List<IJsonAdapter> UserDefined;
        
        public Enumerator GetEnumerator() 
            => new Enumerator(UserDefined, Global, InternalAdapter);
    }
}
#endif
