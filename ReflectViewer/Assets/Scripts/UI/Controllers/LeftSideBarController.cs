using System;
using System.Collections;
using SharpFlux.Dispatching;
using UnityEngine;
using UnityEngine.Reflect.Viewer;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    public class LeftSideBarController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        RectTransform m_MainRectTransform;
        [SerializeField]
        RectTransform m_SubRectTransform;
        [SerializeField]
        ToolButton m_FilterButton;
        [SerializeField]
        ToolButton m_SunstudyButton;
        [SerializeField]
        ToolButton m_SceneSettingsButton;
        [SerializeField]
        ToolButton m_MarkerButton;
        [SerializeField]
        ToolButton m_SelectButton;
        [SerializeField]
        ToolButton m_MeasureToolButton;
        [SerializeField]
        ToolButton m_MoreButton;
        [SerializeField]
        LeftSidebarMoreController m_MoreButtonController;
        [SerializeField]
        RectTransform m_GizmoBuffer;
#pragma warning restore CS0649

        IUISelector<bool> m_ToolBarEnabledGetter;
        IUISelector<bool> m_BimFilterEnabledGetter;
        IUISelector<bool> m_SunStudyEnabledGetter;
        IUISelector<bool> m_SceneSettingEnabledGetter;
        IUISelector<bool> m_SelectEnabledGetter;
        IUISelector<bool> m_MeasureToolEnabledGetter;
        IUISelector<bool> m_VREnableGetter;
        IUISelector<bool> m_MarkerEnabledGetter;
        IUISelector<Vector2> m_ScaledScreenSizeGetter;
        IUISelector<SetActiveToolAction.ToolType> m_ActiveToolSelector;
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        SpatialSelector m_ObjectSelector;

        IUISelector<bool> m_MeasureToolStateSelector;
        IUISelector<SelectObjectAction.IObjectSelectionInfo> m_ObjectSelectionInfoSelector;
        RectTransform m_LayoutRoot;
        float m_Spacing;
        List<ToolButton> m_HiddenButtons;
        ToolButton[] m_Buttons;
        Coroutine m_Coroutine;

        const float k_ButtonHeight = 60f;

        void OnDestroy()
        {
            m_SelectButton.buttonClicked -= CloseMeasureTool;
            m_FilterButton.buttonClicked -= CloseTools;
            m_SunstudyButton.buttonClicked -= CloseTools;
            m_SceneSettingsButton.buttonClicked -= CloseTools;
            m_MarkerButton.buttonClicked -= CloseTools;
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        void Awake()
        {
            m_LayoutRoot = GetComponent<RectTransform>();
            m_Buttons = m_MainRectTransform.GetComponentsInChildren<ToolButton>().OrderBy(b => b.transform.parent.GetSiblingIndex()).ToArray();
            m_HiddenButtons = new List<ToolButton>();

            ToolStateContext.current.stateChanged += UpdateToolState;
            m_SelectButton.buttonClicked += CloseMeasureTool;
            m_FilterButton.buttonClicked += CloseTools;
            m_SunstudyButton.buttonClicked += CloseTools;
            m_SceneSettingsButton.buttonClicked += CloseTools;
            m_MarkerButton.buttonClicked += CloseTools;

            m_DisposeOnDestroy.Add(m_ActiveToolSelector = UISelectorFactory.createSelector<SetActiveToolAction.ToolType>(ToolStateContext.current, nameof(IToolStateDataProvider.activeTool)));

            m_DisposeOnDestroy.Add(m_ToolBarEnabledGetter = UISelectorFactory.createSelector<bool>(UIStateContext.current, nameof(IToolBarDataProvider.toolbarsEnabled),
                data =>
                {
                    UpdateButtons();
                }));
            m_DisposeOnDestroy.Add(m_BimFilterEnabledGetter = UISelectorFactory.createSelector<bool>(SettingsToolContext.current, nameof(ISettingsToolDataProvider.bimFilterEnabled),
                data =>
                {
                    UpdateButtons();
                }));
            m_DisposeOnDestroy.Add(m_SunStudyEnabledGetter = UISelectorFactory.createSelector<bool>(SettingsToolContext.current, nameof(ISettingsToolDataProvider.sunStudyEnabled),
            data =>
                {
                    UpdateButtons();
                }));
            m_DisposeOnDestroy.Add(m_SceneSettingEnabledGetter = UISelectorFactory.createSelector<bool>(SettingsToolContext.current, nameof(ISettingsToolDataProvider.sceneSettingsEnabled),
                data =>
                {
                    UpdateButtons();
                }));

            m_DisposeOnDestroy.Add(m_MarkerEnabledGetter = UISelectorFactory.createSelector<bool>(SettingsToolContext.current, nameof(ISettingsToolDataProvider.markerSettingsEnabled),
                data =>
                {
                    UpdateButtons();
                }));

            m_DisposeOnDestroy.Add(m_SelectEnabledGetter = UISelectorFactory.createSelector<bool>(ARToolStateContext.current, nameof(IARToolStatePropertiesDataProvider.selectionEnabled),
                data =>
                {
                    UpdateButtons();
                }));

            m_DisposeOnDestroy.Add(m_MeasureToolEnabledGetter = UISelectorFactory.createSelector<bool>(ARToolStateContext.current, nameof(IARToolStatePropertiesDataProvider.measureToolEnabled),
                data =>
                {
                    UpdateButtons();
                }));

            m_DisposeOnDestroy.Add(m_VREnableGetter = UISelectorFactory.createSelector<bool>(VRContext.current, nameof(IVREnableDataProvider.VREnable), OnVREnableChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(NavigationContext.current, nameof(INavigationDataProvider.gizmoEnabled), OnGizmoEnabledChanged));
            m_DisposeOnDestroy.Add(m_ScaledScreenSizeGetter = UISelectorFactory.createSelector<Vector2>(UIStateContext.current, nameof(IUIStateDisplayProvider<DisplayData>.display) + "." + nameof(IDisplayDataProvider.scaledScreenSize), OnScaledScreenSizeChanged));

            m_MoreButtonController.OnItemClick += OnExtraButtonClicked;

            m_DisposeOnDestroy.Add(m_MeasureToolStateSelector = UISelectorFactory.createSelector<bool>(MeasureToolContext.current, nameof(IMeasureToolDataProvider.toolState), OnToolStateDataChanged));
            m_DisposeOnDestroy.Add(m_ObjectSelectionInfoSelector = UISelectorFactory.createSelector<SelectObjectAction.IObjectSelectionInfo>(ProjectContext.current, nameof(IObjectSelectorDataProvider.objectSelectionInfo)));

            m_ObjectSelector = new SpatialSelector();
            var verticalLayout = m_LayoutRoot.GetComponent<VerticalLayoutGroup>();
            m_Spacing = verticalLayout.spacing;
        }

        void CloseTools()
        {
            // TODO: Find a more global way to avoid adding this to every new button's buttonClick Action.
            if (m_ActiveToolSelector.GetValue() != SetActiveToolAction.ToolType.None)
            {
                Dispatcher.Dispatch(SetActiveToolAction.From(SetActiveToolAction.ToolType.None));
            }

            CloseMeasureTool();
        }

        void CloseMeasureTool()
        {
            if (m_MeasureToolStateSelector.GetValue())
            {
                var toggleData = ToggleMeasureToolAction.ToggleMeasureToolData.defaultData;
                Dispatcher.Dispatch(ToggleMeasureToolAction.From(toggleData));
                Dispatcher.Dispatch(ClearStatusAction.From(true));
                Dispatcher.Dispatch(ClearStatusAction.From(false));
            }
        }

        void OnToolStateDataChanged(bool newData)
        {
            m_MeasureToolButton.selected = newData;
        }

        void OnGizmoEnabledChanged(bool newData)
        {
            m_GizmoBuffer.gameObject.SetActive(newData);
            UpdateLayout();
        }

        void OnScaledScreenSizeChanged(Vector2 _)
        {
            UpdateMainBarHeightNextFrame();
        }

        void OnVREnableChanged(bool _)
        {
            UpdateMainBarHeightNextFrame();
        }

        void UpdateButtons()
        {
            bool isHiddenButtonDirty = false;

            isHiddenButtonDirty |= UpdateButton(m_FilterButton, m_BimFilterEnabledGetter?.GetValue() ?? false);
            isHiddenButtonDirty |= UpdateButton(m_SunstudyButton, m_SunStudyEnabledGetter?.GetValue() ?? false);
            isHiddenButtonDirty |= UpdateButton(m_SceneSettingsButton, m_SceneSettingEnabledGetter?.GetValue() ?? false);
            isHiddenButtonDirty |= UpdateButton(m_SelectButton, m_SelectEnabledGetter?.GetValue() ?? false);
            isHiddenButtonDirty |= UpdateButton(m_MeasureToolButton, m_MeasureToolEnabledGetter?.GetValue() ?? false);

            isHiddenButtonDirty |= UpdateButton(m_MarkerButton, m_MarkerEnabledGetter?.GetValue() ?? false);

            if (isHiddenButtonDirty)
            {
                m_MoreButtonController.UpdateButtons(m_HiddenButtons);
                if (!m_HiddenButtons.Any())
                {
                    m_MoreButton.transform.parent.gameObject.SetActive(false);
                }
            }

            UpdateLayout();
        }

        bool UpdateButton(ToolButton button, bool value)
        {
            var enable = (m_ToolBarEnabledGetter?.GetValue() ?? false) && value;
            button.transform.parent.gameObject.SetActive(enable);
            if (m_HiddenButtons.Contains(button))
            {
                m_HiddenButtons.Remove(button);
                return true;
            }

            return false;
        }

        public void UpdateLayout()
        {
            if (m_LayoutRoot != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(m_LayoutRoot);
            UpdateMainBarHeightNextFrame();
        }

        void UpdateToolState()
        {
            m_SelectButton.selected = false;

            var activeTool = m_ActiveToolSelector.GetValue();

            switch (activeTool)
            {
                case SetActiveToolAction.ToolType.SelectTool:
                    m_SelectButton.selected = true;
                    break;
            }
        }

        void UpdateMainBarHeight()
        {
            if (m_VREnableGetter.GetValue())
            {
                // Disable hidden button feature in VR
                if (m_HiddenButtons.Any())
                {
                    m_MoreButton.transform.parent.gameObject.SetActive(false);
                    foreach (var button in m_HiddenButtons)
                    {
                        button.transform.parent.gameObject.SetActive(true);
                    }

                    m_HiddenButtons.Clear();
                }

                return;
            }

            var layoutHeight = m_ScaledScreenSizeGetter.GetValue().y + m_LayoutRoot.sizeDelta.y;

            var spacing = (m_SubRectTransform.rect.height > 0 ? m_Spacing : 0) + m_Spacing;
            var gizmoBuffer = (m_GizmoBuffer.gameObject.activeSelf ? m_GizmoBuffer.rect.height : 0);
            var contentHeight = (m_MainRectTransform.rect.height + m_SubRectTransform.rect.height + spacing + gizmoBuffer);

            if (layoutHeight <= contentHeight)
            {
                HideButtons(contentHeight, layoutHeight);
            }
            else if (m_HiddenButtons.Any() &&
                layoutHeight - contentHeight > k_ButtonHeight)
            {
                ShowButtons(contentHeight, layoutHeight);
            }
        }

        void HideButtons(float contentHeight, float layoutHeight)
        {
            // Compute how many button we need to hide
            var removeCount = Mathf.Ceil((contentHeight - layoutHeight) / k_ButtonHeight);

            // Left place for the More button
            if (!m_MoreButton.transform.parent.gameObject.activeSelf)
            {
                removeCount++;
            }

            // Check if an hidden button was selected, if so remove it
            foreach (var button in m_HiddenButtons)
            {
                if (button.selected)
                {
                    m_HiddenButtons.Remove(button);

                    // Only one can be selected at once
                    break;
                }
            }

            var activatedButtons = m_Buttons.Where(b => b.transform.parent.gameObject.activeSelf && !b.selected).ToList();
            var size = activatedButtons.Count - 1;
            for (int i = size; i > size - removeCount && i >= 1; i--)
            {
                m_HiddenButtons.Add(activatedButtons[i]);
                activatedButtons[i].transform.parent.gameObject.SetActive(false);
            }

            if (m_HiddenButtons.Count == 1)
            {
                var button = m_HiddenButtons[0];
                button.transform.parent.gameObject.SetActive(true);
                m_HiddenButtons.RemoveAt(0);

                m_MoreButton.transform.parent.gameObject.SetActive(false);
                if (m_MoreButton.selected)
                {
                    Dispatcher.Dispatch(OpenSubDialogAction.From(OpenDialogAction.DialogType.None));
                }
            }
            else if (m_HiddenButtons.Count > 0)
            {
                m_MoreButton.transform.parent.gameObject.SetActive(true);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(m_MainRectTransform);
            m_MoreButtonController.UpdateButtons(m_HiddenButtons.OrderBy(b => b.transform.parent.GetSiblingIndex()));
        }

        void ShowButtons(float contentHeight, float layoutHeight)
        {
            // Try re enable hidden buttons
            var addCount = Mathf.Floor((layoutHeight - contentHeight) / k_ButtonHeight);

            var size = Mathf.Min(addCount, m_HiddenButtons.Count);
            for (int i = 0; i < size; i++)
            {
                var button = m_HiddenButtons[m_HiddenButtons.Count - 1];
                button.transform.parent.gameObject.SetActive(true);
                m_HiddenButtons.RemoveAt(m_HiddenButtons.Count - 1);
            }

            var count = m_HiddenButtons.Count;
            if (count <= 1)
            {
                if (count == 1)
                {
                    var button = m_HiddenButtons[0];
                    button.transform.parent.gameObject.SetActive(true);
                    m_HiddenButtons.RemoveAt(0);
                }

                m_MoreButton.transform.parent.gameObject.SetActive(false);
                if (m_MoreButton.selected)
                {
                    Dispatcher.Dispatch(OpenSubDialogAction.From(OpenDialogAction.DialogType.None));
                }
            }
            else
            {
                m_MoreButtonController.UpdateButtons(m_HiddenButtons.OrderBy(b => b.transform.parent.GetSiblingIndex()));
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(m_MainRectTransform);
        }

        void OnExtraButtonClicked()
        {
            UpdateMainBarHeightNextFrame();
        }

        void UpdateMainBarHeightNextFrame()
        {
            IEnumerator UpdateNextFrame()
            {
                yield return null;
                UpdateMainBarHeight();
                m_Coroutine = null;
            }

            // Avoid multiple call in the same frame
            if (m_Coroutine == null && gameObject.activeInHierarchy)
            {
                m_Coroutine = StartCoroutine(UpdateNextFrame());
            }
        }
    }
}
