using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SharpFlux;
using SharpFlux.Dispatching;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(DialogWindow))]
    public class LeftSidebarMoreController : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        ToolButton m_DialogButton; 
        [SerializeField]
        RectTransform m_Container;
        [SerializeField]
        ScrollRect m_ScrollView;
        [SerializeField]
        ExtraButtonListItemController m_ExtraButtonPrebab;
#pragma warning restore CS0649

        DialogWindow m_DialogWindow;
        Vector3 m_ButtonPosition;
        RectTransform m_RectTransform;
        RectTransform m_ScrollViewParent;
        Coroutine m_Coroutine;
        List<ExtraButtonListItemController> m_ExtraButtons;

        IUISelector<OpenDialogAction.DialogType> m_ActiveSubDialogGetter;
        IUISelector<SetDialogModeAction.DialogMode> m_DialogModeGetter;
        IUISelector<float> m_ScaleFactorGetter;
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        public event Action OnItemClick;

        void OnDestroy()
        {
            m_DialogButton.buttonClicked -= OnDialogButtonClicked;
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        void Awake()
        {
            m_RectTransform = (RectTransform)transform;
            m_DialogWindow = GetComponent<DialogWindow>();
            m_ScrollViewParent = (RectTransform)m_ScrollView.transform.parent;
            m_DialogButton.buttonClicked += OnDialogButtonClicked;
            m_DialogWindow.dialogOpen.AddListener(OnOpen);

            m_ExtraButtons = new List<ExtraButtonListItemController>();

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(MeasureToolContext.current, nameof(IMeasureToolDataProvider.toolState), OnMeasureToolStateDataChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeDialog), OnActiveDialogChanged));
            m_DisposeOnDestroy.Add(m_ActiveSubDialogGetter = UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeSubDialog), OnActiveSubDialogChanged));
            m_DisposeOnDestroy.Add(m_DialogModeGetter = UISelectorFactory.createSelector<SetDialogModeAction.DialogMode>(UIStateContext.current, nameof(IDialogDataProvider.dialogMode), OnDialogModeChanged));
            m_DisposeOnDestroy.Add(m_ScaleFactorGetter = UISelectorFactory.createSelector<float>(UIStateContext.current, nameof(IUIStateDisplayProvider<DisplayData>.display) + "." + nameof(IDisplayDataProvider.scaleFactor)));
        }

        public void UpdateButtons(IEnumerable<ToolButton> buttons)
        {
            foreach (var button in m_ExtraButtons)
            {
                button.gameObject.SetActive(false);
            }

            while(buttons.Count() > m_ExtraButtons.Count)
            {
                m_ExtraButtons.Add(Instantiate(m_ExtraButtonPrebab, m_ScrollView.content));
            }

            int i = 0;
            foreach (var button in buttons)
            {
                m_ExtraButtons[i].gameObject.SetActive(true);
                m_ExtraButtons[i].Name = button.transform.parent.name;
                m_ExtraButtons[i].Icon = button.buttonIcon.sprite;
                m_ExtraButtons[i].OnClick = () =>
                {
                    button.button.onClick?.Invoke();
                    if (m_DialogModeGetter.GetValue() == SetDialogModeAction.DialogMode.Normal)
                    {
                        button.transform.parent.gameObject.SetActive(true);
                        OnItemClick?.Invoke();
                    }
                };
                i++;
            }
        }

        void OnOpen()
        {
            UpdatePosition();
            UpdateHeight();
        }

        void OnActiveDialogChanged(OpenDialogAction.DialogType newData)
        {
            if (newData != OpenDialogAction.DialogType.None &&
                m_ActiveSubDialogGetter.GetValue() == OpenDialogAction.DialogType.LeftSidebarMore)
            {
                m_DialogButton.selected = false;
                m_DialogWindow.Close();

                IEnumerator WaitAFrame()
                {
                    yield return null;
                    Dispatcher.Dispatch(OpenSubDialogAction.From(OpenDialogAction.DialogType.None));
                }

                StartCoroutine(WaitAFrame());
            }
        }

        void OnActiveSubDialogChanged(OpenDialogAction.DialogType newData)
        {
            m_DialogButton.selected = newData == OpenDialogAction.DialogType.LeftSidebarMore;
        }

        void OnDialogModeChanged(SetDialogModeAction.DialogMode mode)
        {
            if (mode == SetDialogModeAction.DialogMode.Normal)
            {
                if (m_DialogWindow.open)
                {
                    m_DialogButton.selected = true;

                    IEnumerator WaitNextFrame()
                    {
                        yield return null;
                        Dispatcher.Dispatch(OpenSubDialogAction.From(OpenDialogAction.DialogType.LeftSidebarMore));
                    }
                    StartCoroutine(WaitNextFrame());
                }
            }
        }

        void OnMeasureToolStateDataChanged(bool value)
        {
            // Special case for Measure tool because it not associated with a dialog
            if (value && m_DialogButton.selected)
            {
                m_DialogButton.selected = false;
                m_DialogWindow.Close();

                IEnumerator WaitAFrame()
                {
                    yield return null;
                    Dispatcher.Dispatch(OpenSubDialogAction.From(OpenDialogAction.DialogType.None));
                }

                StartCoroutine(WaitAFrame());
            }
        }

        void OnDialogButtonClicked()
        {
            var dialogType = m_DialogWindow.open && m_DialogModeGetter.GetValue() == SetDialogModeAction.DialogMode.Normal ? OpenDialogAction.DialogType.None : OpenDialogAction.DialogType.LeftSidebarMore;
            Dispatcher.Dispatch(OpenSubDialogAction.From(dialogType));
        }

        void UpdatePosition()
        {
            if (m_ButtonPosition != m_DialogButton.transform.parent.position)
            {
                m_ButtonPosition = m_DialogButton.transform.parent.position;
                Vector2 pos = m_Container.position;
                pos.y = m_ButtonPosition.y;
                m_Container.position = pos;

                // Fix a problem when returning from VR
                pos = m_Container.localPosition;
                pos.x = 0;
                m_Container.localPosition = pos;
            }
        }

        void UpdateHeight()
        {
            var availableSpace = m_RectTransform.rect.height - m_ScrollViewParent.position.y/m_ScaleFactorGetter.GetValue();
            var size = m_ScrollViewParent.sizeDelta;
            var contentSizeDelta = m_ScrollView.content.sizeDelta;
            size.y = contentSizeDelta.y > availableSpace ? availableSpace : contentSizeDelta.y;
            m_ScrollViewParent.sizeDelta = size;

            LayoutRebuilder.ForceRebuildLayoutImmediate(m_Container);
        }

        void OnRectTransformDimensionsChange()
        {
            if (m_RectTransform == null)
                return;

            // This is needed because OnRectTransformDimensionChange can be call more than once in the same frame
            IEnumerator UpdateNextFrame()
            {
                yield return null;
                UpdatePosition();
                UpdateHeight();
                m_Coroutine = null;
            }

            if (m_Coroutine == null && gameObject.activeInHierarchy)
            {
                m_Coroutine = StartCoroutine(UpdateNextFrame());
            }
        }
    }
}