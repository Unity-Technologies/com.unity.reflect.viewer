using System;
using System.Reflection;
using SharpFlux;
using SharpFlux.Middleware;
using Unity.Reflect.Viewer.UI;
using UnityEngine;
using UnityEngine.Events;
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

            UIStateManager.projectStateChanged += CheckActiveProject;
        }

        private void CheckActiveProject(UIProjectStateData obj)
        {
            if (obj.activeProject != null &&
                !string.IsNullOrEmpty(obj.activeProject.projectId) &&
                obj.activeProject.projectId != m_exceptionExtraInfo.reflectUpid)
            {
                m_exceptionExtraInfo.reflectUpid = obj.activeProject.projectId;
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
