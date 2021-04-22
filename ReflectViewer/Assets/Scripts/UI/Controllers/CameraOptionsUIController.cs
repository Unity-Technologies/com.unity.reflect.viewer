using System;
using SharpFlux;
using SharpFlux.Dispatching;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(DialogWindow))]
    public class CameraOptionsUIController : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        Button m_DialogButton;
        [SerializeField]
        TMP_Dropdown m_CameraTypeDropdown;
        [SerializeField]
        TMP_Dropdown m_CameraViewDropdown;
        [SerializeField]
        Button m_CameraDefaultViewButton;

        [SerializeField]
        SlideToggle m_JoysticksToggle;
        [SerializeField]
        SegmentedPropertyControl m_JoystickPreferenceSwitch;

        [SerializeField]
        SlideToggle m_NavigationAutoToggle;
        [SerializeField]
        MinMaxPropertyControl m_NavigationSpeedControl;

#pragma warning restore 649

        DialogWindow m_DialogWindow;
        Image m_DialogButtonImage;
        CameraOptionData m_CurrentCameraOptionData;

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;

            m_DialogButtonImage = m_DialogButton.GetComponent<Image>();
            m_DialogWindow = GetComponent<DialogWindow>();
        }

        void Start()
        {
            m_DialogButton.onClick.AddListener(OnDialogButtonClicked);

            m_CameraTypeDropdown.onValueChanged.AddListener(OnCameraTypeChanged);
            m_CameraViewDropdown.onValueChanged.AddListener(OnCameraViewChanged);
            m_CameraDefaultViewButton.onClick.AddListener(OnCameraDefaultViewButtonClicked);

            m_JoysticksToggle.onValueChanged.AddListener(OnJoysticksToggleChanged);
            m_JoystickPreferenceSwitch.onValueChanged.AddListener(OnJoystickPreferenceChanged);

            m_NavigationAutoToggle.onValueChanged.AddListener(OnNavigationAutoToggleChanged);
            m_NavigationSpeedControl.onIntValueChanged.AddListener(OnNavigationSpeedControlChanged);
        }

        void OnStateDataChanged(UIStateData data)
        {
            m_DialogButtonImage.enabled = data.activeDialog == DialogType.CameraOptions;
            m_DialogButton.interactable = data.toolbarsEnabled;

            if (m_CurrentCameraOptionData != data.cameraOptionData)
            {
                int cameraProjectionTypeIndex = 0;
                switch (data.cameraOptionData.cameraProjectionType)
                {
                    case CameraProjectionType.Perspective:
                        cameraProjectionTypeIndex = 0;
                        break;
                    case CameraProjectionType.Orthographic:
                        cameraProjectionTypeIndex = 1;
                        break;
                }
                m_CameraTypeDropdown.SetValueWithoutNotify(cameraProjectionTypeIndex);

                int cameraViewTypeIndex = 0;
                switch (data.cameraOptionData.cameraViewType)
                {
                    case CameraViewType.Default:
                        cameraViewTypeIndex = -1;
                        break;
                    case CameraViewType.Top:
                        cameraViewTypeIndex = 0;
                        break;
                    case CameraViewType.Left:
                        cameraViewTypeIndex = 1;
                        break;
                    case CameraViewType.Right:
                        cameraViewTypeIndex = 2;
                        break;
                }
                if( cameraViewTypeIndex != -1)
                    m_CameraViewDropdown.SetValueWithoutNotify(cameraViewTypeIndex);


                m_JoysticksToggle.on = data.cameraOptionData.enableJoysticks;

                var joystickPreferenceIndex = data.cameraOptionData.joystickPreference == JoystickPreference.RightHanded ? 0 : 1;
                m_JoystickPreferenceSwitch.activePropertyIndex = joystickPreferenceIndex;

                m_NavigationAutoToggle.on = data.cameraOptionData.enableAutoNavigationSpeed;
                m_NavigationSpeedControl.SetValue(data.cameraOptionData.navigationSpeed);

                m_CurrentCameraOptionData = data.cameraOptionData;
            }
        }

        void OnDialogButtonClicked()
        {
            var dialogType = m_DialogWindow.open ? DialogType.None : DialogType.CameraOptions;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, dialogType));
        }


        void OnCameraTypeChanged(int index)
        {
            var data = UIStateManager.current.stateData.cameraOptionData;

            data.cameraProjectionType =
                index == 0 ? CameraProjectionType.Perspective : CameraProjectionType.Orthographic;

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetCameraOption, data));
        }

        void OnCameraViewChanged(int index)
        {
            var data = UIStateManager.current.stateData.cameraOptionData;
            var cameraViewType = CameraViewType.Default;
            if (index == 0)
                cameraViewType = CameraViewType.Top;
            else if (index == 1)
                cameraViewType = CameraViewType.Left;
            else if (index == 2)
                cameraViewType = CameraViewType.Right;

            data.cameraViewType = cameraViewType;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetCameraOption, data));
        }

        void OnCameraDefaultViewButtonClicked()
        {
            var data = UIStateManager.current.stateData.cameraOptionData;
            data.cameraViewType = CameraViewType.Default;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetCameraOption, data));
        }

        void OnJoysticksToggleChanged(bool on)
        {
            var data = UIStateManager.current.stateData.cameraOptionData;
            data.enableJoysticks = on;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetJoystickOption, data));
        }

        void OnJoystickPreferenceChanged(int index)
        {
            var data = UIStateManager.current.stateData.cameraOptionData;
            var rightHanded = index == 0;
            var preference = rightHanded ? JoystickPreference.RightHanded : JoystickPreference.LeftHanded;
            data.joystickPreference = preference;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetJoystickOption, data));
        }

        void OnNavigationAutoToggleChanged(bool on)
        {
            var data = UIStateManager.current.stateData.cameraOptionData;
            data.enableJoysticks = on;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetNavigationOption, data));
        }

        void OnNavigationSpeedControlChanged(int value)
        {
            var data = UIStateManager.current.stateData.cameraOptionData;
            data.navigationSpeed = value;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetNavigationOption, data));
        }
    }
}
