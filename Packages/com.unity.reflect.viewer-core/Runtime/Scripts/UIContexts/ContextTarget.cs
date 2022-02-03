using System;
using System.Collections.Generic;
using Unity.Properties;

namespace UnityEngine.Reflect.Viewer.Core
{
    public enum UpdateNotification
    {
        NotifyIfChange,
        ForceNotify,
        DoNotNotify
    }

    public interface IContextTarget
    {
        event Action stateChanged;

        bool IsPathValid(PropertyPath path);
        object GetValue(string name);
        void UpdateWith<TContainer>(ref TContainer data, UpdateNotification notifyChange = UpdateNotification.NotifyIfChange);
        void UpdateValueWith<TValue>(string propertyName, ref TValue propertyValue, bool notifyChange = true);
        bool TryGetTarget<T>(out T t);
    }

    public class ContextTarget<T>: IContextTarget where T : new()
    {
        public bool TryGetTarget<TContainer>(out TContainer target)
        {
            if (m_Target is TContainer t)
            {
                target = t;
                return true;
            }

            target = default;
            return false;
        }

        public event Action stateChanged = delegate { };

        object m_Target;

        public ContextTarget(ref T target) : this()
        {
            UpdateWith(ref target, UpdateNotification.DoNotNotify);
        }

        public ContextTarget()
        {
            m_Target = new T();
        }

        public object GetValue(string name)
        {
            return PropertyContainer.GetValue<object>(ref m_Target, name);
        }

        /// <summary>
        /// Update the current context with the new Container. If you do a change on List make sure that
        /// you use UpdateNotification.ForceNotify so that it will trigger the change.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="notifyChange"></param>
        /// <typeparam name="TContainer"></typeparam>
        public void UpdateWith<TContainer>(ref TContainer data, UpdateNotification notifyChange)
        {
            // nothing changed
            if (data.Equals(m_Target) && notifyChange != UpdateNotification.ForceNotify)
                return;

            bool changed = false;

            // go through each property
            var propertyBag = PropertyBag.GetPropertyBag<TContainer>();
            foreach (var property in propertyBag.GetProperties(ref data))
            {
                var name = property.Name;
                var value = property.GetValue(ref data);

                if (TrySetValue(name, value))
                    changed = notifyChange != UpdateNotification.DoNotNotify;
            }

            if (changed || notifyChange == UpdateNotification.ForceNotify)
                stateChanged?.Invoke();
        }

        public void UpdateValueWith<TValue>(string propertyName, ref TValue propertyValue, bool notifyChange)
        {
            bool changed = TrySetValue(propertyName, propertyValue);

            if (notifyChange && changed)
                stateChanged?.Invoke();
        }

        bool TrySetValue<TValue>(string name, TValue value)
        {
            var oldValue = PropertyContainer.GetValue<TValue>(ref m_Target, name);

            if (PropertyContainer.TrySetValue(ref m_Target, name, value))
            {
                // see if the value actually changed (this could be reverted due to constraints, range, etc.)
                var newValue = PropertyContainer.GetValue<TValue>(ref m_Target, name);

                if (!EqualityComparer<TValue>.Default.Equals(newValue, oldValue))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsPathValid(PropertyPath path)
        {
            return path.PartsCount == 0 || PropertyContainer.IsPathValid(ref m_Target, path);
        }
    }
}
