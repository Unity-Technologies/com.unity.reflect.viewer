using SharpFlux;
using SharpFlux.Dispatching;
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
        }

        void OnStateDataChanged(UIStateData data)
        {
            m_Button.interactable = data.toolbarsEnabled;

            if (m_currentDialogMode != data.dialogMode)
            {
                m_currentDialogMode = data.dialogMode;
                m_ButtonImage.enabled = data.dialogMode == DialogMode.Help;
            }

            // Currently only support Help Mode in Main (Non AR/VR) screen
            m_Button.transform.parent.gameObject.SetActive(data.activeDialog != DialogType.LandingScreen &&
                data.navigationState.navigationMode != NavigationMode.AR && data.navigationState.navigationMode != NavigationMode.VR);
        }

        void OnButtonClick()
        {
            var dialogMode = UIStateManager.current.stateData.dialogMode;
            dialogMode = (dialogMode == DialogMode.Help) ? DialogMode.Normal : DialogMode.Help;

            // close all (sub)dialogs, and sunstudy dial (a ToolbarType)
            if (dialogMode == DialogMode.Help)
            {
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.None));
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenSubDialog, DialogType.None));
                var activeToolbar = UIStateManager.current.stateData.activeToolbar;
                if (activeToolbar == ToolbarType.TimeOfDayYearDial || activeToolbar == ToolbarType.AltitudeAzimuthDial)
                {
                    Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ClearStatus, null));
                    Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, TimeRadialUIController.m_previousToolbar));
                }

                var measureToolStateData = UIStateManager.current.externalToolStateData.measureToolStateData;
                if (measureToolStateData.toolState)
                {
                    measureToolStateData.toolState = !measureToolStateData.toolState;
                    Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ClearStatus, null));
                    Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetMeasureToolOptions, measureToolStateData));
                }
            }
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetDialogMode, dialogMode));
        }
    }
}
