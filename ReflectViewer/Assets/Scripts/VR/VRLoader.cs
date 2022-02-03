using System;
using System.Collections;
using SharpFlux.Dispatching;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.SceneManagement;

namespace Unity.Reflect.Viewer.UI
{
    /// <summary>
    /// This script is here to help accelerate the coding process in VR
    /// It's allow to load the necessary VR script on play in the editor
    /// </summary>
    public class VRLoader : MonoBehaviour
    {
        IDisposable m_ActiveProjectSelector;

        void Awake()
        {
            m_ActiveProjectSelector = UISelectorFactory.createSelector<Project>(ProjectManagementContext<Project>.current, nameof(IProjectDataProvider<Project>.activeProject), OnActiveProjectChanged);
        }

        void OnDestroy()
        {
            m_ActiveProjectSelector?.Dispose();
        }

        void OnActiveProjectChanged(Project newData)
        {
            if (newData.name != "")
            {
                StartCoroutine(DisableDepthCulling());
            }
        }

        void Start()
        {
            StartCoroutine(LoadAsyncScene("ReflectVR"));
        }

        IEnumerator LoadAsyncScene(string scenePath)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);
            Dispatcher.Dispatch(SetVREnableAction.From(true));
            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }

        IEnumerator DisableDepthCulling()
        {
            yield return new WaitForEndOfFrame();
            Dispatcher.Dispatch(EnableAllNavigationAction.From(true));
            Dispatcher.Dispatch(SetNavigationModeAction.From(SetNavigationModeAction.NavigationMode.VR));
            Dispatcher.Dispatch(SetShowScaleReferenceAction.From(false));
        }
    }
}
