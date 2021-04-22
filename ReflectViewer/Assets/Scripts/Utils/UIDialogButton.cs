using SharpFlux;
using SharpFlux.Dispatching;
using Unity.Reflect.Viewer.UI;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.Reflect.Utils
{
    [RequireComponent(typeof(Button))]
    public class UIDialogButton : MonoBehaviour
    {

        [SerializeField, Tooltip("Dialog Type")]
        DialogType m_DialogType;
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

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;

            m_Button = GetComponent<Button>();
            m_Button.onClick.AddListener(OnDialogButtonClick);
        }

        void OnStateDataChanged(UIStateData stateData)
        {
            if (m_DialogType != DialogType.None)
            {
                if ((m_IsSubDialog && stateData.activeSubDialog == m_DialogType) || stateData.activeDialog == m_DialogType)
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
            if (m_DialogType != DialogType.None)
            {
                var activeDialog = m_IsSubDialog ? UIStateManager.current.stateData.activeSubDialog : UIStateManager.current.stateData.activeDialog;
                var dialogType = activeDialog != m_DialogType && openDialogCondition() ? m_DialogType : DialogType.None;
                var action = m_IsSubDialog ? ActionTypes.OpenSubDialog : ActionTypes.OpenDialog;
                Dispatcher.Dispatch(Payload<ActionTypes>.From(action, dialogType));
                if (!ReferenceEquals(m_Dialog, null) && m_MoveDialogToButton && dialogType != DialogType.None)
                {
                    m_Dialog.transform.position = m_Button.transform.position;
                }
            }
        }
    }
}
