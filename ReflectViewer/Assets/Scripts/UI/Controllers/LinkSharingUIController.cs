using System;
using System.Collections;
using Unity.TouchFramework;
using SharpFlux;
using SharpFlux.Dispatching;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Reflect;
using UnityEditor;
using TMPro;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(DialogWindow))]
    public class LinkSharingUIController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        Button m_DialogButton;
        [SerializeField]
        Button m_CopyLinkButton;
        [SerializeField]
        SlideToggle m_PublicLinkToggle;
        [SerializeField]
        TMP_Text m_PublicLinkToggleText;
        [SerializeField]
        TMP_Text m_LinkAccessMessage;
#pragma warning restore CS0649

        DialogWindow m_DialogWindow;
        Image m_DialogButtonImage;
        DialogType m_currentActiveDialog;
        LinkPermission m_currentPermission;
        string m_Link;

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;
            UIStateManager.sessionStateChanged += OnSessionStateDataChanged;
            m_DialogWindow = GetComponent<DialogWindow>();
            m_DialogButtonImage = m_DialogButton.GetComponent<Image>();
        }
        void Start()
        {
            // only interactable after link created
            MakeInteractable(false);

            m_DialogButton.onClick.AddListener(OnDialogButtonClick);
            m_CopyLinkButton.onClick.AddListener(OnCopyLinkButtonClick);
            m_PublicLinkToggle.onValueChanged.AddListener(OnPublicLinkToggleChanged);

            var linkSharingManager = UIStateManager.current.m_LinkSharingManager;
            linkSharingManager.sharingLinkCreated.AddListener(OnSharingLinkCreated);
            linkSharingManager.linkCreatedExceptionEvent.AddListener(OnAuthenticationFailed);
            linkSharingManager.setLinkPermissionDone.AddListener(OnSetLinkPermissionDone);
        }

        void OnStateDataChanged(UIStateData data)
        {
            m_DialogButtonImage.enabled = data.activeSubDialog == DialogType.LinkSharing;
            if (m_currentActiveDialog != data.activeDialog)
            {
                m_currentActiveDialog = data.activeDialog;
                m_DialogButton.gameObject.SetActive(data.activeDialog != DialogType.LandingScreen);
            }
        }

        void OnSessionStateDataChanged(UISessionStateData sessionData)
        {
            if (m_currentPermission != sessionData.sessionState.linkSharePermission)
            {
                m_currentPermission = sessionData.sessionState.linkSharePermission;
                m_PublicLinkToggle.on = m_currentPermission == LinkPermission.Public;
                m_LinkAccessMessage.text = m_currentPermission == LinkPermission.Public ?
                    "People not added to this project have access with the link." : "Only people added to the project have access with the link.";
            }
        }

        void OnDialogButtonClick()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(HelpModeEntryID.LinkSharing)) return;

            var dialogType = m_DialogWindow.open ? DialogType.None : DialogType.LinkSharing;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenSubDialog, dialogType));
            if (dialogType == DialogType.LinkSharing)
            {
                var linkSharingManager = UIStateManager.current.m_LinkSharingManager;
                linkSharingManager.GetSharingLinkInfo(UIStateManager.current.sessionStateData.sessionState.user.AccessToken, UIStateManager.current.projectStateData.activeProject);
            }
            else
            {
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ClearStatus, null));
                m_Link = null;
            }
        }
        void OnCopyLinkButtonClick()
        {
            GUIUtility.systemCopyBuffer = m_Link;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusMessage, "Link copied to clipboard"));
        }

        void OnSharingLinkCreated(SharingLinkInfo sharingLinkInfo)
        {
            m_Link = sharingLinkInfo.Uri.ToString();
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetLinkSharePermission, sharingLinkInfo.Permission));

            // only interactable if link created successfully
            MakeInteractable(true);
        }
        void OnAuthenticationFailed(Exception exception)
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusMessage, "Unable to Get Link"));  // actual exception message way to long
            MakeInteractable(false);
        }

        void OnSetLinkPermissionDone(SharingLinkInfo sharingLinkInfo)
        {
            // only interactable if permission updated successfully
            MakeInteractable(true);

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetLinkSharePermission, sharingLinkInfo.Permission));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusMessage, $"Set Access Level to: {sharingLinkInfo.Permission}"));
        }

        void OnPublicLinkToggleChanged(bool isPublic)
        {
            MakeInteractable(false);

            LinkPermission permission = isPublic ? LinkPermission.Public : LinkPermission.Private;
            var linkSharingManager = UIStateManager.current.m_LinkSharingManager;
            linkSharingManager.SetSharingLinkPermission(UIStateManager.current.sessionStateData.sessionState.user.AccessToken,
                UIStateManager.current.projectStateData.activeProject, permission);
        }

        void MakeInteractable(bool interactable)
        {
            m_CopyLinkButton.interactable = interactable;
            m_PublicLinkToggle.isInteractable = interactable;
            m_PublicLinkToggleText.alpha = interactable ? 1f : 0.3f;
        }
    }
}

