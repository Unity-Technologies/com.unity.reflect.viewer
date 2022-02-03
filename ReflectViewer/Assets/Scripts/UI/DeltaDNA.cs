using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    //EventData strange structure is cause by JsonUtility.ToJson that don't handle inheritance
    public class DeltaDNA : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        protected string url; //http://localhost:1880/deltaDNA
#pragma warning restore CS0649
        IUISelector<UnityUser> m_UserSelector;
        IUISelector<SetDialogModeAction.DialogMode> m_DialogModeSelector;
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();
        string m_FilterCache = "";
        DeltaDNARequest m_DeltaDnaRequest = new DeltaDNARequest();

        void OnDestroy()
        {
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        void Awake()
        {
            m_DisposeOnDestroy.Add(m_DialogModeSelector = UISelectorFactory.createSelector<SetDialogModeAction.DialogMode>(UIStateContext.current, nameof(IDialogDataProvider.dialogMode), OnDialogModeChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<Project>(ProjectManagementContext<Project>.current, nameof(IProjectDataProvider<Project>.activeProject), OnActiveProjectChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<LoginState>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.loggedState), OnLoggedStateDataChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.isOpenWithLinkSharing), OnOpenWithLinkSharingChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(UIStateContext.current, nameof(ISyncModeDataProvider.syncEnabled), OnSyncEnable));
            m_DisposeOnDestroy.Add(m_UserSelector = UISelectorFactory.createSelector<UnityUser>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.user)));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<DNALicenseInfo>(DeltaDNAContext.current, nameof(IDeltaDNADataProvider.dnaLicenseInfo), OnLicenseInfoChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<string>(DeltaDNAContext.current, nameof(IDeltaDNADataProvider.buttonName), OnButtonEvent));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeDialog), OnActiveDialogChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeSubDialog), OnActiveDialogChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<string>(ProjectContext.current, nameof(IProjectSortDataProvider.filterGroup), OnFilterGroupChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<SetHelpModeIDAction.HelpModeEntryID>(UIStateContext.current, nameof(IHelpModeDataProvider.helpModeEntryId), OnHelpModeEntryChanged));
        }

        void OnHelpModeEntryChanged(SetHelpModeIDAction.HelpModeEntryID id)
        {
            if(id != SetHelpModeIDAction.HelpModeEntryID.None)
                m_DeltaDnaRequest.TrackButtonEvent(m_DeltaDnaRequest.userId, $"HelpMode_{id.ToString()}");
        }

        void OnDialogModeChanged(SetDialogModeAction.DialogMode obj)
        {
            if (obj == SetDialogModeAction.DialogMode.Help)
                m_DeltaDnaRequest.TrackButtonEvent(m_DeltaDnaRequest.userId, "HelpModeEnter");
        }

        void OnFilterGroupChanged(string obj)
        {
            if (!string.IsNullOrEmpty(obj) && !string.IsNullOrEmpty(m_FilterCache))
            {
                OnButtonEvent($"BimFilter_{obj}");
            }

            m_FilterCache = obj;
        }

        void OnActiveDialogChanged(OpenDialogAction.DialogType obj)
        {
            if (obj != OpenDialogAction.DialogType.None)
            {
                m_DeltaDnaRequest.TrackButtonEvent(m_DeltaDnaRequest.userId,
                    m_DialogModeSelector.GetValue() == SetDialogModeAction.DialogMode.Help ? $"HelpMode_{obj}" : $"ButtonType_{obj}");
            }
        }

        void OnButtonEvent(string name)
        {
            if (m_DialogModeSelector.GetValue() != SetDialogModeAction.DialogMode.Help)
            {
                m_DeltaDnaRequest.TrackButtonEvent(m_DeltaDnaRequest.userId, name);
            }
        }

        void OnLicenseInfoChanged(DNALicenseInfo dnaLicenseInfo)
        {
            if (!string.IsNullOrEmpty(m_DeltaDnaRequest.userId))
            {
                m_DeltaDnaRequest.TrackUserLicence(m_DeltaDnaRequest.userId, dnaLicenseInfo.floatingSeat != TimeSpan.Zero,
                    string.Join(",", dnaLicenseInfo.entitlements));
            }
        }

        void OnOpenWithLinkSharingChanged(bool isShareLinkOpen)
        {
            if (isShareLinkOpen)
                m_DeltaDnaRequest.TrackEvent(m_DeltaDnaRequest.userId, DeltaDNARequest.shareLinkOpen);
        }

        void OnLoggedStateDataChanged(LoginState loggedState)
        {
            if (loggedState == LoginState.LoggedIn && m_UserSelector.GetValue()?.UserId != m_DeltaDnaRequest.userId)
            {
                m_DeltaDnaRequest.TrackViewerLoaded(m_UserSelector.GetValue().UserId);
                m_DeltaDnaRequest.userId = m_UserSelector.GetValue().UserId;
            }
        }

        void OnSyncEnable(bool syncEnabled)
        {
            m_DeltaDnaRequest.TrackViewerSyncEnabled(m_DeltaDnaRequest.userId, syncEnabled);
        }

        void OnActiveProjectChanged(Project project)
        {
            if(project != null)
                m_DeltaDnaRequest.TrackViewerOpenProject(m_DeltaDnaRequest?.userId, project.projectId);
        }

        void Start()
        {
            m_DeltaDnaRequest.SetURL(url);
        }

        public bool ListEquals<T>(List<T> list1, List<T> List2)
        {
            if (list1 == null || List2 == null) return false;
            return list1.SequenceEqual(List2);
        }
    }
}
