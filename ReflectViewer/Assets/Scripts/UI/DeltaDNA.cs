using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using SharpFlux;
using SharpFlux.Dispatching;
using SharpFlux.Middleware;
using UnityEngine;
using UnityEngine.Reflect;

namespace Unity.Reflect.Viewer.UI
{
    //EventData strange structure is cause by JsonUtility.ToJson that don't handle inheritance
    [Serializable]
    public class EventData
    {
        public string eventName;
        public string userID;
        public string sessionID;
        public string deviceUniqueIdentifier;
        public string cloudProvider;
        public string platform;
        public string viewerVersion;
        public string reflectVersion;
    }

    [Serializable]
    public class EventDataWithEmptyParams : EventData
    {
        public EventParam eventParams;
    }

    [Serializable]
    public class EventDataWithProjectID : EventData
    {
        public EventParamProjectID eventParams;
    }

    [Serializable]
    public class EventDataWithEnabled : EventData
    {
        public EventParamEnabled eventParams;
    }

    [Serializable]
    public class EventParam
    {
    }

    [Serializable]
    public class EventParamProjectID
    {
        public string projectID;
    }

    [Serializable]
    public class EventParamEnabled
    {
        public bool isEnabled;
    }

    public class DeltaDNA : MonoBehaviour, IMiddleware<Payload<ActionTypes>>
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private static string ddnaUrl;
#pragma warning disable CS0649
        [SerializeField] private string url;
#pragma warning restore CS0649
        private string userId;

        private void Start()
        {
            var assemblyVersion = Assembly.GetEntryAssembly()?.GetName().Version.ToString();

            ddnaUrl = url;

            if (url != string.Empty && Uri.IsWellFormedUriString(ddnaUrl, UriKind.Absolute))
            {
                UIStateManager.sessionStateChanged += OnSessionStateDataChanged;
                Dispatcher.RegisterMiddleware(this);
            }
        }

        public bool Apply(ref Payload<ActionTypes> payload)
        {
            var proceedToInvocation = true;

            switch (payload.ActionType)
            {
                case ActionTypes.OpenProject:
                {
                    var projectData = (UIProjectStateData) payload.Data;
                    TrackViewerOpenProject(UIStateManager.current.sessionStateData.sessionState.user?.UserId,
                        projectData.activeProject.projectId);
                    break;
                }
                case ActionTypes.SetSync:
                {
                    var projectData = (bool) payload.Data;
                    TrackViewerSyncEnabled(UIStateManager.current.sessionStateData.sessionState.user?.UserId,
                        projectData);
                    break;
                }
                case ActionTypes.Login:
                case ActionTypes.Logout:
                case ActionTypes.SetToolState:
                case ActionTypes.OpenDialog:
                case ActionTypes.CloseAllDialogs:
                case ActionTypes.SetStatusMessage:
                case ActionTypes.ClearStatus:
                case ActionTypes.Failure:
                    break;
            }

            return proceedToInvocation;
        }

        private void OnSessionStateDataChanged(UISessionStateData obj)
        {
            var uid = obj.sessionState.user?.UserId;
            if (obj.sessionState.loggedState == LoginState.LoggedIn && uid != userId)
            {
                TrackViewerLoaded(uid);
                userId = uid;
            }
        }

        private static void SendEvent(EventData body)
        {
            var json = JsonUtility.ToJson(body);

            SendEvent(json);
        }

        private static void SendEvent(EventDataWithEmptyParams body)
        {
            var json = JsonUtility.ToJson(body);

            SendEvent(json);
        }

        private static void SendEvent(EventDataWithProjectID body)
        {
            var json = JsonUtility.ToJson(body);

            SendEvent(json);
        }

        private static void SendEvent(EventDataWithEnabled body)
        {
            var json = JsonUtility.ToJson(body);

            SendEvent(json);
        }

        private static async void SendEvent(string json)
        {
            Debug.Log(json);
            using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
            {
                var res = await httpClient.PostAsync(ddnaUrl, content).ConfigureAwait(false);
                if (res.StatusCode != HttpStatusCode.NoContent)
                    Debug.Log($"Failed to send event to DeltaDNA. Reason: {res.ReasonPhrase}");
            }
        }

        public static void TrackViewerLoaded(string userId, string sessionId = "")
        {
            var payload = new EventDataWithEmptyParams
            {
                eventName = "reflectViewerLoaded",
                userID = userId,
                sessionID = sessionId,
                deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier,
                cloudProvider = LocaleUtils.GetProvider().ToString(),
                platform = Application.platform.ToString(),
                viewerVersion = Application.version,
                reflectVersion = Assembly.GetAssembly(typeof(UnityProject)).GetName().Version.ToString(),
            };

            SendEvent(payload);
        }

        public static void TrackViewerOpenProject(string userId, string projectId, string sessionId = "")
        {
            var payload = new EventDataWithProjectID
            {
                eventName = "reflectViewerOpenProject",
                userID = userId,
                sessionID = sessionId,
                deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier,
                cloudProvider = LocaleUtils.GetProvider().ToString(),
                platform = Application.platform.ToString(),
                viewerVersion = Application.version,
                reflectVersion = Assembly.GetAssembly(typeof(UnityProject)).GetName().Version.ToString(),
                eventParams = new EventParamProjectID
                {
                    projectID = projectId
                }
            };

            SendEvent(payload);
        }

        public static void TrackViewerSyncEnabled(string userId, bool isEnabled, string sessionId = "")
        {
            var payload = new EventDataWithEnabled
            {
                eventName = "reflectViewerSyncEnabled",
                userID = userId,
                sessionID = sessionId,
                deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier,
                cloudProvider = LocaleUtils.GetProvider().ToString(),
                platform = Application.platform.ToString(),
                viewerVersion = Application.version,
                reflectVersion = Assembly.GetAssembly(typeof(UnityProject)).GetName().Version.ToString(),
                eventParams = new EventParamEnabled
                {
                    isEnabled = isEnabled
                }
            };

            SendEvent(payload);
        }
    }
}
