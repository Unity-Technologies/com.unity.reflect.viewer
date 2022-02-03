using System;
using System.Collections.Generic;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    /// <summary>
    /// Controller responsible of managing the active dialog.
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
        DialogWindow m_InfoSelectDialog;

        [SerializeField, Tooltip("Reference to the Navigation Mode Fly")]
        DialogWindow m_NavigationModeFlyOut;

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

        [SerializeField, Tooltip("Reference to the Scene Settings Dialog")]
        DialogWindow m_SceneSettingsDialog;

        [SerializeField, Tooltip("Reference to the Marker Edit dialog")]
        DialogWindow m_MarkerDialog;

        [SerializeField, Tooltip("Reference to the Left Sidebar More dialog")]
        DialogWindow m_LeftSidebarMoreDialog;

#pragma warning restore CS0649
        IUISelector<SetDialogModeAction.DialogMode> m_DialogModeSelector;
        List<IDisposable> m_DisposableSelectors;

        void Awake()
        {
            m_DialogModeSelector = UISelectorFactory.createSelector<SetDialogModeAction.DialogMode>(UIStateContext.current, nameof(IDialogDataProvider.dialogMode));
            m_DisposableSelectors = new List<IDisposable>()
            {
                m_DialogModeSelector,
                UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeDialog), OnActiveDialogChanged),
                UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeSubDialog), OnActiveSubDialogChanged),
                UISelectorFactory.createSelector<CloseAllDialogsAction.OptionDialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeOptionDialog), OnActiveOptionDialogChanged),
                UISelectorFactory.createSelector<SetProgressStateAction.ProgressState>(ProgressContext.current, nameof(IProgressDataProvider.progressState), OnProgressStateChanged),
            };
        }

        void OnDestroy()
        {
            m_DisposableSelectors?.ForEach(x => x.Dispose());
        }

        void OnProgressStateChanged(SetProgressStateAction.ProgressState newData)
        {
            if (newData == SetProgressStateAction.ProgressState.NoPendingRequest)
            {
                m_ProgressIndicatorDialog.Close(true);
            }
            else
            {
                m_ProgressIndicatorDialog.Open(true);
            }
        }

        void OnActiveOptionDialogChanged(CloseAllDialogsAction.OptionDialogType newData)
        {
            switch (newData)
            {
                case CloseAllDialogsAction.OptionDialogType.None:
                    break;
                case CloseAllDialogsAction.OptionDialogType.ProjectOptions:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void OnActiveSubDialogChanged(OpenDialogAction.DialogType newData)
        {
            m_AccountDialog.Close();
            m_LinkSharingDialog.Close();
            m_NavigationGizmoModeFanOut.Close();
            m_SceneOptionsDialog.Close();
            m_NavigationModeFlyOut.Close();

            if (m_DialogModeSelector.GetValue() == SetDialogModeAction.DialogMode.Help)
            {
                m_HelpDialogController.Display(newData);
                return;
            }

            m_LeftSidebarMoreDialog.Close();

            switch (newData)
            {
                case OpenDialogAction.DialogType.None:
                    break;
                case OpenDialogAction.DialogType.Account:
                    m_AccountDialog.Open();
                    break;
                case OpenDialogAction.DialogType.LinkSharing:
                    m_LinkSharingDialog.Open();
                    break;
                case OpenDialogAction.DialogType.GizmoMode:
                    m_NavigationGizmoModeFanOut.Open();
                    break;
                case OpenDialogAction.DialogType.SceneOptions:
                    m_SceneOptionsDialog.Open();
                    break;
                case OpenDialogAction.DialogType.NavigationMode:
                    m_NavigationModeFlyOut.Open();
                    break;
                case OpenDialogAction.DialogType.LeftSidebarMore:
                    m_LeftSidebarMoreDialog.Open();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void OnActiveDialogChanged(OpenDialogAction.DialogType newData)
        {
            m_FiltersDialog.Close();
            m_ClippingToolDialog.Close();
            m_CameraOptionsDialog.Close();
            m_SunStudyDialog.Close();
            m_SequenceDialog.Close();
            m_LandingScreenDialog.Close();
            m_InfoSelectDialog.Close();
            m_ARCardSelectionDialog.Close();
            m_CollaborationUserListDialog.Close();
            m_CollaborationUserInfoDialog.Close();
            m_LoginScreenDialog.Close();
            m_SceneSettingsDialog.Close();
            m_MarkerDialog.Close();
            m_BimDialog.Close();

            if (m_DialogModeSelector.GetValue() == SetDialogModeAction.DialogMode.Help)
            {
                m_HelpDialogController.Display(newData);
                return;
            }

            switch (newData)
            {
                case OpenDialogAction.DialogType.None:
                    break;
                case OpenDialogAction.DialogType.Projects:
                    // TODO remove this type and ProjectsUIController class
                    // m_ProjectsDialog.Open();
                    break;
                case OpenDialogAction.DialogType.Filters:
                    m_FiltersDialog.Open();
                    break;
                case OpenDialogAction.DialogType.ClippingTool:
                    m_ClippingToolDialog.Open();
                    break;
                case OpenDialogAction.DialogType.CameraOptions:
                    m_CameraOptionsDialog.Open();
                    break;
                case OpenDialogAction.DialogType.SunStudy:
                    m_SunStudyDialog.Open();
                    break;
                case OpenDialogAction.DialogType.Sequence:
                    m_SequenceDialog.Open();
                    break;
                case OpenDialogAction.DialogType.SelectTool:
                    // TODO
                    break;
                case OpenDialogAction.DialogType.LandingScreen:
                    m_LandingScreenDialog.Open();
                    break;
                case OpenDialogAction.DialogType.InfoSelect:
                    m_InfoSelectDialog.Open();
                    break;
                case OpenDialogAction.DialogType.ARCardSelection:
                    m_ARCardSelectionDialog.Open();
                    break;
                case OpenDialogAction.DialogType.CollaborationUserList:
                    m_CollaborationUserListDialog.Open();
                    break;
                case OpenDialogAction.DialogType.CollaborationUserInfo:
                    m_CollaborationUserInfoDialog.Open();
                    break;
                case OpenDialogAction.DialogType.LoginScreen:
                    m_LoginScreenDialog.Open();
                    break;
                case OpenDialogAction.DialogType.SceneSettings:
                    m_SceneSettingsDialog.Open();
                    break;
                case OpenDialogAction.DialogType.Marker:
                    m_MarkerDialog.Open();
                    break;
                case OpenDialogAction.DialogType.BimInfo:
                    m_BimDialog.Open();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
