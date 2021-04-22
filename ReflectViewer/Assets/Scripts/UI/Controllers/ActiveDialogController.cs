using System;
using System.Data;
using Unity.TouchFramework;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    /// <summary>
    /// Controller responible of managing the active dialog.
    /// </summary>
    public class ActiveDialogController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        HelpDialogController m_HelpDialogController;

        [SerializeField]
        DialogWindow m_LandingScreenDialog;

        [SerializeField, Tooltip("Reference to the ClippingTool Dialog")]
        DialogWindow m_ClippingToolDialog;

        [SerializeField, Tooltip("Reference to the Filters Dialog")]
        DialogWindow m_FiltersDialog;

        [SerializeField, Tooltip("Reference to the Camera Options Dialog")]
        DialogWindow m_CameraOptionsDialog;

        [SerializeField, Tooltip("Reference to the Scene Options Dialog")]
        DialogWindow m_SceneOptionsDialog;

        [SerializeField, Tooltip("Reference to the Sun Study Dialog")]
        DialogWindow m_SunStudyDialog;

        [SerializeField, Tooltip("Reference to the Sequence Dialog")]
        DialogWindow m_SequenceDialog;

        [SerializeField, Tooltip("Reference to the Account Dialog")]
        DialogWindow m_AccountDialog;

        [SerializeField, Tooltip("Reference to the Link Sharing Dialog")]
        DialogWindow m_LinkSharingDialog;

        [SerializeField]
        DialogWindow m_ProgressIndicatorDialog;

        [SerializeField, Tooltip("Reference to the BIM Dialog")]
        DialogWindow m_BimDialog;

        [SerializeField]
        DialogWindow m_StatsInfoDialog;

        [SerializeField]
        DialogWindow m_InfoSelectDialog;

        [SerializeField]
        DialogWindow m_DebugOptionsDialog;

        [SerializeField, Tooltip("Reference to the Navigation Mode FanOut")]
        FanOutWindow m_NavigationModeFanOut;

        [SerializeField]
        DialogWindow m_OrbitSelectDialog;

        [SerializeField, Tooltip("Reference to the Gizmo Navigation Mode FanOut")]
        FanOutWindow m_NavigationGizmoModeFanOut;

        [SerializeField]
        DialogWindow m_ARCardSelectionDialog;

        [SerializeField, Tooltip("Reference to the Collaboration Vertical User List")]
        DialogWindow m_CollaborationUserListDialog;

        [SerializeField, Tooltip("Reference to the Collaboration Vertical User List")]
        DialogWindow m_CollaborationUserInfoDialog;

        [SerializeField]
        DialogWindow m_LoginScreenDialog;

#pragma warning restore CS0649

        DialogType m_CurrentActiveDialog = DialogType.None;
        DialogType m_CurrentSubDialog = DialogType.None;
        OptionDialogType m_CurrentActiveOptionDialog = OptionDialogType.None;
        ProgressData m_CurrentProgressData;

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;
        }

        void OnStateDataChanged(UIStateData stateData)
        {
            if (m_CurrentActiveDialog != stateData.activeDialog)
            {
                m_CurrentActiveDialog = stateData.activeDialog;

                m_FiltersDialog.Close();
                m_OrbitSelectDialog.Close();
                m_ClippingToolDialog.Close();
                m_CameraOptionsDialog.Close();
                m_SceneOptionsDialog.Close();
                m_SunStudyDialog.Close();
                m_SequenceDialog.Close();
                m_LandingScreenDialog.Close();
                m_NavigationModeFanOut.Close();
                m_NavigationGizmoModeFanOut.Close();
                m_StatsInfoDialog.Close();
                m_InfoSelectDialog.Close();
                m_DebugOptionsDialog.Close();
                m_ARCardSelectionDialog.Close();
                m_CollaborationUserListDialog.Close();
                m_CollaborationUserInfoDialog.Close();
                m_LoginScreenDialog.Close();

                if (stateData.dialogMode == DialogMode.Help)
                {
                    m_HelpDialogController.Display(m_CurrentActiveDialog);
                    return;
                }

                switch (stateData.activeDialog)
                {
                    case DialogType.None:
                        break;
                    case DialogType.Projects:
                        // TODO remove this type and ProjectsUIController class
                        // m_ProjectsDialog.Open();
                        break;
                    case DialogType.Filters:
                        m_FiltersDialog.Open();
                        break;
                    case DialogType.OrbitSelect:
                        m_OrbitSelectDialog.Open();
                        break;
                    case DialogType.ClippingTool:
                        m_ClippingToolDialog.Open();
                        break;
                    case DialogType.CameraOptions:
                        m_CameraOptionsDialog.Open();
                        break;
                    case DialogType.SceneOptions:
                        m_SceneOptionsDialog.Open();
                        break;
                    case DialogType.SunStudy:
                        m_SunStudyDialog.Open();
                        break;
                    case DialogType.Sequence:
                        m_SequenceDialog.Open();
                        break;
                    case DialogType.SelectTool:
                        // TODO
                        break;
                    case DialogType.LandingScreen:
                        m_LandingScreenDialog.Open();
                        break;
                    case DialogType.NavigationMode:
                        m_NavigationModeFanOut.Open();
                        break;
                    case DialogType.GizmoMode:
                        m_NavigationGizmoModeFanOut.Open();
                        break;
                    case DialogType.StatsInfo:
                        m_StatsInfoDialog.Open();
                        break;
                    case DialogType.InfoSelect:
                        m_InfoSelectDialog.Open();
                        break;
                    case DialogType.DebugOptions:
                        m_DebugOptionsDialog.Open();
                        break;
                    case DialogType.ARCardSelection:
                        m_ARCardSelectionDialog.Open();
                        break;
                    case DialogType.CollaborationUserList:
                        m_CollaborationUserListDialog.Open();
                        break;
                    case DialogType.CollaborationUserInfo:
                        m_CollaborationUserInfoDialog.Open();
                        break;
                    case DialogType.LoginScreen:
                        m_LoginScreenDialog.Open();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (m_CurrentSubDialog != stateData.activeSubDialog)
            {
                m_CurrentSubDialog = stateData.activeSubDialog;

                m_NavigationModeFanOut.Close();
                m_BimDialog.Close();
                m_AccountDialog.Close();
                m_LinkSharingDialog.Close();

                if (stateData.dialogMode == DialogMode.Help)
                {
                    m_HelpDialogController.Display(m_CurrentSubDialog);
                    return;
                }

                switch (stateData.activeSubDialog)
                {
                    case DialogType.BimInfo:
                        m_BimDialog.Open();
                        break;
                    case DialogType.Account:
                        m_AccountDialog.Open();
                        break;
                    case DialogType.LinkSharing:
                        m_LinkSharingDialog.Open();
                        break;
                }
            }

            if (m_CurrentActiveOptionDialog != stateData.activeOptionDialog)
            {
                switch (stateData.activeOptionDialog)
                {
                    case OptionDialogType.None:
                        break;
                    case OptionDialogType.ProjectOptions:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                m_CurrentActiveOptionDialog = stateData.activeOptionDialog;
            }

            if (m_CurrentProgressData != stateData.progressData)
            {
                if (stateData.progressData.progressState == ProgressData.ProgressState.NoPendingRequest)
                {
                    m_ProgressIndicatorDialog.Close(true);
                }
                else
                {
                    m_ProgressIndicatorDialog.Open(true);
                }

                m_CurrentProgressData = stateData.progressData;
            }
        }
    }
}
