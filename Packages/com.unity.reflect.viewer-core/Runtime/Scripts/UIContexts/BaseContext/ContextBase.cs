using System;
using System.Collections.Generic;
using Unity.Properties;

namespace UnityEngine.Reflect.Viewer.Core
{
    public abstract class ContextBase<TContext> : IUIContext where TContext : ContextBase<TContext>, new()
    {
        IContextTarget m_RegisteredTarget;

        public abstract List<Type> implementsInterfaces { get; }

        public IContextTarget target
        {
            get => m_RegisteredTarget;
            private set
            {
                if (m_RegisteredTarget != value)
                {
                    if (m_RegisteredTarget != null)
                    {
                        m_RegisteredTarget.stateChanged -= OnStateChanged;
                    }

                    m_RegisteredTarget = value;
                    if (m_RegisteredTarget != null)
                    {
                        m_RegisteredTarget.stateChanged += OnStateChanged;
                    }

                    OnStateChanged(); //initial state changed request for on-the-fly binding
                }
            }
        }

        public static TContext current { get; private set; } = new TContext();

        public Func<object> GetPropertyGetter(string name)
        {
            return () => current.target.GetValue(name);
        }

        bool IUIContext.ContainsProperty(string propertyName)
        {
            return ContainsProperty(propertyName);
        }

        public event Action stateChanged = delegate {};

        protected void OnStateChanged()
        {
            stateChanged?.Invoke();
        }

        public static bool ContainsProperty(string propertyName)
        {
            if (current == null || current.target == null)
            {
                throw new ApplicationException("The referenced context is invalid, or no Target is configured in this context.");
            }
            return current.target.IsPathValid(new PropertyPath(propertyName));
        }

        public static IContextTarget BindTarget<T>(T data) where T : new()
        {
            current.target = new ContextTarget<T>(ref data);
            return current.target;
        }

        public static void Clear()
        {
            current.target = null;
            current = Activator.CreateInstance(typeof(TContext), true) as TContext;
        }
    }
}
