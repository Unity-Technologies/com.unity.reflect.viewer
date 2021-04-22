using System;
using System.Collections;
using SharpFlux.Stores;
using Unity.MARS.Providers;
using UnityEngine;
using UnityEngine.Reflect.MeasureTool;

namespace Unity.Reflect.Viewer.UI
{

    /// <summary>
    /// Component that hold the state of the UI.
    /// Partial Class with class definitions
    /// </summary>
    public partial class UIStateManager : MonoBehaviour,
        IStore<UIStateData>, IStore<UISessionStateData>, IStore<UIProjectStateData>, IStore<UIARStateData>, IStore<UIDebugStateData>, IStore<ApplicationStateData>,
        IStore<RoomConnectionStateData>,
        IUsesSessionControl, IUsesPointCloud, IUsesPlaneFinding
    {

        static UIStateManager s_Current;

        public static UIStateManager current => s_Current;

        void Awake()
        {
            /// TODO: pseudo singleton will be deleted
            if (s_Current == null)
            {
                s_Current = this;
            }

            DetectCapabilities();
            AwakeActions();
            AwakePipeline();
            AwakeMultiplayer();
        }

        void OnDestroy()
        {
            stateChanged = delegate {};
            sessionStateChanged = delegate {};
            projectStateChanged = delegate {};
            arStateChanged = delegate {};
            debugStateChanged = delegate {};
            applicationStateChanged = delegate {};
            roomConnectionStateChanged = delegate {};
            externalToolChanged = delegate {};
            s_Current = null;
        }

        IEnumerator Start()
        {
            yield return StartPipeline();

            if (s_Current == this)
            {
                OnAppStateChanged();
                OnSessionStateChanged();
                OnProjectStateChanged();
                OnARStateChanged();
                OnExternalToolStateChanged();

                m_LoginManager.onDeeplink(m_ArgumentParser.TrailingArg);
            }
        }

        void DetectCapabilities()
        {
            m_UIStateData.deviceCapability = DeviceCapability.None;
#if UNITY_EDITOR
            m_UIStateData.deviceCapability |= DeviceCapability.VRCapability | DeviceCapability.ARCapability;
#elif UNITY_IOS || UNITY_ANDROID
            m_UIStateData.deviceCapability |= DeviceCapability.ARCapability;
#elif UNITY_STANDALONE_WIN
            m_UIStateData.deviceCapability |= DeviceCapability.VRCapability;
#endif
            if (SystemInfo.supportsAsyncGPUReadback)
                m_UIStateData.deviceCapability |= DeviceCapability.SupportsAsyncGPUReadback;
        }

        void OnAppStateChanged()
        {
            ForceSendStateChangedEvent();
        }

        void OnSessionStateChanged()
        {

        }

        void OnProjectStateChanged()
        {
            ForceSendProjectStateChangedEvent();
        }

        void OnARStateChanged()
        {
            ForceSendARStateChangedEvent();
        }

        void OnExternalToolStateChanged()
        {
            ForceExternalToolChangedEvent();
        }

        /// <summary>
        /// Invoke model changed event.
        /// </summary>
        public void ForceSendStateChangedEvent()
        {
            stateChanged.Invoke(m_UIStateData);
        }
        /// <summary>
        /// Invoke model changed event.
        /// </summary>
        public void ForceSendSessionStateChangedEvent()
        {
            sessionStateChanged.Invoke(m_UISessionStateData);
        }

        /// <summary>
        /// Invoke Project changed event.
        /// </summary>
        public void ForceSendProjectStateChangedEvent()
        {
            projectStateChanged.Invoke(m_UIProjectStateData);
        }

        /// <summary>
        /// Invoke AR Simulation changed event.
        /// </summary>
        public void ForceSendARStateChangedEvent()
        {
            arStateChanged.Invoke(m_ARStateData);
        }

        /// <summary>
        /// Invoke application state changed event.
        /// </summary>
        public void ForceSendApplicationChangedEvent()
        {
            applicationStateChanged.Invoke(m_ApplicationStateData);
        }

        /// <summary>
        /// Invoke Connection state changed event.
        /// </summary>
        public void ForceSendConnectionChangedEvent()
        {
            roomConnectionStateChanged.Invoke(m_RoomConnectionStateData);
		}

        /// <summary>
        /// Invoke Tool changed event.
        /// </summary>
        public void ForceExternalToolChangedEvent()
        {
            externalToolChanged.Invoke(m_ExternalToolStateData);
        }
    }
}
