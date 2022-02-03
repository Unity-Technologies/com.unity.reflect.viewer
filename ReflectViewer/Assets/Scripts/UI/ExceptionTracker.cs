using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using Unity.Reflect.Runtime;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect;

namespace Unity.Reflect.Viewer
{
    public class ExceptionTracker : MonoBehaviour
    {
        private const string unsetConstant = "<unset>";

        [System.Serializable]
        public class StringUnityEvent : UnityEvent<string> { }

        [System.Serializable]
        public struct ExceptionExtraInfo
        {
            public string deviceUniqueIdentifier;
            public string cloudProvider;
            public string platform;
            public string viewerVersion;
            public string reflectVersion;
            public string userId;
            public string reflectUpid;
            public string EnvTrace;
        }

        [SerializeField]
        private ExceptionExtraInfo m_exceptionExtraInfo;

        IDisposable m_ActiveProjectSelector;

        public StringUnityEvent onExceptionExtraInfoChanged;

        public void Awake()
        {
            m_exceptionExtraInfo.deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier;
            m_exceptionExtraInfo.cloudProvider = LocaleUtils.GetProvider().ToString();
            m_exceptionExtraInfo.platform = Application.platform.ToString();
            m_exceptionExtraInfo.viewerVersion = Application.version;
            m_exceptionExtraInfo.reflectVersion = Assembly.GetAssembly(typeof(UnityProject)).GetName().Version.ToString();
            if (string.IsNullOrEmpty(m_exceptionExtraInfo.EnvTrace))
                m_exceptionExtraInfo.userId = unsetConstant;
            if (string.IsNullOrEmpty(m_exceptionExtraInfo.EnvTrace))
                m_exceptionExtraInfo.reflectUpid = unsetConstant;
            if (string.IsNullOrEmpty(m_exceptionExtraInfo.EnvTrace))
                m_exceptionExtraInfo.EnvTrace = unsetConstant;
            onExceptionExtraInfoChanged?.Invoke(JsonUtility.ToJson(m_exceptionExtraInfo));

            m_ActiveProjectSelector = UISelectorFactory.createSelector<Project>(ProjectManagementContext<Project>.current, nameof(IProjectDataProvider<Project>.activeProject), OnActiveProjectChanged);
        }

        void OnDestroy()
        {
            m_ActiveProjectSelector?.Dispose();
        }

        void OnActiveProjectChanged(Project newData)
        {
            if (newData != null &&
                !string.IsNullOrEmpty(newData.projectId) &&
                newData.projectId != m_exceptionExtraInfo.reflectUpid)
            {
                m_exceptionExtraInfo.reflectUpid = newData.projectId;
                onExceptionExtraInfoChanged?.Invoke(JsonUtility.ToJson(m_exceptionExtraInfo));
            }
        }

        public void SetReflectTrace(string value)
        {
            if (string.IsNullOrEmpty(m_exceptionExtraInfo.EnvTrace))
                m_exceptionExtraInfo.EnvTrace = unsetConstant;
            else
                m_exceptionExtraInfo.EnvTrace = value;
            onExceptionExtraInfoChanged?.Invoke(JsonUtility.ToJson(m_exceptionExtraInfo));
        }

        public void OnUserLogin(UnityUser user)
        {
            if (user != null)
            {
                m_exceptionExtraInfo.userId = user.UserId;
                onExceptionExtraInfoChanged?.Invoke(JsonUtility.ToJson(m_exceptionExtraInfo));
            }
        }

        public void OnUserLogout()
        {
            m_exceptionExtraInfo.userId = unsetConstant;
            onExceptionExtraInfoChanged?.Invoke(JsonUtility.ToJson(m_exceptionExtraInfo));
        }
    }
}
