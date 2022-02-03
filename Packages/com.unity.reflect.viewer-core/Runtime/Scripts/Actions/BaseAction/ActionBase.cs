using System;
using System.Collections.Generic;
using SharpFlux;
using Unity.Properties;

namespace UnityEngine.Reflect.Viewer.Core
{
    public abstract class ActionBase : IViewerAction
    {
        public virtual void ApplyPayload<T>(object actionData, ref T stateData, Action onStateDataChanged)
        {
            CopyProperties(actionData, ref stateData, out var stateDataChanged);

            if (stateDataChanged)
                onStateDataChanged?.Invoke();
        }

        protected internal object CopyProperties<T>(object sourceData, ref T targetData, out bool targetDataChanged)
        {
            // Todo: convert to Transfer Visitor and use Value Tuples instead of anonymous objects
            object boxed = targetData;
            var t = sourceData.GetType();
            targetDataChanged = false;
            foreach (var p in t.GetProperties())
            {
                if (PropertyContainer.IsPathValid(ref targetData, new PropertyPath(p.Name)))
                {
                    var oldValue = PropertyContainer.GetValue<object>(ref boxed, p.Name);
                    PropertyContainer.SetValue(ref targetData, p.Name, p.GetValue(sourceData));
                    boxed = targetData;
                    var newValue = PropertyContainer.GetValue<object>(ref boxed, p.Name);
                    if (!EqualityComparer<object>.Default.Equals(newValue, oldValue))
                        targetDataChanged = true;
                }
            }
            return boxed;
        }

        public abstract bool RequiresContext(IUIContext context, object viewerActionData);

        static bool HasValueChanged<TValue>(ref object boxed, string propertyName, TValue oldValue)
        {
            var newValue = PropertyContainer.GetValue<TValue>(ref boxed, propertyName);
            if (!EqualityComparer<object>.Default.Equals(newValue, oldValue))
                return true;
            return false;
        }

        public static bool SetPropertyValue<T, TValue>(ref T stateData, ref object boxed, string propertyName, TValue newValue)
        {
            if (PropertyContainer.IsPathValid(ref stateData, new PropertyPath(propertyName)))
            {
                var oldValue = PropertyContainer.GetValue<TValue>(ref boxed, propertyName);
                return SetPropertyValue(ref stateData, propertyName, newValue, oldValue);
            }
            return false;
        }

        public static bool SetPropertyValue<T, TValue>(ref T stateData, string propertyName, TValue newValue, TValue oldValue)
        {
            PropertyContainer.SetValue(ref stateData, propertyName, newValue);
            object boxed = stateData;
            return HasValueChanged(ref boxed, propertyName, oldValue);
        }

        public static Payload<IViewerAction> From<T>(object data) where T : ActionBase, new()
            => Payload<IViewerAction>.From(new T(), data);
    }

    public abstract class ActionBase<TStateData> : ActionBase
    {
        public override sealed void ApplyPayload<T>(object actionData, ref T stateData, Action onStateDataChanged)
        {
            object boxed = stateData;
            TStateData stateDataT;
            try
            {
                stateDataT = (TStateData)boxed;
            }
            catch (InvalidCastException e)
            {
                throw new ArgumentException(
                    $"Failed to cast <{stateData?.GetType()?.Name} stateData> to {typeof(TStateData).Name}. " +
                    $"<stateData> argument must implement or be an instance of {typeof(TStateData).Name}." +
                    $" Consider using ActionBase instead of ActionBase<TStateData>", "stateData", e);
            }

            ApplyPayloadToState(actionData, ref stateDataT, out var stateDataChanged);
            boxed = stateDataT;
            stateData = (T)boxed;

            if (stateDataChanged)
                onStateDataChanged?.Invoke();
        }

        protected abstract void ApplyPayloadToState(object actionData, ref TStateData stateData, out bool stateDataChanged);
    }

    public abstract class ActionBase<TActionData, TStateData> : ActionBase
    {
        public override sealed void ApplyPayload<T>(object actionData, ref T stateData, Action onStateDataChanged)
        {
            object boxed = stateData;
            TStateData stateDataT;
            TActionData actionDataT;

            try
            {
                stateDataT = (TStateData)boxed;
            }
            catch (InvalidCastException e)
            {
                throw new ArgumentException(
                    $"Failed to cast <{stateData?.GetType()?.Name} stateData> to {typeof(TStateData).Name}. " +
                    $"<stateData> argument must implement or be an instance of {typeof(TStateData).Name}." +
                    $" Consider using ActionBase instead of ActionBase<TStateData>", "stateData", e);
            }

            try
            {
                actionDataT = (TActionData)actionData;
            }
            catch (InvalidCastException e)
            {
                throw new ArgumentException(
                    $"Failed to cast <{actionData?.GetType()?.Name} actionData> to {typeof(TActionData).Name}. " +
                    $"<actionData> argument must implement or be an instance/value of {typeof(TStateData).Name}." +
                    $" Check arguments or consider using ActionBase/Action<TStateData> instead of ActionBase<TStateData,TActionData>", "actionData", e);
            }

            ApplyPayloadToState(actionDataT, ref stateDataT, out var stateDataChanged);
            boxed = stateDataT;
            stateData = (T)boxed;

            if (stateDataChanged)
                onStateDataChanged?.Invoke();
        }

        protected abstract void ApplyPayloadToState(TActionData actionData, ref TStateData stateData, out bool stateDataChanged);
    }

    /// <summary>
    /// Sets payload data into a specified property of state data
    /// </summary>
    public abstract class SetPropertyAction : ActionBase
    {
        public sealed override void ApplyPayload<T>(object actionData, ref T stateData, Action onStateDataChanged)
        {
            object boxed = stateData;
            if (SetPropertyValue(ref stateData, ref boxed, GetTargetPropertyName(), actionData))
                onStateDataChanged?.Invoke();
        }

        /// <summary>
        /// Returns the name of state data's property that will be updated with action data
        /// </summary>
        /// <returns>Property name</returns>
        protected abstract string GetTargetPropertyName();
    }
}
