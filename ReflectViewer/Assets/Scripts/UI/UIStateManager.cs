using System.Collections;
using System.Collections.Generic;
using SharpFlux.Stores;
using Unity.MARS.Providers;
using UnityEngine;
using UnityEngine.Reflect.Viewer;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{

    /// <summary>
    /// Component that hold the state of the UI.
    /// Partial Class with class definitions
    /// </summary>
    public partial class UIStateManager : MonoBehaviour,
        IStore<UIStateData>, IStore<UISessionStateData>, IStore<UIProjectStateData>, IStore<ProjectSettingStateData>, IStore<UIARStateData>, IStore<UIDebugStateData>, IStore<ApplicationSettingsStateData>,
        IStore<RoomConnectionStateData>, IStore<ExternalToolStateData>, IStore<UIWalkStateData>, IStore<DragStateData>, IStore<PipelineStateData>, IStore<ForceNavigationModeData>,
        IStore<AppBarStateData>, IUsesSessionControl, IUsesPointCloud, IUsesPlaneFinding, IStore<SceneOptionData>, IStore<VRStateData>
    {

        static UIStateManager s_Current;

        public static UIStateManager current => s_Current;

#if UNITY_EDITOR
        public string StartUpLink;
#endif

        bool m_Initialized;

        Dictionary<SetNavigationModeAction.NavigationMode, string> m_SceneDictionary = new Dictionary<SetNavigationModeAction.NavigationMode, string>();

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

            foreach (var info in m_UIStateData.navigationStateData.navigationModeInfos)
            {
                m_SceneDictionary[info.navigationMode] = info.modeScenePath;
            }
        }

        void OnDestroy()
        {
            s_Current = null;
            m_TeleportSelector?.Dispose();
            ReflectProjectsManager.Dispose();
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        IEnumerator Start()
        {
            yield return StartPipeline();

            if (s_Current == this)
            {
                m_UISessionStateData.networkReachability = Application.internetReachability;
#if UNITY_EDITOR
                if (!string.IsNullOrEmpty(StartUpLink))
                {
                    m_LoginManager.onDeeplink(StartUpLink);
                }
#else
                m_LoginManager.onDeeplink(m_ArgumentParser.TrailingArg);
#endif
                StartCoroutine(InternetCheck());


                foreach (var info in m_UIStateData.navigationStateData.navigationModeInfos)
                {
                    m_SceneDictionary[info.navigationMode] = info.modeScenePath;
                }
            }
            m_Initialized = true;
        }

        IEnumerator InternetCheck()
        {
            while (s_Current == this)
            {
                yield return new WaitForSeconds(5);
                StartCoroutine(UnityEngine.Reflect.ProjectServer.CheckProjectServerConnection(ConnectionCheck));
                NetworkReachabilityCheck();
            }
        }

        void ConnectionCheck(bool connection)
        {
            if (m_UISessionStateData.projectServerConnection != connection)
            {
                Debug.Log($"projectServerConnectionChanged = {connection}");
                m_UISessionStateData.projectServerConnection = connection;
                ForceSendSessionStateChangedEvent();
            }
        }

        void NetworkReachabilityCheck()
        {
            if (m_UISessionStateData.networkReachability != Application.internetReachability)
            {
                Debug.Log($"networkReachabilityChanged = {Application.internetReachability}");
                m_UISessionStateData.networkReachability = Application.internetReachability;
                ForceSendSessionStateChangedEvent();
            }
        }

        void DetectCapabilities()
        {
            m_PipelineStateData.deviceCapability = SetVREnableAction.DeviceCapability.None;
#if UNITY_EDITOR
            m_PipelineStateData.deviceCapability |= SetVREnableAction.DeviceCapability.VRCapability | SetVREnableAction.DeviceCapability.ARCapability;
#elif UNITY_IOS || UNITY_ANDROID
            m_PipelineStateData.deviceCapability |= SetVREnableAction.DeviceCapability.ARCapability;
#elif UNITY_STANDALONE_WIN
            m_PipelineStateData.deviceCapability |= SetVREnableAction.DeviceCapability.VRCapability;
#endif
            if (SystemInfo.supportsAsyncGPUReadback)
                m_PipelineStateData.deviceCapability |= SetVREnableAction.DeviceCapability.SupportsAsyncGPUReadback;
        }



        public void ForceSendSessionStateChangedEvent()
        {
            m_SessionStateContextTarget.UpdateWith(ref m_UISessionStateData, UpdateNotification.ForceNotify);
        }

        public Dictionary<SetNavigationModeAction.NavigationMode, string> GetSceneDictionary()
        {
            return m_SceneDictionary;
        }

    }
}
