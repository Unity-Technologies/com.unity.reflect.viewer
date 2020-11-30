using SharpFlux;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(Button))]
    public class HelpToggleButton : MonoBehaviour
    {
        Button m_Button;
        Image m_ButtonImage;
        DialogMode m_currentDialogMode;

        void Awake()
        {
            m_Button = GetComponent<Button>();
            m_ButtonImage = m_Button.GetComponent<Image>();
            UIStateManager.stateChanged += OnStateDataChanged;
        }

        void Start()
        {
            m_Button.onClick.AddListener(OnButtonClick);
            m_Button.interactable = false;
        }

        void OnStateDataChanged(UIStateData data)
        {
            if (m_currentDialogMode != data.dialogMode)
            {
                m_currentDialogMode = data.dialogMode;
                m_ButtonImage.enabled = data.dialogMode == DialogMode.Help;
            }

            // Currently only support Help Mode in Main (Non AR/VR) screen
            m_Button.interactable = data.activeDialog != DialogType.LandingScreen &&
                data.navigationState.navigationMode != NavigationMode.AR && data.navigationState.navigationMode != NavigationMode.VR;
        }

        void OnButtonClick()
        {
            var dialogMode = UIStateManager.current.stateData.dialogMode;
            dialogMode = (dialogMode == DialogMode.Help) ? DialogMode.Normal : DialogMode.Help;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetDialogMode, dialogMode));
        }
    }
}
