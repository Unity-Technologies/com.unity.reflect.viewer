using System;
using System.Collections;
using System.Collections.Generic;
using SharpFlux;
using SharpFlux.Dispatching;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(Button))]
    public class DialogToggleButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
#pragma warning disable CS0649
        [Serializable]
        struct ToolImage
        {
            [SerializeField]
            public ToolType m_ToolType;
            [SerializeField]
            public Image m_ToolImage;
        }
        [SerializeField]
        DialogType m_DialogType;
        [SerializeField]
        List<ToolImage> m_ToolImages;
#pragma warning restore CS0649

        Button m_Button;
        Image m_ButtonImage;
        Image m_ButtonIcon;
        bool m_Held;
        private Dictionary<ToolType, Image> m_ToolImageDictionary;

        void Awake()
        {
            m_Button = GetComponent<Button>();

            UIStateManager.stateChanged += OnStateDataChanged;

            m_ToolImageDictionary = new Dictionary<ToolType, Image>();
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

        void OnStateDataChanged(UIStateData data)
        {
            if (m_ToolImages.Count == 0)
                return;

            if (m_DialogType == DialogType.None && m_ToolImages[0].m_ToolType == ToolType.None)
                return;

            m_Button.interactable = data.toolbarsEnabled;

            var index = 0;
            var current = 0;
            m_ButtonIcon = m_ToolImages[0].m_ToolImage;
            foreach (var toolImage in m_ToolImages)
            {
                if (m_Button.IsInteractable())
                {
                    toolImage.m_ToolImage.enabled = false;
                }
                if (toolImage.m_ToolType == data.toolState.activeTool)
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

            if (m_DialogType == DialogType.None)
            {
                m_ButtonImage.enabled = data.toolState.activeTool == m_ToolImages[current].m_ToolType;
            }
            else
            {
                m_ButtonImage.enabled = data.activeDialog == m_DialogType || data.toolState.activeTool == m_ToolImages[current].m_ToolType;
            }
        }

        void OnDialogButtonClick()
        {
            var data = UIStateManager.current.stateData;
            if (data.activeDialog != m_DialogType || m_DialogType == DialogType.None)
            {
                var toolState = UIStateManager.current.stateData.toolState;
                toolState.activeTool = data.toolState.activeTool == m_ToolImages[0].m_ToolType?ToolType.None:m_ToolImages[0].m_ToolType;
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, toolState));
            }

            if (m_Held == false)
            {
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.CloseAllDialogs, null));
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
            var data = UIStateManager.current.stateData;
            data.toolState.activeTool = m_ToolImages[0].m_ToolType;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, data.toolState));

            var activeDialog = data.activeDialog == m_DialogType ? DialogType.None : m_DialogType;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, activeDialog));
        }

        private IEnumerator DelayPress(float delay)
        {
            yield return new WaitForSeconds(delay);

            OnLongPress();
        }
    }
}
