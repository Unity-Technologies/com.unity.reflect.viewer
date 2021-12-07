using System;
using System.Collections.Generic;
using System.Linq;
using SharpFlux;
using SharpFlux.Dispatching;
using SharpFlux.Middleware;
using UnityEngine;
using UnityEngine.Reflect;


namespace Unity.Reflect.Viewer.UI
{
    //EventData strange structure is cause by JsonUtility.ToJson that don't handle inheritance
    public class DeltaDNA : MonoBehaviour//, IMiddleware<Payload<ActionTypes>>
    {
#pragma warning disable CS0649
        [SerializeField]
        protected string url; //http://localhost:1880/deltaDNA
#pragma warning restore CS0649
        UISessionStateData m_CacheUISessionStateData;
        void Start()
        {
            DeltaDNARequest.Instance.SetURL(url);

            if (url != string.Empty && Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                //UIStateManager.sessionStateChanged += OnSessionStateDataChanged;
                //Dispatcher.RegisterMiddleware(this);
            }
        }

/*
        public bool Apply(ref Payload<ActionTypes> payload)
        {
            var proceedToInvocation = true;

            switch (payload.ActionType)
            {
                case ActionTypes.OpenProject:
                {
                    var project = (Project) payload.Data;
                    DeltaDNARequest.Instance.TrackViewerOpenProject(UIStateManager.current.sessionStateData.sessionState.user?.UserId,
                        project.projectId);
                    break;
                }
                case ActionTypes.SetSync:
                {
                    var syncEnabled = (bool) payload.Data;
                    DeltaDNARequest.Instance.TrackViewerSyncEnabled(UIStateManager.current.sessionStateData.sessionState.user?.UserId,
                        syncEnabled);
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
*/

        void OnSessionStateDataChanged(UISessionStateData obj)
        {
            if (m_CacheUISessionStateData == obj)
                return;
/*
            var uid = obj.sessionState.user?.UserId;
            if (obj.sessionState.loggedState == LoginState.LoggedIn && uid != DeltaDNARequest.Instance.userId)
            {
                DeltaDNARequest.Instance.TrackViewerLoaded(uid);
                DeltaDNARequest.Instance.userId = uid;
            }

            if (obj.sessionState.isOpenWithLinkSharing && !m_CacheUISessionStateData.sessionState.isOpenWithLinkSharing)
            {
                DeltaDNARequest.Instance.TrackEvent(uid, DeltaDNARequest.shareLinkOpen);
            }

            if (!string.IsNullOrEmpty(uid) &&
                !ListEquals(obj.sessionState.dnaLicenseInfo.entitlements, m_CacheUISessionStateData.sessionState.dnaLicenseInfo.entitlements))
            {
                DeltaDNARequest.Instance.TrackUserLicence(uid,obj.sessionState.dnaLicenseInfo.floatingSeat != TimeSpan.Zero,
                    string.Join(",", obj.sessionState.dnaLicenseInfo.entitlements));
            }
*/
            m_CacheUISessionStateData = obj;
        }

        public bool ListEquals<T>(List<T> list1, List<T> List2)
        {
            if (list1 == null || List2 == null) return false;
            return list1.SequenceEqual(List2);
        }
    }
}
