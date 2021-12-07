using System;
using SharpFlux.Dispatching;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.UI;

namespace UnityEngine.Reflect.Utils
{
    [RequireComponent(typeof(Button))]
    public class UIDialogButton: MonoBehaviour
    {
        [SerializeField, Tooltip("Dialog Type")]
        OpenDialogAction.DialogType m_DialogType;
        [SerializeField, Tooltip("Is Sub Dialog")]
        bool m_IsSubDialog;
        [SerializeField, Tooltip("Button Image")]
        Image m_ButtonImage;
        [SerializeField, Tooltip("Dialog")]
        DialogWindow m_Dialog;
        [SerializeField, Tooltip("Should the dialog move to the button")]
        bool m_MoveDialogToButton;

        Button m_Button;

        public delegate bool OpenDialogCondition();
        public OpenDialogCondition openDialogCondition = () => true;
        IUISelector<OpenDialogAction.DialogType> m_ActiveSubDialogSelector;
        IUISelector<OpenDialogAction.DialogType> m_ActiveDialogSelector;

        void OnDestroy()
        {
            m_ActiveDialogSelector?.Dispose();
            m_ActiveSubDialogSelector?.Dispose();
        }

        void Awake()
        {
            m_ActiveSubDialogSelector = UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeSubDialog), OnActiveSubDialogChanged);
            m_ActiveDialogSelector = UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeDialog), OnActiveDialogChanged);

            m_Button = GetComponent<Button>();
            m_Button.onClick.AddListener(OnDialogButtonClick);
        }

        void OnActiveDialogChanged(OpenDialogAction.DialogType data)
        {
            if (m_DialogType != OpenDialogAction.DialogType.None)
            {
                if (data == m_DialogType)
                {
                    m_ButtonImage.color = UIConfig.propertySelectedColor;
                }
                else
                {
                    m_ButtonImage.color = Color.clear;
                }
            }
        }

        void OnActiveSubDialogChanged(OpenDialogAction.DialogType data)
        {
            if (m_DialogType != OpenDialogAction.DialogType.None)
            {
                if (m_IsSubDialog && data == m_DialogType)
                {
                    m_ButtonImage.color = UIConfig.propertySelectedColor;
                }
                else
                {
                    m_ButtonImage.color = Color.clear;
                }
            }
        }

        void OnDialogButtonClick()
        {
            if (m_DialogType != OpenDialogAction.DialogType.None)
            {
                var activeDialog = m_IsSubDialog ? m_ActiveSubDialogSelector.GetValue() : m_ActiveDialogSelector.GetValue();
                var dialogType = activeDialog != m_DialogType && openDialogCondition() ? m_DialogType : OpenDialogAction.DialogType.None;

                if (m_IsSubDialog)
                {
                    Dispatcher.Dispatch(OpenSubDialogAction.From(dialogType));
                }
                else
                {
                    Dispatcher.Dispatch(OpenDialogAction.From(dialogType));
                }

                if (!ReferenceEquals(m_Dialog, null) && m_MoveDialogToButton && dialogType != OpenDialogAction.DialogType.None)
                {
                    m_Dialog.transform.position = m_Button.transform.position;
                }
            }
        }
    }
}
