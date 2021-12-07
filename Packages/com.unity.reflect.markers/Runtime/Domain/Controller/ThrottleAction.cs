using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

// @@TODO: Move this to Unity.Reflect or Replace
namespace Unity.Reflect.Markers
{
    /// <summary>
    /// Throttles the invoking of an action
    /// </summary>
    public class ThrottleAction
    {
        private Action m_Value;
        private float m_Wait = 2f;
        private InvokeWhen m_InvokeWhen;

        bool m_Throttling = false;

        CoroutineObject m_CoroutineRunner;
        IEnumerator m_Coroutine;

        /// <summary>
        /// Throttles the invoking of an action to the given time.
        /// Creates a game object, and runs on the Main thread within a coroutine.
        /// </summary>
        /// <param name="function">Action to invoke</param>
        /// <param name="wait">Time between invokes</param>
        /// <param name="invokeWhen">Time when to invoke. Trailing = end of wait, Leading = before the wait.</param>
        public void Run(Action function, float wait = 2f, InvokeWhen invokeWhen = InvokeWhen.Trailing,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = -1)
        {
            if (m_CoroutineRunner == null)
                m_CoroutineRunner = new GameObject($"ThrottleActionObject - {filePath} : {lineNumber} - {memberName}  ({this.GetHashCode()})").AddComponent<CoroutineObject>();
            m_Value = function;
            this.m_Wait = wait;
            this.m_InvokeWhen = invokeWhen;
            if (m_Throttling)
                return;
            m_Coroutine = Invoker();
            this.m_CoroutineRunner.StartCoroutine(m_Coroutine);
        }

        /// <summary>
        /// Cancels any scheduled actions.
        /// </summary>
        public void Cancel()
        {
            if (m_Coroutine != null && m_CoroutineRunner)
                m_CoroutineRunner.StopCoroutine(m_Coroutine);
        }

        private class CoroutineObject : MonoBehaviour
        {
            public void Destroy()
            {
                GameObject.Destroy(gameObject);
            }
        }

        public enum InvokeWhen
        {
            Leading,
            Trailing
        }

        IEnumerator Invoker()
        {
            m_Throttling = true;
            if (m_InvokeWhen == InvokeWhen.Leading)
                m_Value?.Invoke();
            yield return new WaitForSeconds(m_Wait);
            if (m_InvokeWhen == InvokeWhen.Trailing)
                m_Value?.Invoke();
            m_Throttling = false;
        }

        ~ThrottleAction()
        {
            Cancel();
            if (m_CoroutineRunner)
                m_CoroutineRunner.Destroy();
        }
    }

}

