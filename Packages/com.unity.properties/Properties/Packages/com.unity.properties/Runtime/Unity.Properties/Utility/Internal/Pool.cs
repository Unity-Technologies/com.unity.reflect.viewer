using System;
using System.Collections.Generic;

#if !NET_DOTS
using System.Linq;
#endif

namespace Unity.Properties.Internal
{
    class Pool<T> where T:class
    {
        internal static string ErrorString =>
#if !NET_DOTS            
            $"Trying to release object of type `{typeof(T).Name}` that is already pooled.";
#else
            "Trying to release object that is already pooled.";
#endif
        
        readonly Func<T> m_CreateFunc;
        readonly Action<T> m_OnRelease;

#if !NET_DOTS  
        readonly Stack<T> m_Stack;
#else
        readonly List<T> m_Stack;
#endif

        public Pool(Func<T> createInstanceFunc, Action<T> onRelease)
        {
            m_CreateFunc = createInstanceFunc;
            m_OnRelease = onRelease;
            
#if !NET_DOTS
            m_Stack = new Stack<T>();
#else
            m_Stack = new List<T>();
#endif
        }

        public T Get()
        {
#if !NET_DOTS
            return m_Stack.Count == 0 ? m_CreateFunc() : m_Stack.Pop();
#else
            if (m_Stack.Count == 0) 
                return m_CreateFunc();

            var index = m_Stack.Count - 1;
            var element = m_Stack[index];
            m_Stack.RemoveAt(index);
            return element;
#endif
        }

        public void Release(T element)
        {
            if (m_Stack.Count > 0 && Contains(element))
            {
                UnityEngine.Debug.LogError(ErrorString);
                return;
            }

            m_OnRelease?.Invoke(element);
            
#if !NET_DOTS
            m_Stack.Push(element);
#else
            m_Stack.Add(element);
#endif
        }

        bool Contains(T element)
        {
#if !NET_DOTS
            return m_Stack.Any(e => ReferenceEquals(e, element));
#else
            foreach (var e in m_Stack)
                if (ReferenceEquals(e, element))
                    return true;
            
            return false;
#endif
        }
    }
}