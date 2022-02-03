using System;
using System.Collections.Generic;
using System.Linq;
using Unity.TouchFramework;
using SharpFlux.Dispatching;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Viewer;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(DialogWindow))]
    public class LinkSharingUIController: MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        ToolButton m_DialogButton;
        [SerializeField]
        Button m_CopyLinkButton;
        [SerializeField]
        SlideToggle m_PublicLinkToggle;
        [SerializeField]
        TMP_Text m_PublicLinkToggleText;
        [SerializeField]
        TMP_Text m_LinkAccessMessage;
        [SerializeField]
        RectTransform m_Arrow;
        [SerializeField]
        RectTransform m_Container;
#pragma warning restore CS0649

        DialogWindow m_DialogWindow;
        LinkPermission m_currentPermission;
        bool m_IsToggleEnabled = true;
        IUISelector<Project> m_ActiveProjectSelector;
        bool m_ButtonVisibility;
        IUISelector<IProjectRoom[]> m_ProjectRoomSelector;
        IUISelector<UnityUser> m_UnityUserSelector;
        IUISelector<LinkPermission> m_LinkSharingSelector;
        IUISelector<OpenDialogAction.DialogType> m_ActiveDialogSelector;
        IUISelector<bool> m_VREnableSelector;
        bool m_Interactable;
        IUISelector<bool> m_ToolBarEnabledSelector;
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        void OnDestroy()
        {
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        void Awake()
        {
            m_DisposeOnDestroy.Add(m_ActiveProjectSelector = UISelectorFactory.createSelector<Project>(ProjectManagementContext<Project>.current, nameof(IProjectDataProvider<Project>.activeProject), OnActiveProjectChanged));
            m_DisposeOnDestroy.Add(m_ProjectRoomSelector = UISelectorFactory.createSelector<IProjectRoom[]>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.rooms)));
            m_DisposeOnDestroy.Add(m_UnityUserSelector = UISelectorFactory.createSelector<UnityUser>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.user)));
            m_DisposeOnDestroy.Add(m_LinkSharingSelector = UISelectorFactory.createSelector<LinkPermission>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.linkSharePermission), OnLinkShareDataChanged));
            m_DisposeOnDestroy.Add(m_ActiveDialogSelector = UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeDialog)));
            m_DisposeOnDestroy.Add(m_VREnableSelector = UISelectorFactory.createSelector<bool>(VRContext.current, nameof(IVREnableDataProvider.VREnable)));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<IButtonVisibility>(AppBarContext.current, nameof(IAppBarDataProvider.buttonVisibility), OnButtonVisibilityChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<IButtonInteractable>(AppBarContext.current, nameof(IAppBarDataProvider.buttonInteractable), OnButtonInteractableChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<SetHelpModeIDAction.HelpModeEntryID>(UIStateContext.current, nameof(IHelpModeDataProvider.helpModeEntryId), OnHelpModeEntryChanged));

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeSubDialog),
                type =>
                {
                    m_DialogButton.selected = type == OpenDialogAction.DialogType.LinkSharing;
                    if (m_DialogButton.selected && !m_VREnableSelector.GetValue())
                    {
                        OnOpen();
                    }
                }));
            ReflectProjectsManager.projectsRefreshCompleted.AddListener(OnProjectsRefreshCompleted);
            m_DisposeOnDestroy.Add(m_ToolBarEnabledSelector = UISelectorFactory.createSelector<bool>(UIStateContext.current, nameof(IToolBarDataProvider.toolbarsEnabled), UpdateButtonInteractable));

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeSubDialog),
                type =>
                {
                    m_DialogButton.selected = type == OpenDialogAction.DialogType.LinkSharing;
                    if (m_DialogButton.selected && !m_VREnableSelector.GetValue())
                    {
                        OnOpen();
                    }
                }));

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeDialog),
                 type =>
                 {
                     m_DialogButton.gameObject.SetActive(type != OpenDialogAction.DialogType.LandingScreen);
                     m_DialogButton.transform.parent.gameObject.SetActive(m_ButtonVisibility && type != OpenDialogAction.DialogType.LandingScreen);
                 }));

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<NetworkReachability>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.networkReachability), NetworkReachabilityChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(SessionStateContext<UnityUser, LinkPermission>.current, nameof(ISessionStateDataProvider<UnityUser, LinkPermission>.projectServerConnection), ProjectServerConnectionChanged));

            m_DialogWindow = GetComponent<DialogWindow>();

            m_ButtonVisibility = true;
            m_Interactable = true;
        }

        void Start()
        {
            ReflectProjectsManager.projectsRefreshCompleted.AddListener(OnProjectsRefreshCompleted);

            // only interactable after link created
            MakeToggleInteractable(false);
            MakeCopyButtonInteractable(false);

            m_DialogButton.buttonClicked += OnDialogButtonClick;
            m_CopyLinkButton.onClick.AddListener(OnCopyLinkButtonClick);
            m_PublicLinkToggle.onValueChanged.AddListener(OnPublicLinkToggleChanged);

            var linkSharingManager = UIStateManager.current.m_LinkSharingManager;
            linkSharingManager.sharingLinkCreated.AddListener(OnSharingLinkCreated);
            linkSharingManager.linkCreatedExceptionEvent.AddListener(OnAuthenticationFailed);
            linkSharingManager.setLinkPermissionDone.AddListener(OnSetLinkPermissionDone);
        }

        void OnLinkShareDataChanged(LinkPermission sessionData)
        {
            if (m_currentPermission != sessionData)
            {
                m_currentPermission = sessionData;
                m_PublicLinkToggle.on = m_currentPermission == LinkPermission.Public;
                m_LinkAccessMessage.text = m_currentPermission == LinkPermission.Public ? "People not added to this project have access with the link." : "Only people added to the project have access with the link.";
            }
        }

        void OnActiveProjectChanged(Project newData)
        {
            if (newData != null && m_ProjectRoomSelector.GetValue() != null)
            {
                var enable = m_ProjectRoomSelector.GetValue()
                    .Select(x => ((ProjectRoom)x).project)
                    .Any(x => x.host.ServerId.Equals(newData.host.ServerId)
                        && x.projectId.Equals(newData.projectId));

                EnablePublicToggle(enable);
            }
        }

        void OnProjectsRefreshCompleted(List<Project> projects)
        {
            var activeProject = m_ActiveProjectSelector.GetValue();
            if (activeProject != null && activeProject != Project.Empty)
            {
                var enable = projects.Any(x => x.host.ServerId.Equals(activeProject.host.ServerId)
                    && x.projectId.Equals(activeProject.projectId));
                EnablePublicToggle(enable);
            }
        }

        void EnablePublicToggle(bool enable)
        {
            m_IsToggleEnabled = enable;
            MakeToggleInteractable(enable);
        }

        void OnDialogButtonClick()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(SetHelpModeIDAction.HelpModeEntryID.LinkSharing))
            {
                m_DialogButton.selected = true;
                return;
            }

            var dialogType = m_DialogWindow.open ? OpenDialogAction.DialogType.None : OpenDialogAction.DialogType.LinkSharing;
            Dispatcher.Dispatch(SetDeltaDNAButtonAction.From($"LinkSharingDialog"));
            Dispatcher.Dispatch(OpenSubDialogAction.From(dialogType));
            if (dialogType == OpenDialogAction.DialogType.LinkSharing)
            {
                var linkSharingManager = UIStateManager.current.m_LinkSharingManager;
                linkSharingManager.GetSharingLinkInfo(UIStateManager.current.projectSettingStateData.accessToken.CloudServicesAccessToken, m_ActiveProjectSelector.GetValue());
            }
            else
            {
                Dispatcher.Dispatch(ClearStatusAction.From(true));
                Dispatcher.Dispatch(ClearStatusAction.From(false));
            }
        }

        void OnCopyLinkButtonClick()
        {
            var queryString = QueryArgHandler.GetQueryString();
            var activeProject = m_ActiveProjectSelector.GetValue();
            if (!string.IsNullOrEmpty(queryString))
            {
                GUIUtility.systemCopyBuffer = $"{activeProject.UnityProject.Uri}?{queryString}";
            }
            else
            {
                GUIUtility.systemCopyBuffer = $"{activeProject.UnityProject.Uri}";
            }
            Dispatcher.Dispatch(SetStatusMessage.From("Link copied to clipboard"));
            Dispatcher.Dispatch(SetDeltaDNAButtonAction.From($"LinkSharingCopy"));
        }

        void OnSharingLinkCreated(SharingLinkInfo sharingLinkInfo)
        {
            Debug.Log($"LinkSharingUIController OnSharingLinkCreated sharingLinkInfo.Permission: {sharingLinkInfo.Permission}");
            Dispatcher.Dispatch(SetLinkSharePermissionAction.From(sharingLinkInfo.Permission));

            // only interactable if link created successfully
            MakeToggleInteractable(m_IsToggleEnabled);
            MakeCopyButtonInteractable(true);
        }

        void OnAuthenticationFailed(Exception exception)
        {
            Dispatcher.Dispatch(SetStatusMessage.From("Unable to Get Link")); // actual exception message way to long
            MakeToggleInteractable(false);
        }

        void OnSetLinkPermissionDone(SharingLinkInfo sharingLinkInfo)
        {
            // only interactable if permission updated successfully
            MakeToggleInteractable(m_IsToggleEnabled);
            MakeCopyButtonInteractable(true);

            Dispatcher.Dispatch(SetLinkSharePermissionAction.From(sharingLinkInfo.Permission));
            Dispatcher.Dispatch(SetStatusMessage.From($"Set Access Level to: {sharingLinkInfo.Permission}"));
        }

        void OnPublicLinkToggleChanged(bool isPublic)
        {
            MakeToggleInteractable(false);
            MakeCopyButtonInteractable(false);

            LinkPermission permission = isPublic ? LinkPermission.Public : LinkPermission.Private;
            var linkSharingManager = UIStateManager.current.m_LinkSharingManager;
            linkSharingManager.SetSharingLinkPermission(UIStateManager.current.projectSettingStateData.accessToken.CloudServicesAccessToken,
                m_ActiveProjectSelector.GetValue(), permission);

            Dispatcher.Dispatch(SetDeltaDNAButtonAction.From($"PublicLink_{isPublic}"));
        }

        void MakeToggleInteractable(bool interactable)
        {
            m_PublicLinkToggle.isInteractable = interactable;
            m_PublicLinkToggleText.alpha = interactable ? 1f : 0.3f;
        }

        void MakeCopyButtonInteractable(bool interactable)
        {
            m_CopyLinkButton.interactable = interactable;
        }

        void OnOpen()
        {
            if (!m_VREnableSelector.GetValue())
            {
                // Reposition dialog in function of button position
                RectTransform buttonPos = (RectTransform)m_DialogButton.transform.parent;
                float horizontalPos = buttonPos.position.x - buttonPos.sizeDelta.x/2;
                Vector2 pos = m_Arrow.position;
                pos.x = horizontalPos;
                m_Arrow.position = pos;

                pos = m_Container.position;
                pos.x = horizontalPos + m_Container.sizeDelta.x/2;
                m_Container.position = pos;
            }
        }

        void OnButtonVisibilityChanged(IButtonVisibility data)
        {
            if (data?.type == (int)ButtonType.LinkSharing)
            {
                m_ButtonVisibility = data.visible;
                m_DialogButton.transform.parent.gameObject.SetActive(m_ButtonVisibility &&
                    m_ActiveDialogSelector.GetValue() != OpenDialogAction.DialogType.LandingScreen
                );
            }
        }

        void OnButtonInteractableChanged(IButtonInteractable data)
        {
            if (data?.type == (int)ButtonType.LinkSharing)
            {
                m_Interactable = data.interactable;
                UpdateButtonInteractable(m_ToolBarEnabledSelector.GetValue());
            }
        }

        void UpdateButtonInteractable(bool toolbarsEnabled)
        {
            m_DialogButton.button.interactable = toolbarsEnabled && m_Interactable &&
                UIStateManager.current.IsNetworkConnected;
        }

        void ProjectServerConnectionChanged(bool _)
        {
            UpdateButtonInteractable(m_ToolBarEnabledSelector.GetValue());
        }

        void NetworkReachabilityChanged(NetworkReachability _)
        {
            UpdateButtonInteractable(m_ToolBarEnabledSelector.GetValue());
        }

        void OnHelpModeEntryChanged(SetHelpModeIDAction.HelpModeEntryID newData)
        {
            if (newData == SetHelpModeIDAction.HelpModeEntryID.None)
            {
                m_DialogButton.selected = false;
            }
        }
    }
}
