using System;
using System.Collections;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(Button))]
    public class DialogToggleButton: MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
#pragma warning disable CS0649
        [Serializable]
        struct ToolImage
        {
            [SerializeField]
            public SetActiveToolAction.ToolType m_ToolType;
            [SerializeField]
            public Image m_ToolImage;
        }
        [SerializeField]
        OpenDialogAction.DialogType m_DialogType;
        [SerializeField]
        List<ToolImage> m_ToolImages;
#pragma warning restore CS0649

        Button m_Button;
        Image m_ButtonImage;
        Image m_ButtonIcon;
        bool m_Held;
        private Dictionary<SetActiveToolAction.ToolType, Image> m_ToolImageDictionary;
        IUISelector<OpenDialogAction.DialogType> m_ActiveDialogSelector;
        IUISelector<SetActiveToolAction.ToolType> m_ActiveToolSelector;
        IUISelector<bool> m_ToolBarEnabledSelector;
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        void OnDestroy()
        {
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        void Awake()
        {
            m_Button = GetComponent<Button>();

            UIStateContext.current.stateChanged += OnStateDataChanged;

            m_DisposeOnDestroy.Add(m_ActiveDialogSelector = UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeDialog)));
            m_DisposeOnDestroy.Add(m_ActiveToolSelector = UISelectorFactory.createSelector<SetActiveToolAction.ToolType>(ToolStateContext.current, nameof(IToolStateDataProvider.activeTool)));
            m_DisposeOnDestroy.Add(m_ToolBarEnabledSelector = UISelectorFactory.createSelector<bool>(UIStateContext.current, nameof(IToolBarDataProvider.toolbarsEnabled)));

            m_ToolImageDictionary = new Dictionary<SetActiveToolAction.ToolType, Image>();
            ConfigureImages();

            m_ButtonImage = m_Button.GetComponent<Image>();
        }

        private void ConfigureImages()
        {
            foreach (var tooImage in m_ToolImages)
            {
                m_ToolImageDictionary[tooImage.m_ToolType] = tooImage.m_ToolImage;
            }
        }

        void Start()
        {
            m_Button.onClick.AddListener(OnDialogButtonClick);
        }

        void OnStateDataChanged()
        {
            if (m_ToolImages.Count == 0)
                return;

            if (m_DialogType == OpenDialogAction.DialogType.None && m_ToolImages[0].m_ToolType == SetActiveToolAction.ToolType.None)
                return;

            m_Button.interactable = m_ToolBarEnabledSelector.GetValue();

            var index = 0;
            var current = 0;
            m_ButtonIcon = m_ToolImages[0].m_ToolImage;
            foreach (var toolImage in m_ToolImages)
            {
                if (m_Button.IsInteractable())
                {
                    toolImage.m_ToolImage.enabled = false;
                }
                if (toolImage.m_ToolType == m_ActiveToolSelector.GetValue())
                {
                    m_ButtonIcon = toolImage.m_ToolImage;
                    current = index;
                }
                index++;
            }
            if (m_Button.IsInteractable())
            {
                m_ButtonIcon.enabled = true;
            }

            if (m_DialogType == OpenDialogAction.DialogType.None)
            {
                m_ButtonImage.enabled = m_ActiveToolSelector.GetValue() == m_ToolImages[current].m_ToolType;
            }
            else
            {
                m_ButtonImage.enabled = m_ActiveDialogSelector.GetValue() == m_DialogType || m_ActiveToolSelector.GetValue() == m_ToolImages[current].m_ToolType;
            }
        }

        void OnDialogButtonClick()
        {
            if (m_ActiveDialogSelector.GetValue() != m_DialogType || m_DialogType == OpenDialogAction.DialogType.None)
            {
                Dispatcher.Dispatch(SetActiveToolAction.From(m_ActiveToolSelector.GetValue() == m_ToolImages[0].m_ToolType ? SetActiveToolAction.ToolType.None : m_ToolImages[0].m_ToolType));
            }

            if (m_Held == false)
            {
                Dispatcher.Dispatch(CloseAllDialogsAction.From(null));
            }

            m_Held = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            StartCoroutine("DelayPress", UIConfig.buttonLongPressTime);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            StopCoroutine("DelayPress");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            StopCoroutine("DelayPress");
        }

        void OnLongPress()
        {
            m_Held = true;

            Dispatcher.Dispatch(SetActiveToolAction.From(m_ToolImages[0].m_ToolType));

            var activeDialog = m_ActiveDialogSelector.GetValue() == m_DialogType ? OpenDialogAction.DialogType.None : m_DialogType;
            Dispatcher.Dispatch(OpenDialogAction.From(activeDialog));
        }

        private IEnumerator DelayPress(float delay)
        {
            yield return new WaitForSeconds(delay);

            OnLongPress();
        }
    }
}
