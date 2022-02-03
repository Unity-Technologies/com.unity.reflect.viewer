using System;
using System.Collections.Generic;
using SharpFlux;
using SharpFlux.Dispatching;
using SharpFlux.Stores;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;

namespace UnityEngine.Reflect.Viewer.Example
{
    [Serializable, GeneratePropertyBag]
    public struct UIApplicationState
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public string qualityLevel { get; set; }
    }

    [Serializable, GeneratePropertyBag]
    struct UIDebugState
    {

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool gesturesTrackingEnabled { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool ARAxisTrackingEnabled { get; set; }
    }

    public class ExampleApplication: MonoBehaviour,
        IStore<UIApplicationState>, IStore<UIDebugState>,
        IContextPropertyProvider
    {
        [SerializeField, Tooltip("State of the UI"), UIContextProperties(nameof(ApplicationContext))]
        [ContextButton("Value Changed",nameof(OnApplicationContextChanged))]
        UIApplicationState m_UIApplicationState;

        [SerializeField, Tooltip("State of the Debugging Info"), UIContextProperties(nameof(DebugOptionContext))]
        [ContextButton("Value Changed",nameof(OnDebugContextChanged))]
        UIDebugState m_UIDebugState;

        UIApplicationState IStore<UIApplicationState>.Data => m_UIApplicationState;
        UIDebugState IStore<UIDebugState>.Data => m_UIDebugState;

        IContextTarget m_ApplicationContextTarget;
        IContextTarget m_DebugContextTarget;

        IDispatcher m_Dispatcher;
        readonly object syncRoot = new object();
        public string DispatchToken { get; private set; }
        public bool HasChanged { get; private set; }

        public void Awake()
        {
            m_Dispatcher = DispatcherFactory.GetDispatcher();

            m_ApplicationContextTarget = ApplicationContext.BindTarget(m_UIApplicationState);
            m_DebugContextTarget = DebugOptionContext.BindTarget(m_UIDebugState);

            DispatchToken = m_Dispatcher.Register<Payload<IViewerAction>>(InvokeOnDispatch);
        }

        void InvokeOnDispatch(Payload<IViewerAction> viewerAction)
        {
            HasChanged = false;

            lock (syncRoot)
            {
                m_UIApplicationState = TryApplyPayload(viewerAction, ApplicationContext.current, m_UIApplicationState, m_ApplicationContextTarget);
                m_UIDebugState = TryApplyPayload(viewerAction, DebugOptionContext.current, m_UIDebugState, m_DebugContextTarget);
            }
        }

        private T TryApplyPayload<T>(Payload<IViewerAction> viewerAction, IUIContext context, T stateData, IContextTarget contextTarget)
        {
            if (viewerAction.ActionType.RequiresContext(context, viewerAction.Data))
            {
                viewerAction.ActionType.ApplyPayload(viewerAction.Data, ref stateData, () =>
                {
                    contextTarget.UpdateWith(ref stateData);
                    HasChanged = true;
                });
            }

            return stateData;
        }


        void OnApplicationContextChanged()
        {
            m_ApplicationContextTarget.UpdateWith(ref m_UIApplicationState);
        }

        void OnDebugContextChanged()
        {
            m_DebugContextTarget.UpdateWith(ref m_UIDebugState);
        }

        public IEnumerable<IContextPropertyProvider.ContextPropertyData> GetProperties()
        {
            var properties = new List<IContextPropertyProvider.ContextPropertyData>();

            properties.AddRange(GetProperties<UIApplicationState, ApplicationContext>());
            properties.AddRange(GetProperties<UIDebugState, DebugOptionContext>());

            return properties;
        }

        IEnumerable<IContextPropertyProvider.ContextPropertyData> GetProperties<TStateData, TContextType>() where TContextType : IUIContext
        {
            var propertyBag = PropertyBag.GetPropertyBag(typeof(TStateData)) as PropertyBag<TStateData>;
            foreach (var property in propertyBag.GetProperties())
            {
                var data = new IContextPropertyProvider.ContextPropertyData() { context = typeof(TContextType), property = property };
                yield return data;
            }
        }
    }
}



