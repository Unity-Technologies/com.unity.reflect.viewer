using System;
using SharpFlux;
using SharpFlux.Dispatching;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(Button))]
    public class ProjectListButton : MonoBehaviour
    {
        [SerializeField, Tooltip("Button Image")]
        Image m_ProjectsButtonBackground;
        [SerializeField, Tooltip("Refresh Button")]
        Button m_RefreshButton;

        Button m_ProjectListButton;

        void Awake()
        {
            m_ProjectListButton = GetComponent<Button>();
        }

        void OnEnable()
        {
            UIStateManager.stateChanged += OnStateDataChanged;
            UIStateManager.projectStateChanged += OnProjectStateDataChanged;
        }

        void OnDisable()
        {
            UIStateManager.stateChanged -= OnStateDataChanged;
            UIStateManager.projectStateChanged -= OnProjectStateDataChanged;
        }

        void Start()
        {
            m_ProjectListButton.interactable = false;
            m_RefreshButton.onClick.AddListener(OnRefreshClicked);
            m_ProjectListButton.onClick.AddListener(OnProjectListButtonClick);
        }

        void OnRefreshClicked()
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.RefreshProjectList, null));
        }

        void OnStateDataChanged(UIStateData stateData)
        {
            if (stateData.activeDialog == DialogType.LandingScreen && stateData.dialogMode == DialogMode.Normal)
            {
                m_ProjectListButton.interactable = UIStateManager.current.projectStateData.activeProject != Project.Empty;
                m_RefreshButton.transform.parent.gameObject.SetActive(true);
                m_ProjectsButtonBackground.color = UIConfig.propertySelectedColor;
            }
            else
            {
                m_ProjectListButton.interactable = true;
                m_RefreshButton.transform.parent.gameObject.SetActive(false);
                m_ProjectsButtonBackground.color = Color.clear;
            }
        }

        void OnProjectStateDataChanged(UIProjectStateData data)
        {
            m_ProjectListButton.interactable = data.activeProject != Project.Empty;
        }

        void OnProjectListButtonClick()
        {
            var dialogType = UIStateManager.current.stateData.activeDialog == DialogType.LandingScreen ? DialogType.None : DialogType.LandingScreen;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, dialogType));
            if (dialogType == DialogType.None)
            {
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenOptionDialog, OptionDialogType.None));
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.CloseAllDialogs, null));
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ResetToolbars, null));
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ResetExternalTools, null));
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ClearStatus, null));
            }
        }
    }
}
