using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SharpFlux;
using SharpFlux.Middleware;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    public class DeltaDNA : MonoBehaviour, IMiddleware<Payload<ActionTypes>>
    {
        string userId;
#pragma warning disable CS0649
        [SerializeField]
        private string url;
#pragma warning restore CS0649
        static readonly HttpClient httpClient = new HttpClient();
        static string ddnaUrl;

        void Start()
        {
            var assemblyVersion = Assembly.GetEntryAssembly()?.GetName().Version.ToString();
            
            ddnaUrl = url;

            if (url != string.Empty && Uri.IsWellFormedUriString(ddnaUrl, UriKind.Absolute))
            {
                UIStateManager.sessionStateChanged += OnSessionStateDataChanged;
                UIStateManager.current.Dispatcher.RegisterMiddleware(this);
            }
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

        static async void SendEvent(object body)
        {
            var json = JsonUtility.ToJson(body);
            using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
            {
                var res = await httpClient.PostAsync(ddnaUrl, content).ConfigureAwait(false);
                if (res.StatusCode != HttpStatusCode.NoContent)
                {
                    Debug.Log($"Failed to send event to DeltaDNA. Reason: {res.ReasonPhrase}");
                }
            }
        }

        public static void TrackViewerLoaded(string userId, string sessionId = "")
        {
            var payload = new
            {
                eventName = "reflectViewerLoaded",
                userID = userId,
                sessionID = sessionId
            };

            SendEvent(payload);
        }

        public static void TrackViewerOpenProject(string userId, string projectId, string sessionId = "")
        {
            var payload = new
            {
                eventName = "reflectViewerOpenProject",
                userID = userId,
                sessionID = sessionId,
                eventParams = new
                {
                    projectId
                }
            };

            SendEvent(payload);
        }

        public static void TrackViewerSyncEnabled(string userId, string projectId, string sessionId = "")
        {
            var payload = new
            {
                eventName = "reflectViewerSyncEnabled",
                userID = userId,
                sessionID = sessionId,
                eventParams = new
                {
                    projectId
                }
            };

            SendEvent(payload);
        }

        public bool Apply(ref Payload<ActionTypes> payload)
        {
            bool proceedToInvocation = true;

            switch (payload.ActionType)
            {
                case ActionTypes.OpenProject:
                {
                    UIProjectStateData projectData = (UIProjectStateData)payload.Data;
                    TrackViewerOpenProject(UIStateManager.current.sessionStateData.sessionState.user?.UserId,
                        projectData.activeProject.projectId);
                    break;
                 }
                case ActionTypes.Login:
                case ActionTypes.Logout:
                case ActionTypes.SetToolState:
                case ActionTypes.OpenDialog:
                case ActionTypes.CloseAllDialogs:
                case ActionTypes.SetStatus:
                case ActionTypes.ClearStatus:
                case ActionTypes.Failure:
                    break;
            }

            return proceedToInvocation;
        }
    }
}
