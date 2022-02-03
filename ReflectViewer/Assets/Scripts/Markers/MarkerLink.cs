using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Reflect.Markers.Camera;
using Unity.Reflect.Markers.Domain.Controller;
using Unity.Reflect.Markers.Storage;
using Unity.Reflect.Viewer.UI;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer
{
    /// <summary>
    /// Generate a link for markers
    /// </summary>
    public class MarkerLink : MonoBehaviour, IProjectLinkSource, IBarcodeDataParser
    {
        [SerializeField]
        MarkerController m_MarkerController;
        [SerializeField]
        ARCardSelectionUIController m_CardSelectionUIController;
        [SerializeField, Tooltip("Reflect Session Manager")]
        LoginManager m_LoginManager;
        [SerializeField]
        MarkerUIPresenter m_MarkerUIPresenter;
        public Uri BaseURI => m_BaseURI;
        Uri m_BaseURI;
        public string Key => k_MarkerKey;
        const string k_MarkerKey = "marker";
        IDisposable m_ProjectSelector;

        IUISelector<AccessToken> m_AccessTokenSelector;
        IUISelector<SetProgressStateAction.ProgressState> m_ProgressStateActionSelector;

        void Awake()
        {
            m_ProjectSelector = UISelectorFactory.createSelector<Project>(ProjectManagementContext<Project>.current, nameof(IProjectDataProvider<Project>.activeProject), ActiveProjectUpdated);
            m_AccessTokenSelector = UISelectorFactory.createSelector<AccessToken>(ProjectManagementContext<Project>.current, nameof(IProjectDataProvider<Project>.accessToken), AccessTokenUpdated);
            m_ProgressStateActionSelector = UISelectorFactory.createSelector<SetProgressStateAction.ProgressState>(ProgressContext.current, nameof(IProgressDataProvider.progressState));
        }

        void Start()
        {
            if (!m_MarkerController)
                m_MarkerController = FindObjectOfType<MarkerController>();
            if (!m_LoginManager)
                m_LoginManager = FindObjectOfType<LoginManager>();
            if (!m_MarkerUIPresenter)
                m_MarkerUIPresenter = FindObjectOfType<MarkerUIPresenter>();
            m_MarkerController.ProjectLinkSource = this;
            m_MarkerController.BarcodeDataParser = this;
            QueryArgHandler.Register(this, k_MarkerKey, SetHandler, GetHandler);
        }

        void OnDestroy()
        {
            QueryArgHandler.Unregister(this);
            m_AccessTokenSelector?.Dispose();
            m_ProjectSelector?.Dispose();
        }

        void SetHandler(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                OpenMarkerBasedARSoon(input);
            }
        }

        string GetHandler()
        {
            if (m_MarkerController.ActiveMarker != null && string.IsNullOrEmpty(m_MarkerController.ActiveMarker.Id.ToString()))
            {
                return m_MarkerController.ActiveMarker.Id.ToString();
            }

            return "";
        }

        void OpenMarkerBasedARSoon(string markerId)
        {
            if (m_MarkerController.Available)
                OpenMarkerBasedAR(markerId);
            else
                m_MarkerController.OnServiceInitialized += OpenWhenAvailable;

            void OpenWhenAvailable(bool available)
            {
                OpenMarkerBasedAR(markerId);
                m_MarkerController.OnServiceInitialized -= OpenWhenAvailable;
            }
        }

        void OpenMarkerBasedAR(string markerId)
        {
            StartCoroutine(OpenMarkerBasedARCoroutine(markerId));
        }

        IEnumerator OpenMarkerBasedARCoroutine(string markerId)
        {
            yield return new WaitUntil(() => m_MarkerController.LoadingComplete);
            yield return new WaitUntil(() => m_ProgressStateActionSelector.GetValue() == SetProgressStateAction.ProgressState.NoPendingRequest);
            m_MarkerController.QueuedMarkerId = markerId;
            m_MarkerUIPresenter.OpenMarkerBasedAR();
        }

        void AccessTokenUpdated(AccessToken current)
        {
            m_BaseURI = current?.UnityProject?.Uri;
        }

        void OnSharingLinkCreated(SharingLinkInfo sharingLinkInfo)
        {
            m_BaseURI = sharingLinkInfo.Uri;

            var linkSharingManager = UIStateManager.current.m_LinkSharingManager;
            linkSharingManager.sharingLinkCreated.RemoveListener(OnSharingLinkCreated);
        }

        void ActiveProjectUpdated(Project current)
        {
            if (string.IsNullOrEmpty(current?.projectId) || UIStateManager.current.projectSettingStateData.accessToken == null)
                return;

            // Fix me: this throws an Exception when user is in Guest mode. 
            var linkSharingManager = UIStateManager.current.m_LinkSharingManager;
            linkSharingManager.sharingLinkCreated.AddListener(OnSharingLinkCreated);
            linkSharingManager.GetSharingLinkInfo(UIStateManager.current.projectSettingStateData.accessToken.CloudServicesAccessToken, current);
        }

        public bool TryParse(string inputData, out IMarker marker)
        {
            // Try to find the marker in the loaded project
            var markerTry = GetProjectMarker(inputData);
            if (markerTry != null)
            {
                marker = markerTry.Value;
                return true;
            }

            // Test for a valid URI
            bool validUri = false;
            try
            {
                Uri uri = new Uri(inputData);
                validUri = true;
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

            // Pass the URI to the deep link handler
            if (validUri)
                StartCoroutine(HandleDeepLink(inputData));
            marker = null;
            return validUri;
        }

        IEnumerator HandleDeepLink(string link)
        {
            // Wait to allow AR mode to turn off, before opening the link.
            yield return null;
            m_LoginManager.onDeeplink(link);
        }

        public string Generate(IMarker marker, UnityProject project)
        {
            var queryArgs = MarkerDeepLinkBarcodeParser.GetQueryArgs(project.Uri);
            queryArgs[Key] = marker.Id.ToString();

            var response = new UriBuilder(project.Uri);
            response.Query = MarkerDeepLinkBarcodeParser.ToQueryString(queryArgs);
            return response.Uri.ToString();
        }

        Marker? GetProjectMarker(string inputDataUri)
        {
            try
            {
                Uri uri = new Uri(inputDataUri);
                Dictionary<string, string> queryArgs = MarkerDeepLinkBarcodeParser.GetQueryArgs(uri);

                var key = m_MarkerController.ProjectLinkSource.Key;

                if (queryArgs.TryGetValue(key, out string value))
                {
                    var markerSuccess = m_MarkerController.MarkerStorage.Get(value);
                    if (markerSuccess != null)
                    {
                        return markerSuccess.Value;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return null;
        }
    }


}
