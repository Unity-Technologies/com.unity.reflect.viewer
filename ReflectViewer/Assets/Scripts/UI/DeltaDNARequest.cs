using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using Unity.Reflect.Runtime;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    //Classes strange structure is cause by JsonUtility.ToJson that don't handle well inheritance
    [Serializable]
    public class EventData<T>
    {
        public string eventName;
        public string userID;
        public string sessionID;

        public T eventParams;
    }

    [Serializable]
    public struct DNALicenseInfo
    {
        public TimeSpan floatingSeat;
        public List<string> entitlements;
    }

    [Serializable]
    public class EventParam
    {
        public string deviceUniqueIdentifier;
        public string cloudProvider;
        public string platform;
        public string viewerVersion;
        public string reflectVersion;
    }

    [Serializable]
    public class EventParamProjectID : EventParam
    {
        public string projectID;
    }

    [Serializable]
    public class EventLicence : EventParam
    {
        public string licenceType;
        public bool isFloating;
    }

    [Serializable]
    public class EventParamEnabled : EventParam
    {
        public bool isEnabled;
    }

    [Serializable]
    public class EventButtonClicked : EventParam
    {
        public string buttonClicked;
    }

    public class DeltaDNARequest
    {
        static readonly DeltaDNARequest s_Instance = new DeltaDNARequest();
        readonly HttpClient m_HttpClient = new HttpClient();
        public string userId;
        public const string shareLinkOpen = "reflectSharedLinkOpened";
        string m_DDNAUrl;

        DeltaDNARequest() { }

        public static DeltaDNARequest Instance => s_Instance;

        public void SetURL(string url)
        {
            m_DDNAUrl = url;
        }

        void SendEvent<T>(EventData<T> body)
        {
            var json = JsonUtility.ToJson(body);

            if (Uri.IsWellFormedUriString(m_DDNAUrl, UriKind.Absolute))
                SendEvent(json);
        }

        async void SendEvent(string json)
        {
            using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
            {
                var res = await m_HttpClient.PostAsync(m_DDNAUrl, content).ConfigureAwait(false);
                if (res.StatusCode != HttpStatusCode.NoContent && res.StatusCode != HttpStatusCode.OK)
                    Debug.Log($"Failed to send event to DeltaDNA. Reason: {res.ReasonPhrase}");
            }
        }

        public void TrackViewerLoaded(string userId, string sessionId = "")
        {
            var payload = new EventData<EventParam>
            {
                eventName = "reflectViewerLoaded",
                userID = userId,
                sessionID = sessionId,
                eventParams = new EventParam
                {
                    deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier,
                    cloudProvider = LocaleUtils.GetProvider().ToString(),
                    platform = Application.platform.ToString(),
                    viewerVersion = Application.version,
                    reflectVersion = Assembly.GetAssembly(typeof(UnityProject)).GetName().Version.ToString()
                }
            };

            SendEvent(payload);
        }

        public void TrackButtonEvent(string userId, string buttonClickedName, string sessionId = "")
        {
            var payload = new EventData<EventButtonClicked>()
            {
                eventName = "reflectButtonClicked",
                userID = userId,
                sessionID = sessionId,
                eventParams = new EventButtonClicked()
                {
                    buttonClicked = buttonClickedName,
                    deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier,
                    cloudProvider = LocaleUtils.GetProvider().ToString(),
                    platform = Application.platform.ToString(),
                    viewerVersion = Application.version,
                    reflectVersion = Assembly.GetAssembly(typeof(UnityProject)).GetName().Version.ToString()
                }
            };
            SendEvent(payload);
        }

        public void TrackEvent(string userId, string eventName, string sessionId = "")
        {
            var payload = new EventData<EventParam>
            {
                eventName = eventName,
                userID = userId,
                sessionID = sessionId,
                eventParams = new EventParam
                {
                    deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier,
                    cloudProvider = LocaleUtils.GetProvider().ToString(),
                    platform = Application.platform.ToString(),
                    viewerVersion = Application.version,
                    reflectVersion = Assembly.GetAssembly(typeof(UnityProject)).GetName().Version.ToString()
                }
            };

            SendEvent(payload);
        }

        public void TrackViewerOpenProject(string userId, string projectId, string sessionId = "")
        {
            var payload = new EventData<EventParamProjectID>()
            {
                eventName = "reflectViewerOpenProject",
                userID = userId,
                sessionID = sessionId,
                eventParams = new EventParamProjectID()
                {
                    projectID = projectId,
                    deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier,
                    cloudProvider = LocaleUtils.GetProvider().ToString(),
                    platform = Application.platform.ToString(),
                    viewerVersion = Application.version,
                    reflectVersion = Assembly.GetAssembly(typeof(UnityProject)).GetName().Version.ToString()
                }
            };

            SendEvent(payload);
        }

        public void TrackViewerSyncEnabled(string userId, bool isEnabled, string sessionId = "")
        {
            var payload = new EventData<EventParamEnabled>()
            {
                eventName = "reflectViewerSyncEnabled",
                userID = userId,
                sessionID = sessionId,
                eventParams = new EventParamEnabled
                {
                    isEnabled = isEnabled,
                    deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier,
                    cloudProvider = LocaleUtils.GetProvider().ToString(),
                    platform = Application.platform.ToString(),
                    viewerVersion = Application.version,
                    reflectVersion = Assembly.GetAssembly(typeof(UnityProject)).GetName().Version.ToString(),
                }
            };
            SendEvent(payload);
        }

        public void TrackUserLicence(string userId, bool isFloating, string licenceType, string sessionId = "")
        {
            var payload = new EventData<EventLicence>()
            {
                eventName = "reflectUserLicenceUsed",
                userID = userId,
                sessionID = sessionId,
                eventParams = new EventLicence()
                {
                    isFloating = isFloating,
                    licenceType = licenceType,
                    deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier,
                    cloudProvider = LocaleUtils.GetProvider().ToString(),
                    platform = Application.platform.ToString(),
                    viewerVersion = Application.version,
                    reflectVersion = Assembly.GetAssembly(typeof(UnityProject)).GetName().Version.ToString(),
                }
            };

            SendEvent(payload);
        }
    }
}
