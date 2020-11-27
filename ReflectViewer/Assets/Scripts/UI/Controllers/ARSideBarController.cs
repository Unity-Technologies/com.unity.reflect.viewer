using System;
using SharpFlux;
using UnityEngine;
using UnityEngine.Reflect.Viewer;

namespace Unity.Reflect.Viewer.UI
{
    public class ARSideBarController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        ToolButton m_BackButton;

        [SerializeField]
        ToolButton m_ScaleButton;

        [SerializeField]
        ToolButton m_SelectButton;

        [SerializeField]
        ToolButton m_TargetButton;

        [SerializeField]
        GameObject m_ScaleRadial;
#pragma warning restore CS0649

        bool m_ToolbarsEnabled;
        ToolType? m_CurrentActiveTool;
        InstructionUIState? m_CurrentInstructionUI;
        ToolState? m_CurrentToolState;
        ARToolStateData? m_CachedARToolStateData;

        SpatialSelector m_ObjectSelector;

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;
            UIStateManager.arStateChanged += OnARStateDataChanged;

            m_BackButton.buttonClicked += OnBackButtonClicked;
            m_SelectButton.buttonClicked += OnSelectButtonClicked;
            m_ScaleButton.buttonClicked += OnScaleButtonClicked;
            m_TargetButton.buttonClicked += OnTargetButtonClicked;

            OnStateDataChanged(UIStateManager.current.stateData);
            OnARStateDataChanged(UIStateManager.current.arStateData);
            m_ObjectSelector = new SpatialSelector();
        }

        void OnStateDataChanged(UIStateData data)
        {
            if (m_ToolbarsEnabled != data.toolbarsEnabled)
            {
                m_ToolbarsEnabled = data.toolbarsEnabled;
                OnARStateDataChanged(UIStateManager.current.arStateData);
            }

            if (m_CurrentToolState != data.toolState)
            {
                m_SelectButton.selected = false;

                if (data.toolState.activeTool == ToolType.SelectTool)
                {
                    m_SelectButton.selected = true;
                }
                m_CurrentToolState = data.toolState;
            }
        }

        void OnARStateDataChanged(UIARStateData arData)
        {
            if (m_CachedARToolStateData != arData.arToolStateData)
            {
                m_SelectButton.button.interactable = m_ToolbarsEnabled && arData.arToolStateData.selectionEnabled;
                m_ScaleButton.button.interactable = m_ToolbarsEnabled && arData.arToolStateData.scaleEnabled;
                m_BackButton.button.interactable = m_ToolbarsEnabled && arData.arToolStateData.previousStepEnabled;
                m_CachedARToolStateData = arData.arToolStateData;
            }
        }

        void OnTargetButtonClicked()
        {
        }

        void OnScaleButtonClicked()
        {
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.ARScaleDial));
            ARScaleRadialUIController.m_previousToolbar = ToolbarType.ARSidebar;

            var radialPosition = m_ScaleRadial.transform.position;
            radialPosition.y = m_ScaleButton.transform.position.y;
            m_ScaleRadial.transform.position = radialPosition;
        }

        void OnSelectButtonClicked()
        {
            var toolState = UIStateManager.current.stateData.toolState;
            toolState.activeTool = m_SelectButton.selected ? ToolType.None : ToolType.SelectTool;

            var dialogType = m_SelectButton.selected ? DialogType.None : DialogType.BimInfo;

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetObjectPicker, m_ObjectSelector));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, toolState));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenSubDialog, dialogType));
        }

        void OnBackButtonClicked()
        {
            UIStateManager.current.arStateData.currentInstructionUI.Back();
        }
    }
}
