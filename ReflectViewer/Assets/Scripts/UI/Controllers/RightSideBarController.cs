using System;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using Unity.Reflect.Viewer.Core.Actions;
using UnityEngine;
using UnityEngine.Reflect.MeasureTool;
using UnityEngine.Reflect.Viewer;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    public class RightSideBarController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        ToolButton m_SelectButton;

        [SerializeField]
        ToolButton m_MeasureToolButton;
#pragma warning restore CS0649

        SpatialSelector m_ObjectSelector;
        IUISelector<bool> m_MeasureToolStateSelector;
        IUISelector<SelectObjectAction.IObjectSelectionInfo> m_ObjectSelectionInfoSelector;
        IUISelector<SetActiveToolAction.ToolType> m_ActiveToolSelector;
        IUISelector<SetOrbitTypeAction.OrbitType> m_OrbitTypeSelector;
        IUISelector<OpenDialogAction.DialogType> m_ActiveDialogSelector;
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        void OnDestroy()
        {
            m_ObjectSelector?.Dispose();
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        void Awake()
        {
            ToolStateContext.current.stateChanged += UpdateToolState;

            m_DisposeOnDestroy.Add(m_MeasureToolStateSelector = UISelectorFactory.createSelector<bool>(MeasureToolContext.current, nameof(IMeasureToolDataProvider.toolState), OnToolStateDataChanged));
            m_DisposeOnDestroy.Add(m_ObjectSelectionInfoSelector = UISelectorFactory.createSelector<SelectObjectAction.IObjectSelectionInfo>(ProjectContext.current, nameof(IObjectSelectorDataProvider.objectSelectionInfo)));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(UIStateContext.current, nameof(IToolBarDataProvider.toolbarsEnabled), OnToolbarEnabledChanged));
            m_DisposeOnDestroy.Add(m_ActiveToolSelector = UISelectorFactory.createSelector<SetActiveToolAction.ToolType>(ToolStateContext.current, nameof(IToolStateDataProvider.activeTool)));
            m_DisposeOnDestroy.Add(m_OrbitTypeSelector = UISelectorFactory.createSelector<SetOrbitTypeAction.OrbitType>(ToolStateContext.current, nameof(IToolStateDataProvider.orbitType)));
            m_DisposeOnDestroy.Add(m_ActiveDialogSelector = UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeDialog)));

            m_SelectButton.buttonClicked += OnSelectButtonClicked;
            m_MeasureToolButton.buttonClicked += OnMeasureToolButtonClicked;

            m_ObjectSelector = new SpatialSelector();
        }

        void UpdateToolState()
        {
            m_SelectButton.selected = false;

            var activeTool = m_ActiveToolSelector.GetValue();
            var orbitType = m_OrbitTypeSelector.GetValue();

            switch (activeTool)
            {
                case SetActiveToolAction.ToolType.SelectTool:
                    m_SelectButton.selected = true;
                    break;
            }
        }

        void OnToolbarEnabledChanged(bool data)
        {
            m_SelectButton.button.interactable = data;
            m_MeasureToolButton.button.interactable = data;
        }

        void OnToolStateDataChanged(bool newData)
        {
            m_MeasureToolButton.selected = newData;
        }

        void OnSelectButtonClicked()
        {
            var dialogType = m_SelectButton.selected ? OpenDialogAction.DialogType.None : OpenDialogAction.DialogType.BimInfo;

            Dispatcher.Dispatch(SetSpatialSelectorAction.From(m_ObjectSelector));
            Dispatcher.Dispatch(SetActiveToolAction.From(m_SelectButton.selected ? SetActiveToolAction.ToolType.None : SetActiveToolAction.ToolType.SelectTool));
            Dispatcher.Dispatch(OpenSubDialogAction.From(dialogType));
            Dispatcher.Dispatch(ToggleMeasureToolAction.From(ToggleMeasureToolAction.ToggleMeasureToolData.defaultData));
        }

        void OnMeasureToolButtonClicked()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(SetHelpModeIDAction.HelpModeEntryID.MeasureTool)) return;

            var toggleData = ToggleMeasureToolAction.ToggleMeasureToolData.defaultData;
            toggleData.toolState = !m_MeasureToolStateSelector.GetValue();

            if (toggleData.toolState)
            {
                if (m_ActiveToolSelector.GetValue() == SetActiveToolAction.ToolType.SelectTool && ((ObjectSelectionInfo)m_ObjectSelectionInfoSelector.GetValue()).CurrentSelectedObject() == null)
                {
                    Dispatcher.Dispatch(SetActiveToolAction.From(SetActiveToolAction.ToolType.None));
                    Dispatcher.Dispatch(OpenSubDialogAction.From(OpenDialogAction.DialogType.None));
                }

                Dispatcher.Dispatch(SetStatusMessageWithType.From(
                    new StatusMessageData() { text = MeasureToolUIController.instructionStart, type = StatusMessageType.Instruction }));
            }
            else
            {
                Dispatcher.Dispatch(ClearStatusAction.From(true));
                Dispatcher.Dispatch(ClearStatusAction.From(false));
            }

            Dispatcher.Dispatch(ToggleMeasureToolAction.From(toggleData));

            // To initialize Anchor
            Dispatcher.Dispatch(SetSpatialSelectorAction.From(m_ObjectSelector));

            if (m_SelectButton.selected)
            {
                Dispatcher.Dispatch(SetActiveToolAction.From(SetActiveToolAction.ToolType.None));
                Dispatcher.Dispatch( OpenSubDialogAction.From(OpenDialogAction.DialogType.None));
            }
        }
    }
}
